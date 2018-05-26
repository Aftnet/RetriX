using Acr.UserDialogs;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class GameSystemSelectionViewModel : MvxViewModel<IFileInfo>
    {
        private IMvxNavigationService NavigationService { get; }
        private IFileSystem FileSystem { get; }
        private IUserDialogs DialogsService { get; }
        private IPlatformService PlatformService { get; }
        private IGameSystemsProviderService GameSystemsProviderService { get; }

        private IFileInfo SelectedGameFile { get; set; }

        private IReadOnlyList<GameSystemViewModel> gameSystems;
        public IReadOnlyList<GameSystemViewModel> GameSystems
        {
            get => gameSystems;
            private set => SetProperty(ref gameSystems, value);
        }

        public IMvxCommand ShowSettings { get; }
        public IMvxCommand ShowAbout { get; }
        public IMvxCommand<GameSystemViewModel> GameSystemSelected { get; }

        public GameSystemSelectionViewModel(IMvxNavigationService navigationService, IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, IGameSystemsProviderService gameSystemsProviderService)
        {
            NavigationService = navigationService;
            FileSystem = fileSystem;
            DialogsService = dialogsService;
            PlatformService = platformService;
            GameSystemsProviderService = gameSystemsProviderService;

            ResetSystemsSelection();

            ShowSettings = new MvxCommand(() => NavigationService.Navigate<SettingsViewModel>());
            ShowAbout = new MvxCommand(() => NavigationService.Navigate<AboutViewModel>());
            GameSystemSelected = new MvxCommand<GameSystemViewModel>(GameSystemSelectedHandler);
        }

        public override void Prepare()
        {
            ResetSystemsSelection();
        }

        public override void Prepare(IFileInfo parameter)
        {
            SelectedGameFile = parameter;
        }

        public override async Task Initialize()
        {
            //Find compatible systems for file extension
            var extension = SelectedGameFile != null ? Path.GetExtension(SelectedGameFile.Name) : string.Empty;
            var compatibleSystems = GameSystemsProviderService.Systems.Where(d => d.SupportedExtensions.Contains(extension));

            //If none, do nothing
            if (!compatibleSystems.Any())
            {
                return;
            }

            //If just one, start game with it
            if (compatibleSystems.Count() == 1)
            {
                await StartGameAsync(compatibleSystems.Single(), SelectedGameFile);
                return;
            }

            GameSystems = compatibleSystems.ToArray();
        }

        private async void GameSystemSelectedHandler(GameSystemViewModel system)
        {
            if (SelectedGameFile == null)
            {
                var extensions = system.SupportedExtensions.Concat(ArchiveStreamProvider.SupportedExtensions).ToArray();
                SelectedGameFile = await FileSystem.PickFileAsync(extensions);
            }
            if (SelectedGameFile == null)
            {
                return;
            }

            await StartGameAsync(system, SelectedGameFile);
        }

        private async Task StartGameAsync(GameSystemViewModel system, IFileInfo file)
        {
            var dependenciesMet = await system.CheckDependenciesMetAsync();
            if (!dependenciesMet)
            {
                ResetSystemsSelection();
                await DialogsService.AlertAsync(Resources.Strings.SystemUnmetDependenciesAlertTitle, Resources.Strings.SystemUnmetDependenciesAlertMessage);
                return;
            }

            var folderNeeded = system.CheckRootFolderRequired(file);
            var folder = default(IDirectoryInfo);
            if (folderNeeded)
            {
                await DialogsService.AlertAsync(Resources.Strings.SelectFolderRequestAlertTitle, Resources.Strings.SelectFolderRequestAlertMessage);
                folder = await FileSystem.PickDirectoryAsync();
                if (folder == null)
                {
                    ResetSystemsSelection();
                    return;
                }

                if (!Path.GetDirectoryName(file.FullName).StartsWith(folder.FullName))
                {
                    ResetSystemsSelection();
                    await DialogsService.AlertAsync(Resources.Strings.SelectFolderInvalidAlertTitle, Resources.Strings.SelectFolderInvalidAlertMessage);
                    return;
                }
            }

            var param = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, folder);
            await NavigationService.Navigate<GamePlayerViewModel, GamePlayerViewModel.Parameter>(param);
            ResetSystemsSelection();
        }

        private void ResetSystemsSelection()
        {
            //Reset systems selection
            GameSystems = GameSystemsProviderService.Systems;
            SelectedGameFile = null;
        }
    }
}