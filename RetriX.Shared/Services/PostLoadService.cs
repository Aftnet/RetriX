using MvvmCross.Navigation;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Models;
using RetriX.Shared.Presentation;
using RetriX.Shared.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class PostLoadService
    {
        private ICurrentViewModelPresenter Presenter { get; }
        private IMvxNavigationService NavigationService { get; }
        private IGameSystemsProviderService GameSystemsProviderService { get; }
        public PostLoadService(ICurrentViewModelPresenter presenter, IMvxNavigationService navigationService, IGameSystemsProviderService gameSystemsProviderService)
        {
            Presenter = presenter;
            NavigationService = navigationService;
            GameSystemsProviderService = gameSystemsProviderService;
        }

        public async Task PerformNavigation(object hint = null)
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
                        currentGamePlayerVM.Initialize().GetAwaiter().GetResult();
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
