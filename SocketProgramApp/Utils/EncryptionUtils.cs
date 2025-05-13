using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SocketProgramApp.Utils
{
    /// <summary>
    /// Provides static methods for encrypting and decrypting data using AES encryption.
    /// </summary>
    public static class EncryptionUtils
    {
        // Read from environment variables (base64-encoded), fallback to demo values
        /// <summary>
        /// The encryption key used for AES encryption, read from environment variable or fallback to a default value.
        /// </summary>
        private static readonly byte[] Key = Convert.FromBase64String(
            Environment.GetEnvironmentVariable("SOCKET_AES_KEY") ?? Convert.ToBase64String(Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"))
        );

        /// <summary>
        /// The initialization vector used for AES encryption, read from environment variable or fallback to a default value.
        /// </summary>
        private static readonly byte[] IV = Convert.FromBase64String(
            Environment.GetEnvironmentVariable("SOCKET_AES_IV") ?? Convert.ToBase64String(Encoding.UTF8.GetBytes("abcdef9876543210"))
        );

        /// <summary>
        /// Encrypts the specified plain text using AES encryption.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <returns>The encrypted byte array.</returns>
        public static byte[] Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts the specified byte array using AES decryption.
        /// </summary>
        /// <param name="cipherBytes">The encrypted byte array to decrypt.</param>
        /// <returns>The decrypted plain text string.</returns>
        public static string Decrypt(byte[] cipherBytes)
        {
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}