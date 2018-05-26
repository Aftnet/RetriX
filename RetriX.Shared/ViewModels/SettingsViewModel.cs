using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class SettingsViewModel : MvxViewModel
    {
        private IGameSystemsProviderService GameSystemsProvider { get; }
        private IFileSystem FileSystem { get; }
        private IUserDialogs DialogsService { get; }
        private IPlatformService PlatformService { get; }
        private ICryptographyService CryptographyService { get; }

        private IReadOnlyList<FileImporterViewModel> fileDependencyImporters;
        public IReadOnlyList<FileImporterViewModel> FileDependencyImporters
        {
            get => fileDependencyImporters;
            private set => SetProperty(ref fileDependencyImporters, value);
        }

        public SettingsViewModel(IGameSystemsProviderService gameSystemsProvider, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService)
        {
            GameSystemsProvider = gameSystemsProvider;
            FileSystem = fileSystem;
            DialogsService = dialogsService;
            PlatformService = platformService;
            CryptographyService = cryptographyService;

            Task.Run(GetFileDependencyImportersAsync).ContinueWith(d => FileDependencyImporters = d.Result, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task<List<FileImporterViewModel>> GetFileDependencyImportersAsync()
        {
            var importers = new List<FileImporterViewModel>();
            var distinctCores = new HashSet<ICore>();
            foreach (var i in GameSystemsProvider.Systems)
            {
                var core = i.Core;
                if (distinctCores.Contains(core))
                {
                    continue;
                }

                distinctCores.Add(core);
                var systemFolder = await i.GetSystemDirectoryAsync();
                var tasks = core.FileDependencies.Select(d => FileImporterViewModel.CreateFileImporterAsync(FileSystem, DialogsService, PlatformService, CryptographyService, systemFolder, d.Name, d.Description, d.MD5)).ToArray();
                var newImporters = await Task.WhenAll(tasks);
                importers.AddRange(newImporters);
            }

            return importers;
        }
    }
}
