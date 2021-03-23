using MvvmCross;
using MvvmCross.Logging;
using MvvmCross.Platforms.Uap.Core;
using MvvmCross.Platforms.Uap.Presenters;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.ViewModels;
using RetriX.Shared.Presentation;
using RetriX.Shared.Services;
using RetriX.UWP.Presentation;
using RetriX.UWP.Services;
using Windows.UI.Xaml.Controls;

namespace RetriX.UWP
{
    public class Setup : MvxWindowsSetup
    {
        public Setup() : base()
        {
        }

        public override void PlatformInitialize(Frame rootFrame, string suspensionManagerSessionStateKey = null)
        {
            base.PlatformInitialize(rootFrame, suspensionManagerSessionStateKey);
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
            var presenter = new CurrentViewModelPresenter(rootFrame);
            Mvx.RegisterSingleton<ICurrentViewModelPresenter>(presenter);
            return presenter;
        }

        public override MvxLogProviderType GetDefaultLogProviderType()
        {
            return MvxLogProviderType.Console;
        }
    }

}
