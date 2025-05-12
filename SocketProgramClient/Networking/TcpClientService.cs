using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramClient.Utils;
using SocketProgramClient.Networking;

namespace SocketProgramClient.Networking
{
    public class TcpClientService : ITcpClientService
    {
        private readonly string _host;
        private readonly int _port;
        private const int MaxConnectionRetries = 3;
        private const int RetryDelayMs = 2000;
        private const int ConnectionTimeoutMs = 600000;

        public TcpClientService(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            int attempts = 0;
            while (attempts < MaxConnectionRetries && !cancellationToken.IsCancellationRequested)
            {
                TcpClient? client = null;
                NetworkStream? stream = null;
                try
                {
                    client = new TcpClient();
                    using var cts = new CancellationTokenSource(ConnectionTimeoutMs);
                    await client.ConnectAsync(_host, _port);
                    stream = client.GetStream();
                    Console.WriteLine($"Connected to server at {_host}:{_port}!");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Enter a message to send (or 'quit' to exit):");
                        string? message = Console.ReadLine();
                        if (string.IsNullOrEmpty(message) || message.ToLower() == "quit")
                        {
                            Console.WriteLine("Disconnecting.");
                            break;
                        }
                        await MessageUtils.SendMessageAsync(stream, message, cancellationToken);
                        Console.WriteLine($"Sent: {message}");
                        // Read and display all responses until the server stops sending (timeout or disconnect)
                        while (true)
                        {
                            try
                            {
                                // Set a short timeout for reading the next message
                                using var readCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                                readCts.CancelAfter(1500); // 1.5 seconds: must be longer than server's 1s interval
                                string? response = await MessageUtils.ReadMessageAsync(stream, readCts.Token);
                                if (string.IsNullOrEmpty(response))
                                    break;
                                Console.WriteLine($"Received: {response}");
                                // Optionally, break if response is "EMPTY"
                            }
                            catch (OperationCanceledException)
                            {
                                // Timeout: no more messages expected
                                break;
                            }
                        }
                    }
                    break;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Connection timed out or cancelled.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    attempts++;
                    if (attempts < MaxConnectionRetries)
                    {
                        Console.WriteLine($"Retrying... (Attempt {attempts + 1}/{MaxConnectionRetries})");
                        await Task.Delay(RetryDelayMs, cancellationToken);
                    }
                    else
                    {
                        Console.WriteLine("Max connection retries reached. Exiting.");
                    }
                }
                finally
                {
                    stream?.Dispose();
                    client?.Dispose();
                }
            }
        }
    }
}