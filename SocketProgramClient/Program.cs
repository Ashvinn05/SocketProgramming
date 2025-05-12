using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketProgramClient
{
    class Program
    {
        private const int DefaultPort = 5000;
        private const int ConnectionTimeoutMs = 600000; // 10 minutes
        private const int ReadWriteTimeoutMs = 600000; // 10 minutes
        private const int MaxConnectionRetries = 3;
        private const int RetryDelayMs = 2000; // 2 seconds

        public static async Task Main(string[] args)
        {
            // Get port from environment variable
            string? portStr = Environment.GetEnvironmentVariable("TCP_SERVER_PORT");
            if (!int.TryParse(portStr, out int port) || port < 1024 || port > 65535)
            {
                port = DefaultPort;
                Console.WriteLine($"Invalid or missing TCP_SERVER_PORT, using default port {port}");
            }

            while (true)
            {
                bool connected = false;
                int attempts = 0;
                TcpClient? client = null;
                NetworkStream? stream = null;

                // Attempt to connect
                while (!connected && attempts < MaxConnectionRetries)
                {
                    try
                    {
                        client = new TcpClient();
                        client.ReceiveTimeout = ReadWriteTimeoutMs;
                        client.SendTimeout = ReadWriteTimeoutMs;

                        Console.WriteLine($"Connecting to localhost:{port}...");
                        using var cts = new CancellationTokenSource(ConnectionTimeoutMs);
                        await client.ConnectAsync("localhost", port).WithCancellation(cts.Token);
                        stream = client.GetStream();
                        Console.WriteLine("Connected to server!");
                        connected = true;
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Connection timed out.");
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Socket error: {ex.SocketErrorCode}, {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    if (!connected)
                    {
                        client?.Dispose();
                        attempts++;
                        if (attempts < MaxConnectionRetries)
                        {
                            Console.WriteLine($"Retrying connection... (Attempt {attempts + 1}/{MaxConnectionRetries})");
                            await Task.Delay(RetryDelayMs);
                        }
                        else
                        {
                            Console.WriteLine("Max connection retries reached. Exiting.");
                            return;
                        }
                    }
                }

                // Message loop
                try
                {
                    while (connected)
                    {
                        Console.WriteLine("Enter a message to send (or 'quit' to exit):");
                        string? message = Console.ReadLine();
                        if (string.IsNullOrEmpty(message) || message.ToLower() == "quit")
                        {
                            Console.WriteLine("Disconnecting.");
                            break;
                        }

                        try
                        {
                            // Send message
                            await SendMessageAsync(stream!, message, CancellationToken.None);
                            Console.WriteLine($"Sent: {message}");

                            // Receive response
                            string response = await ReceiveMessageAsync(stream!, CancellationToken.None);
                            Console.WriteLine($"Received: {response}");
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"Connection error: {ex.Message}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error: {ex.Message}");
                            break;
                        }
                    }
                }
                finally
                {
                    stream?.Dispose();
                    client?.Dispose();
                    Console.WriteLine("Connection closed.");
                }
            }
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

        private static async Task<string> ReceiveMessageAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await ReadExactAsync(stream, lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                throw new IOException("Server disconnected.");
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (messageLength <= 0 || messageLength > 1024 * 1024)
            {
                throw new IOException("Invalid message length received.");
            }

            byte[] messageBuffer = new byte[messageLength];
            bytesRead = await ReadExactAsync(stream, messageBuffer, 0, messageLength, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                throw new IOException("Server disconnected.");
            }

            return Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
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
                    return totalRead; // Server disconnected
                }
                totalRead += bytesRead;
            }
            return totalRead;
        }
    }

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

        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            await task;
        }
    }
}