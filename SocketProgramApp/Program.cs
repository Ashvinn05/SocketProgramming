using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketProgramApp
{
    class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new();
        private const int DefaultPort = 5000;
        private const int ConnectionTimeoutMs = 600000; // 5 seconds
        private const int ReadWriteTimeoutMs = 600000; // 10 seconds

        public static async Task Main(string[] args)
        {
            TcpListener? server = null;
            try
            {
                // Get port from environment variable
                string? portStr = Environment.GetEnvironmentVariable("TCP_SERVER_PORT");
                if (!int.TryParse(portStr, out int port) || port < 1024 || port > 65535)
                {
                    port = DefaultPort;
                    Console.WriteLine($"Invalid or missing TCP_SERVER_PORT, using default port {port}");
                }

                // Start server
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                Console.WriteLine($"Server started, listening on port {port}...");

                // Allow cancellation via Ctrl+C
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLine("Shutting down server...");
                    cancellationTokenSource.Cancel();
                    server.Stop();
                    e.Cancel = true;
                };

                // Accept clients asynchronously
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
                        cts.CancelAfter(ConnectionTimeoutMs);
                        TcpClient client = await server.AcceptTcpClientAsync().WithCancellation(cts.Token).ConfigureAwait(false);
                        Console.WriteLine("Client connected!");

                        // Configure timeouts
                        client.ReceiveTimeout = ReadWriteTimeoutMs;
                        client.SendTimeout = ReadWriteTimeoutMs;

                        // Handle client in a separate task
                        _ = Task.Run(() => HandleClientAsync(client, cancellationTokenSource.Token), cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Socket error accepting client: {ex.SocketErrorCode}, {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accepting client: {ex.Message}");
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Server socket error: {ex.SocketErrorCode}, {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                server?.Stop();
                Console.WriteLine("Server stopped.");
            }
        }

        private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                try
                {
                    using NetworkStream stream = client.GetStream();
                    byte[] lengthBuffer = new byte[4];
                    byte[] dataBuffer = new byte[1024];

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Read length prefix
                            int bytesRead = await ReadExactAsync(stream, lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);
                            if (bytesRead == 0)
                            {
                                break; // Client disconnected
                            }

                            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                            if (messageLength <= 0 || messageLength > 1024 * 1024) // Cap at 1MB
                            {
                                Console.WriteLine("Invalid message length received.");
                                break;
                            }

                            // Read message
                            byte[] messageBuffer = new byte[messageLength];
                            bytesRead = await ReadExactAsync(stream, messageBuffer, 0, messageLength, cancellationToken).ConfigureAwait(false);
                            if (bytesRead == 0)
                            {
                                break; // Client disconnected
                            }

                            string message = Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
                            Console.WriteLine($"Received from client: {message}");

                            // Send response
                            string response = $"Received: {message}";
                            await SendMessageAsync(stream, response, cancellationToken).ConfigureAwait(false);
                            Console.WriteLine($"Sent to client: {response}");
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine($"Client socket error: {ex.SocketErrorCode}, {ex.Message}");
                            break;
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"Client IO error: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error handling client: {ex.Message}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client error: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("Client disconnected.");
                }
            }
        }

        private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ReadWriteTimeoutMs);
                int bytesRead = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cts.Token).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    return totalRead; // Client disconnected
                }
                totalRead += bytesRead;
            }
            return totalRead;
        }

        private static async Task SendMessageAsync(NetworkStream stream, string message, CancellationToken cancellationToken)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            byte[] data = new byte[4 + messageBytes.Length];
            Array.Copy(lengthBytes, 0, data, 0, 4);
            Array.Copy(messageBytes, 0, data, 4, messageBytes.Length);

            int totalSent = 0;
            while (totalSent < data.Length)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ReadWriteTimeoutMs);
                int toSend = data.Length - totalSent;
                await stream.WriteAsync(data, totalSent, toSend, cts.Token).ConfigureAwait(false);
                totalSent += toSend;
            }
        }
    }

    // Extension method for cancellation
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            return await task;
        }
    }
}