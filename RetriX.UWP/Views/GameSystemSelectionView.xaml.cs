using MvvmCross.Platforms.Uap.Views;
using RetriX.Shared.ViewModels;

namespace RetriX.UWP.Pages
{
    public sealed partial class GameSystemSelectionView : MvxWindowsPage
    {
        public GameSystemSelectionViewModel VM => ViewModel as GameSystemSelectionViewModel;

        public GameSystemSelectionView()
        {
            this.InitializeComponent();
        }
    }
}
