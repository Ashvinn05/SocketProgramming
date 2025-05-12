namespace SocketProgramClient.Networking
{
    public interface ITcpClientService
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}