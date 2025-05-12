using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Utils;

namespace SocketProgramApp.Networking
{
    public class TcpServer : ITcpServer
    {
        private readonly int _port;
        private TcpListener? _server;
        private readonly IClientHandler _clientHandler;

        public TcpServer(int port, IClientHandler? clientHandler = null)
        {
            _port = port;
            _clientHandler = clientHandler ?? new ClientHandler();
        }

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

        public void Stop()
        {
            _server?.Stop();
        }
    }
}