using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.Presentation;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using System.Linq;

namespace RetriX.Shared
{
    public class AppStart : IMvxAppStart
    {
        private IMvxNavigationService NavigationService { get; }
        private ICurrentViewModelPresenter Presenter { get; }
        private IGameSystemsProviderService GameSystemsProviderService { get; }

        public AppStart(IMvxNavigationService navigationService, ICurrentViewModelPresenter presenter, IGameSystemsProviderService gameSystemsProviderService)
        {
            NavigationService = navigationService;
            Presenter = presenter;
            GameSystemsProviderService = gameSystemsProviderService;
        }

        public async void Start(object hint = null)
        {
            var file = hint as IFileInfo;
            if (file == null)
            {
                await NavigationService.Navigate<GameSystemSelectionViewModel>();
                return;
            }

            if (Presenter.CurrentViewModel is GamePlayerViewModel)
            {
                var compatibleSystems = await GameSystemsProviderService.GetCompatibleSystems(file);
                if (compatibleSystems.Count == 1)
                {
                    var result = await GameSystemsProviderService.GenerateGameLaunchEnvironmentAsync(compatibleSystems.First(), file, null);
                    if (result.Item2 == GameLaunchEnvironment.GenerateResult.Success)
                    {
                        var currentGamePlayerVM = Presenter.CurrentViewModel as GamePlayerViewModel;
                        currentGamePlayerVM.Prepare(result.Item1);
                        await currentGamePlayerVM.Initialize();
                        return;
                    }
                }
            }

            if (Presenter.CurrentViewModel is GameSystemSelectionViewModel)
            {
                var currentSystemSelectionVM = Presenter.CurrentViewModel as GameSystemSelectionViewModel;
                currentSystemSelectionVM.Prepare(file);
                await currentSystemSelectionVM.Initialize();
            }
            else
            {
                await NavigationService.Navigate<GameSystemSelectionViewModel, IFileInfo>(file);
            }
        }
    }
}
