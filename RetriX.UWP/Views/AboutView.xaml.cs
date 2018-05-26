using MvvmCross.Uwp.Views;
using RetriX.Shared.ViewModels;

namespace RetriX.UWP.Pages
{
    public sealed partial class AboutView : MvxWindowsPage
    {
        public AboutViewModel VM => ViewModel as AboutViewModel;

        public AboutView()
        {
            this.InitializeComponent();
        }
    }
}
