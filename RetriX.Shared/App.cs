using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using RetriX.Shared.Services;

namespace RetriX.Shared
{
    public class App : MvxApplication
    {
        public App()
        {
        }

        public override void Initialize()
        {
            Mvx.IoCProvider.RegisterSingleton(Acr.UserDialogs.UserDialogs.Instance);
            Mvx.IoCProvider.RegisterSingleton(Plugin.FileSystem.CrossFileSystem.Current);
            Mvx.IoCProvider.RegisterSingleton(Plugin.LocalNotifications.CrossLocalNotifications.Current);
            Mvx.IoCProvider.RegisterSingleton(Plugin.VersionTracking.CrossVersionTracking.Current);
            Mvx.IoCProvider.RegisterSingleton(Plugin.Settings.CrossSettings.Current);

            Mvx.IoCProvider.ConstructAndRegisterSingleton<ICryptographyService, CryptographyService>();
            Mvx.IoCProvider.ConstructAndRegisterSingleton<ISaveStateService, SaveStateService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IEmulationService, EmulationService>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<IMvxAppStart, AppStart>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<PostLoadService, PostLoadService>();
        }
    }
}
