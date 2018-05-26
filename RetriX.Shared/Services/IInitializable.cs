using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public interface IInitializable
    {
        Task InitAsync();
        Task DeinitAsync();
    }
}
