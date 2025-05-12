namespace SocketProgramApp.Networking
{
    public interface ITcpServer
    {
        Task StartAsync(CancellationToken cancellationToken);
        void Stop();
    }
}