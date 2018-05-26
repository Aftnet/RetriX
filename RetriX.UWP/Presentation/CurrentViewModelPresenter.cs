using MvvmCross.Core.ViewModels;
using MvvmCross.Uwp.Views;
using RetriX.Shared.Presentation;

namespace RetriX.UWP.Presentation
{
    public class CurrentViewModelPresenter : MvxWindowsViewPresenter, ICurrentViewModelPresenter
    {
        public IMvxViewModel CurrentViewModel => (_rootFrame.Content as MvxWindowsPage)?.ViewModel;

        public CurrentViewModelPresenter(IMvxWindowsFrame rootFrame) : base(rootFrame)
        {
        }
    }
}
