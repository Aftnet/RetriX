using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.ViewModels;

namespace RetriX.Shared.Services
{
    public interface IGameSystemsProviderService
    {
        IReadOnlyList<GameSystemViewModel> Systems { get; }

        Task<IReadOnlyList<GameSystemViewModel>> GetCompatibleSystems(IFileInfo file);
        Task<(GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult)> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder);
    }
}