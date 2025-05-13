using System;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramClient.Networking;

/// <summary>
/// The main entry point for the SocketProgramClient application.
/// </summary>
namespace SocketProgramClient
{
    /// <summary>
    /// The main program class that contains the entry point for the application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// A cancellation token source used to cancel the TCP client service.
        /// </summary>
        private static readonly CancellationTokenSource cancellationTokenSource = new();

        /// <summary>
        /// The default port number used for the TCP client service.
        /// </summary>
        private const int DefaultPort = 5000;

        /// <summary>
        /// The default host name used for the TCP client service.
        /// </summary>
        private const string DefaultHost = "localhost";

        /// <summary>
        /// The main asynchronous method that starts the TCP client service.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
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