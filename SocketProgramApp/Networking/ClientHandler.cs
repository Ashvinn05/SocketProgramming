using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Utils;
using SocketProgramApp.BusinessLogic;

/// <summary>
/// Handles client connections and processes requests asynchronously.
/// </summary>
namespace SocketProgramApp.Networking
{
    /// <summary>
    /// Represents a client handler that implements the IClientHandler interface.
    /// </summary>
    public class ClientHandler : IClientHandler
    {
        /// <summary>
        /// Asynchronously handles the client connection and processes incoming messages.
        /// </summary>
        /// <param name="client">The TcpClient representing the client connection.</param>
        /// <param name="cancellationToken">Cancellation token to signal cancellation of the operation.</param>
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