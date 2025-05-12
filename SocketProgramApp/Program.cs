using System;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Networking;

namespace SocketProgramApp
{
    class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new();
        private const int DefaultPort = 5000;

        public static async Task Main(string[] args)
        {
            int port = DefaultPort;
            // (Optional: parse port from env/args)

            ITcpServer server = new TcpServer(port);

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutting down server...");
                cancellationTokenSource.Cancel();
                server.Stop();
                e.Cancel = true;
            };

            await server.StartAsync(cancellationTokenSource.Token);
        }
    }
}