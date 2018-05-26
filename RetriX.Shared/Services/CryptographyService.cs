using Plugin.FileSystem.Abstractions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class CryptographyService : ICryptographyService
    {
        public async Task<string> ComputeMD5Async(IFileInfo file)
        {
            using (var inputStream = await file.OpenAsync(FileAccess.Read))
            using (var hasher = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
            {
                var buffer = new byte[1024 * 1024];
                while (inputStream.Position < inputStream.Length)
                {
                    var bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                    hasher.AppendData(buffer, 0, bytesRead);
                }

                var hashBytes = hasher.GetHashAndReset();
                var hashString = BitConverter.ToString(hashBytes);
                return hashString.Replace("-", string.Empty);
            }            
        }
    }
}
