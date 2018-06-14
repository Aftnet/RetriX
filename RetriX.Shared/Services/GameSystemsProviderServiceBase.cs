using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.StreamProviders;
using RetriX.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public abstract class GameSystemsProviderServiceBase : IGameSystemsProviderService
    {
        protected abstract IReadOnlyList<GameSystemViewModel> GenerateSystemsList(IFileSystem fileSystem);

        private IFileSystem FileSystem { get; }

        protected Lazy<IReadOnlyList<GameSystemViewModel>> systems { get; }
        public IReadOnlyList<GameSystemViewModel> Systems => systems.Value;

        public GameSystemsProviderServiceBase(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;

            systems = new Lazy<IReadOnlyList<GameSystemViewModel>>(() => GenerateSystemsList(FileSystem), LazyThreadSafetyMode.PublicationOnly);
        }

        public Task<IReadOnlyList<GameSystemViewModel>> GetCompatibleSystems(IFileInfo file)
        {
            IReadOnlyList<GameSystemViewModel> output = new GameSystemViewModel[0];
            if (file == null)
            {
                return Task.FromResult(output);
            }

            output = Systems.Where(d => d.SupportedExtensions.Contains(Path.GetExtension(file.Name))).ToArray();
            return Task.FromResult(output);
        }

        public async Task<(GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult)> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder)
        {
            var dependenciesMet = await system.CheckDependenciesMetAsync();
            if (!dependenciesMet)
            {
                return (default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.DependenciesUnmet);
            }

            if (system.CheckRootFolderRequired(file) && rootFolder == null)
            {
                return (default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.RootFolderRequired);
            }

            var vfsRomPath = "ROM";
            var vfsSystemPath = "System";
            var vfsSavePath = "Save";

            var core = system.Core;

            string virtualMainFilePath = null;
            var provider = default(IStreamProvider);
            if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(file.Name)) && core.NativeArchiveSupport == false)
            {
                var archiveProvider = new ArchiveStreamProvider(vfsRomPath, file);
                provider = archiveProvider;
                var entries = await provider.ListEntriesAsync();
                virtualMainFilePath = entries.FirstOrDefault(d => system.SupportedExtensions.Contains(Path.GetExtension(d)));
                if (string.IsNullOrEmpty(virtualMainFilePath))
                {
                    return (default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.NoMainFileFound);
                }
            }
            else
            {
                virtualMainFilePath = Path.Combine(vfsRomPath, file.Name);
                provider = new SingleFileStreamProvider(virtualMainFilePath, file);
                if (rootFolder != null)
                {
                    virtualMainFilePath = file.FullName.Substring(rootFolder.FullName.Length + 1);
                    virtualMainFilePath = Path.Combine(vfsRomPath, virtualMainFilePath);
                    provider = new FolderStreamProvider(vfsRomPath, rootFolder);
                }
            }

            var systemFolder = await system.GetSystemDirectoryAsync();
            var systemProvider = new FolderStreamProvider(vfsSystemPath, systemFolder);
            core.SystemRootPath = vfsSystemPath;
            var saveFolder = await system.GetSaveDirectoryAsync();
            var saveProvider = new FolderStreamProvider(vfsSavePath, saveFolder);
            core.SaveRootPath = vfsSavePath;

            provider = new CombinedStreamProvider(new HashSet<IStreamProvider>() { provider, systemProvider, saveProvider });

            return (new GameLaunchEnvironment(core, provider, virtualMainFilePath), GameLaunchEnvironment.GenerateResult.Success);
        }
    }
}
