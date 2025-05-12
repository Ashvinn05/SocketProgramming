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
        public static async Task<string?> ReadMessageAsync(NetworkStream stream, CancellationToken cancellationToken = default)
        {
            // Read length prefix
            byte[] lengthPrefix = new byte[4];
            int read = await stream.ReadAsync(lengthPrefix, 0, 4, cancellationToken);
            if (read < 4) return null;
            int messageLength = BitConverter.ToInt32(lengthPrefix, 0);
            // Read encrypted message
            byte[] encryptedBytes = new byte[messageLength];
            int totalRead = 0;
            while (totalRead < messageLength)
            {
                int bytesRead = await stream.ReadAsync(encryptedBytes, totalRead, messageLength - totalRead, cancellationToken);
                if (bytesRead == 0) return null; // Disconnected
                totalRead += bytesRead;
            }
            // Decrypt
            return EncryptionUtils.Decrypt(encryptedBytes);
        }

        public static async Task SendMessageAsync(NetworkStream stream, string message, CancellationToken cancellationToken = default)
        {
            // Encrypt message
            byte[] encryptedBytes = EncryptionUtils.Encrypt(message);
            // Send length prefix
            byte[] lengthPrefix = BitConverter.GetBytes(encryptedBytes.Length);
            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length, cancellationToken);
            // Send encrypted message
            await stream.WriteAsync(encryptedBytes, 0, encryptedBytes.Length, cancellationToken);
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