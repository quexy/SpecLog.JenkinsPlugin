using System;
using System.Linq;
using System.Text;

namespace SpecLog.JenkinsPlugin.Client
{
    static class CryptoService
    {
        private const string secret = "In the beginning, you lived inside the Egg. That is where Crake made you.";
        private static readonly byte[] magic = Encoding.UTF8.GetBytes(secret);

        public static string Encrypt(string value)
        {
            return Convert.ToBase64String(XorArray(Encoding.UTF8.GetBytes(value)));
        }

        public static string Decrypt(string value)
        {
            return Encoding.UTF8.GetString(XorArray(Convert.FromBase64String(value)));
        }

        private static byte[] XorArray(byte[] array)
        {
            var other = new byte[0];
            while (other.Length < array.Length)
                other = other.Concat(magic).ToArray();
            return array.Select((v, i) => v ^ other[i])
                .Select(i => (byte)i).ToArray();
        }
    }
}
