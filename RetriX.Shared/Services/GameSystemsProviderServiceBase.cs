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
        protected abstract IEnumerable<GameSystemViewModel> GenerateSystemsList(IFileSystem fileSystem);

        private IFileSystem FileSystem { get; }

        protected Lazy<IEnumerable<GameSystemViewModel>> systems { get; }
        public IEnumerable<GameSystemViewModel> Systems => systems.Value;

        public GameSystemsProviderServiceBase(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;

            systems = new Lazy<IEnumerable<GameSystemViewModel>>(() => GenerateSystemsList(FileSystem).ToArray(), LazyThreadSafetyMode.PublicationOnly);
        }

        public async Task<IReadOnlyList<GameSystemViewModel>> GetCompatibleSystems(IFileInfo file)
        {
            if (file == null)
            {
                return new GameSystemViewModel[0];
            }

            var output = new HashSet<GameSystemViewModel>();
            bool shouldAddNativelySupportingSystems = true;
            if (ArchiveStreamProvider.SupportedExtensions.Contains(Path.GetExtension(file.Name)))
            {
                IEnumerable<string> entries;
                using (var provider = new ArchiveStreamProvider($"test{Path.DirectorySeparatorChar}", file))
                {
                    entries = await provider.ListEntriesAsync();
                }

                await Task.Run(() =>
                {
                    var entriesExtensions = new HashSet<string>(entries.Select(d => Path.GetExtension(d)));
                    foreach (var i in Systems)
                    {
                        foreach (var j in entriesExtensions)
                        {
                            if (i.SupportedExtensions.Contains(j))
                            {
                                output.Add(i);
                            }
                        }
                    }

                    //One extension in archive and one compatible core found. Skip adding systems natively supporting archives.
                    if (entriesExtensions.Count == 1 && output.Any())
                    {
                        shouldAddNativelySupportingSystems = false;
                    }
                });
            }

            if (shouldAddNativelySupportingSystems)
            {
                var nativelySupportingSystems = Systems.Where(d => d.SupportedExtensions.Contains(Path.GetExtension(file.Name))).ToArray();
                foreach (var i in nativelySupportingSystems)
                {
                    output.Add(i);
                }
            }

            return output.OrderBy(d => d.Name).ToList();
        }

        public async Task<Tuple<GameLaunchEnvironment, GameLaunchEnvironment.GenerateResult>> GenerateGameLaunchEnvironmentAsync(GameSystemViewModel system, IFileInfo file, IDirectoryInfo rootFolder)
        {
            var dependenciesMet = await system.CheckDependenciesMetAsync();
            if (!dependenciesMet)
            {
                return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.DependenciesUnmet);
            }

            if (system.CheckRootFolderRequired(file) && rootFolder == null)
            {
                return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.RootFolderRequired);
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
                    return Tuple.Create(default(GameLaunchEnvironment), GameLaunchEnvironment.GenerateResult.NoMainFileFound);
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

            return Tuple.Create(new GameLaunchEnvironment(core, provider, virtualMainFilePath), GameLaunchEnvironment.GenerateResult.Success);
        }
    }
}
