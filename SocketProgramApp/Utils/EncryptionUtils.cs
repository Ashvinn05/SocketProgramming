using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SocketProgramApp.Utils
{
    public static class EncryptionUtils
    {
        // Read from environment variables (base64-encoded), fallback to demo values
        private static readonly byte[] Key = Convert.FromBase64String(
            Environment.GetEnvironmentVariable("SOCKET_AES_KEY") ?? Convert.ToBase64String(Encoding.UTF8.GetBytes("0123456789abcdef0123456789abcdef"))
        );
        private static readonly byte[] IV = Convert.FromBase64String(
            Environment.GetEnvironmentVariable("SOCKET_AES_IV") ?? Convert.ToBase64String(Encoding.UTF8.GetBytes("abcdef9876543210"))
        );

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