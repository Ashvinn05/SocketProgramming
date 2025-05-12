using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketProgramClient.Utils
{
    public static class MessageUtils
    {
        public static async Task<string?> ReadMessageAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await ReadExactAsync(stream, lengthBuffer, 0, 4, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                return null; // Disconnected

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            if (messageLength <= 0 || messageLength > 1024 * 1024)
                throw new IOException("Invalid message length received.");

            byte[] messageBuffer = new byte[messageLength];
            bytesRead = await ReadExactAsync(stream, messageBuffer, 0, messageLength, cancellationToken).ConfigureAwait(false);
            if (bytesRead == 0)
                return null; // Disconnected

            return Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
        }

        public static async Task SendMessageAsync(NetworkStream stream, string message, CancellationToken cancellationToken)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
            byte[] data = new byte[4 + messageBytes.Length];
            Array.Copy(lengthBytes, 0, data, 0, 4);
            Array.Copy(messageBytes, 0, data, 4, messageBytes.Length);

            await stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
        }

        private static async Task<int> ReadExactAsync(NetworkStream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int bytesRead = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                    return totalRead; // Disconnected
                totalRead += bytesRead;
            }
            return totalRead;
        }
    }
}