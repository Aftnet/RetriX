using MvvmCross.Uwp.Views;
using RetriX.Shared.ViewModels;

namespace RetriX.UWP.Pages
{
    public sealed partial class SettingsView : MvxWindowsPage
    {
        public SettingsViewModel VM => ViewModel as SettingsViewModel;

        public SettingsView()
        {
            this.InitializeComponent();
        }
    }
}
