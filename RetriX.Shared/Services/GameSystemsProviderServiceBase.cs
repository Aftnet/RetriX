using Plugin.FileSystem.Abstractions;
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

        public Task<GamePlayerViewModel.Parameter> GenerateGameLaunchEnvironmentAsync(IFileInfo file)
        {
            var extension = Path.GetExtension(file.Name);
            var compatibleSystems = Systems.Where(d => d.SupportedExtensions.Contains(extension)).ToArray();
            if (compatibleSystems.Length != 1)
            {
                return Task.FromResult(default(GamePlayerViewModel.Parameter));
            }

            return GenerateGameLaunchEnvironmentAsync(compatibleSystems.First(), file, null);
        }

        public async Task<GamePlayerViewModel.Parameter> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder)
        {
            var dependenciesMet = await system.CheckDependenciesMetAsync();
            if (!dependenciesMet || (system.CheckRootFolderRequired(file) && rootFolder == null))
            {
                return null;
            }

            var vfsRomPath = "ROM";
            var vfsSystemPath = "System";
            var vfsSavePath = "Save";

            var core = system.Core;

            string virtualMainFilePath = null;
            var provider = default(IStreamProvider);

            if (core.NativeArchiveSupport || !ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(file.Name)))
            {
                virtualMainFilePath = $"{vfsRomPath}{Path.DirectorySeparatorChar}{file.Name}";
                provider = new SingleFileStreamProvider(virtualMainFilePath, file);
                if (rootFolder != null)
                {
                    virtualMainFilePath = file.FullName.Substring(rootFolder.FullName.Length + 1);
                    virtualMainFilePath = $"{vfsRomPath}{Path.DirectorySeparatorChar}{virtualMainFilePath}";
                    provider = new FolderStreamProvider(vfsRomPath, rootFolder);
                }
            }
            else
            {
                var archiveProvider = new ArchiveStreamProvider(vfsRomPath, file);
                await archiveProvider.InitializeAsync();
                provider = archiveProvider;
                var entries = await provider.ListEntriesAsync();
                virtualMainFilePath = entries.FirstOrDefault(d => system.SupportedExtensions.Contains(Path.GetExtension(d)));
            }

            var systemFolder = await system.GetSystemDirectoryAsync();
            var systemProvider = new FolderStreamProvider(vfsSystemPath, systemFolder);
            core.SystemRootPath = vfsSystemPath;
            var saveFolder = await system.GetSaveDirectoryAsync();
            var saveProvider = new FolderStreamProvider(vfsSavePath, saveFolder);
            core.SaveRootPath = vfsSavePath;

            provider = new CombinedStreamProvider(new HashSet<IStreamProvider>() { provider, systemProvider, saveProvider });

            return new GamePlayerViewModel.Parameter(core, provider, virtualMainFilePath);
        }
    }
}
