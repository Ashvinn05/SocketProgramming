using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Utils;
using SocketProgramApp.BusinessLogic;

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
                        // Read message from client
                        string? request = await MessageUtils.ReadMessageAsync(stream, cancellationToken);
                        if (string.IsNullOrWhiteSpace(request) || !InputSanitizer.IsValidFormat(request))
                        {
                            await MessageUtils.SendMessageAsync(stream, "EMPTY", cancellationToken);
                            continue;
                        }
                        request = request.Trim();
                        int? count = TimeResponseService.GetTimeResponseCount(request);
                        if (count.HasValue && count.Value > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                string currentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                                await MessageUtils.SendMessageAsync(stream, currentTime, cancellationToken);
                                await Task.Delay(1000, cancellationToken);
                            }
                        }
                        else
                        {
                            await MessageUtils.SendMessageAsync(stream, "EMPTY", cancellationToken);
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