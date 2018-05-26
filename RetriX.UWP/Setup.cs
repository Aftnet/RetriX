using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Logging;
using MvvmCross.Uwp.Platform;
using MvvmCross.Uwp.Views;
using RetriX.Shared.Presentation;
using RetriX.Shared.Services;
using RetriX.UWP.Presentation;
using RetriX.UWP.Services;
using Windows.UI.Xaml.Controls;

namespace RetriX.UWP
{
    public class Setup : MvxWindowsSetup
    {
        private CurrentViewModelPresenter Presenter;

        public Setup(Frame rootFrame) : base(rootFrame)
        {
        }

        protected override IMvxApplication CreateApp()
        {
            return new Shared.App();
        }

        protected override void InitializeFirstChance()
        {
            Mvx.ConstructAndRegisterSingleton<IPlatformService, PlatformService>();
            Mvx.ConstructAndRegisterSingleton<IInputService, InputService>();
            Mvx.ConstructAndRegisterSingleton<IAudioService, AudioService>();
            Mvx.ConstructAndRegisterSingleton<IVideoService, VideoService>();
        }

        protected override void InitializeLastChance()
        {
            Mvx.ConstructAndRegisterSingleton<IGameSystemsProviderService, GameSystemsProviderService>();
        }

        protected override IMvxWindowsViewPresenter CreateViewPresenter(IMvxWindowsFrame rootFrame)
        {
            Presenter = new CurrentViewModelPresenter(rootFrame);
            Mvx.RegisterSingleton<ICurrentViewModelPresenter>(Presenter);
            return Presenter;
        }

        protected override MvxLogProviderType GetDefaultLogProviderType()
        {
            return MvxLogProviderType.None;
        }
    }

}
