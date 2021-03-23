using MvvmCross;
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
            Mvx.RegisterSingleton(Acr.UserDialogs.UserDialogs.Instance);
            Mvx.RegisterSingleton(Plugin.FileSystem.CrossFileSystem.Current);
            Mvx.RegisterSingleton(Plugin.LocalNotifications.CrossLocalNotifications.Current);
            Mvx.RegisterSingleton(Plugin.VersionTracking.CrossVersionTracking.Current);
            Mvx.RegisterSingleton(Plugin.Settings.CrossSettings.Current);

            Mvx.ConstructAndRegisterSingleton<ICryptographyService, CryptographyService>();
            Mvx.ConstructAndRegisterSingleton<ISaveStateService, SaveStateService>();
            Mvx.LazyConstructAndRegisterSingleton<IEmulationService, EmulationService>();
            Mvx.LazyConstructAndRegisterSingleton<IMvxAppStart, AppStart>();
            Mvx.LazyConstructAndRegisterSingleton<PostLoadService, PostLoadService>();
        }
    }
}
