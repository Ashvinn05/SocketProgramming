using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Utils;

namespace SocketProgramApp.Networking
{
    public class ClientHandler : IClientHandler
    {
        public async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                try
                {
                    using NetworkStream stream = client.GetStream();
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            string? message = await MessageUtils.ReadMessageAsync(stream, cancellationToken);
                            if (message == null)
                                break;

                            Console.WriteLine($"Received from client: {message}");

                            string response = $"Received: {message}";
                            await MessageUtils.SendMessageAsync(stream, response, cancellationToken);
                            Console.WriteLine($"Sent to client: {response}");
                        }
                        catch (OperationCanceledException)
                        {
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
    }
}