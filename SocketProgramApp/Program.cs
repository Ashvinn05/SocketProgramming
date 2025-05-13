using System;
using System.Threading;
using System.Threading.Tasks;
using SocketProgramApp.Networking;

/// <summary>
/// The main entry point for the SocketProgramApp application.
/// </summary>
namespace SocketProgramApp
{
    /// <summary>
    /// The main program class that contains the entry point for the application.
    /// </summary>
    class Program
    {
        /// <summary>
        /// A cancellation token source used to cancel the TCP server operation.
        /// </summary>
        private static readonly CancellationTokenSource cancellationTokenSource = new();

        /// <summary>
        /// The default port number used by the TCP server.
        /// </summary>
        private const int DefaultPort = 5000;

        /// <summary>
        /// The main asynchronous method that starts the TCP server.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
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