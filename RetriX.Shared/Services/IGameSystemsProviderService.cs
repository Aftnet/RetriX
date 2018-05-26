using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.ViewModels;

namespace RetriX.Shared.Services
{
    public interface IGameSystemsProviderService
    {
        IReadOnlyList<GameSystemViewModel> Systems { get; }

        Task<GamePlayerViewModel.Parameter> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder);
        Task<GamePlayerViewModel.Parameter> GenerateGameLaunchEnvironmentAsync(IFileInfo file);
    }
}