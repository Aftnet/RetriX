using Acr.UserDialogs;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using System;
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

        private IEnumerable<GameSystemViewModel> gameSystems;
        public IEnumerable<GameSystemViewModel> GameSystems
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
            var compatibleSystems = await GameSystemsProviderService.GetCompatibleSystems(SelectedGameFile);
            switch (compatibleSystems.Count)
            {
                case 0:
                    {
                        ResetSystemsSelection();
                        break;
                    }
                case 1:
                    {
                        await StartGameAsync(compatibleSystems.Single(), SelectedGameFile);
                        break;
                    }
                default:
                    {
                        GameSystems = compatibleSystems.ToArray();
                        break;
                    }
            }
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
            var result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, null);
            if (result.Item2 == GameLaunchEnvironment.GenerateResult.RootFolderRequired)
            {
                await DialogsService.AlertAsync(Resources.Strings.SelectFolderRequestAlertMessage, Resources.Strings.SelectFolderRequestAlertTitle);
                var folder = await FileSystem.PickDirectoryAsync();
                if (folder == null)
                {
                    ResetSystemsSelection();
                    return;
                }

                if (!Path.GetDirectoryName(file.FullName).StartsWith(folder.FullName))
                {
                    ResetSystemsSelection();
                    await DialogsService.AlertAsync(Resources.Strings.SelectFolderInvalidAlertMessage, Resources.Strings.SelectFolderInvalidAlertTitle);
                    return;
                }

                result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(system, file, folder);
            }

            switch (result.Item2)
            {
                case GameLaunchEnvironment.GenerateResult.DependenciesUnmet:
                    {
                        ResetSystemsSelection();
                        await DialogsService.AlertAsync(Resources.Strings.SystemUnmetDependenciesAlertMessage, Resources.Strings.SystemUnmetDependenciesAlertTitle);
                        return;
                    }
                case GameLaunchEnvironment.GenerateResult.NoMainFileFound:
                    {
                        ResetSystemsSelection();
                        await DialogsService.AlertAsync(Resources.Strings.NoCompatibleFileInArchiveAlertMessage, Resources.Strings.NoCompatibleFileInArchiveAlertTitle);
                        return;
                    }
                case GameLaunchEnvironment.GenerateResult.Success:
                    {
                        await NavigationService.Navigate<GamePlayerViewModel, GameLaunchEnvironment>(result.Item1);
                        ResetSystemsSelection();
                        return;
                    }
                default:
                    throw new Exception("This should never happen");
            }
        }

        private void ResetSystemsSelection()
        {
            //Reset systems selection
            GameSystems = GameSystemsProviderService.Systems;
            SelectedGameFile = null;
        }
    }
}