using Plugin.FileSystem.Abstractions;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public interface ICryptographyService
    {
        Task<string> ComputeMD5Async(IFileInfo file);
    }
}
