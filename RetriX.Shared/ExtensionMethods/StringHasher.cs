using System;
using System.Security.Cryptography;
using System.Text;

namespace RetriX.Shared.ExtensionMethods
{
    public static class StringHasher
    {
        private static readonly UTF8Encoding Encoder = new UTF8Encoding();

        public static string MD5(this string input)
        {
            return HashString(input, HashAlgorithmName.MD5);
        }

        public static string SHA1(this string input)
        {
            return HashString(input, HashAlgorithmName.SHA1);
        }

        private static string HashString(string input, HashAlgorithmName algorithmName)
        {
            using (var hasher = IncrementalHash.CreateHash(algorithmName))
            {
                var bytes = Encoder.GetBytes(input);
                hasher.AppendData(bytes);
                bytes = hasher.GetHashAndReset();
                var hash = BitConverter.ToString(bytes);
                return hash.Replace("-", string.Empty).ToLower();
            }
        }
    }
}
