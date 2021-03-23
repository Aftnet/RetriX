using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using RetriX.Shared.Services;
using System.Threading.Tasks;

namespace RetriX.Shared
{
    public class AppStart : MvxAppStart
    {
        private PostLoadService PostLoadService { get; }
        public AppStart(IMvxNavigationService navigationService, IMvxApplication application, PostLoadService postLoadService)
            : base(application, navigationService)
        {
            PostLoadService = postLoadService;
        }

        protected override Task NavigateToFirstViewModel(object hint = null)
        {
            return PostLoadService.PerformNavigation(hint);
        }
    }
}
