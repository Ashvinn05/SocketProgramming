using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Utils;

namespace SocketProgramApp.Networking
{
    /// <summary>
    /// Implements the ITcpServer interface for managing TCP connections.
    /// </summary>
    public class TcpServer : ITcpServer
    {
        private readonly int _port;
        private TcpListener? _server;
        private readonly IClientHandler _clientHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        /// <param name="port">The port number on which the server will listen.</param>
        /// <param name="clientHandler">An optional client handler to manage client connections.</param>
        public TcpServer(int port, IClientHandler? clientHandler = null)
        {
            _port = port;
            _clientHandler = clientHandler ?? new ClientHandler();
        }

        /// <summary>
        /// Starts the TCP server and begins accepting client connections asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to signal cancellation of the operation.</param>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();
            Console.WriteLine($"Server started, listening on port {_port}...");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    TcpClient client = await _server.AcceptTcpClientAsync().WithCancellation(cancellationToken);
                    Console.WriteLine("Client connected!");
                    _ = Task.Run(() => _clientHandler.HandleClientAsync(client, cancellationToken), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }
            finally
            {
                _server.Stop();
                Console.WriteLine("Server stopped.");
            }
        }

        /// <summary>
        /// Stops the TCP server.
        /// </summary>
        public void Stop()
        {
            _server?.Stop();
        }
    }
}