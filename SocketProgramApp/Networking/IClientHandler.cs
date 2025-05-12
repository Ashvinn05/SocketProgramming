namespace SocketProgramApp.Networking
{
    public interface IClientHandler
    {
        Task HandleClientAsync(System.Net.Sockets.TcpClient client, CancellationToken cancellationToken);
    }
}