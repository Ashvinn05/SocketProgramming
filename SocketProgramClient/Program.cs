using System;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramClient.Networking;

namespace SocketProgramClient
{
    class Program
    {
        private static readonly CancellationTokenSource cancellationTokenSource = new();
        private const int DefaultPort = 5000;
        private const string DefaultHost = "localhost";

        public static async Task Main(string[] args)
        {
            int port = DefaultPort;
            string host = DefaultHost;
            // (Optional: parse host/port from env/args)

            ITcpClientService clientService = new TcpClientService(host, port);

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutting down client...");
                cancellationTokenSource.Cancel();
                e.Cancel = true;
            };

            await clientService.RunAsync(cancellationTokenSource.Token);
        }
    }
}