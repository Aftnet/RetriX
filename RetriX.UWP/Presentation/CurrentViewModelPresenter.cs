using MvvmCross.Platforms.Uap.Presenters;
using MvvmCross.Platforms.Uap.Views;
using MvvmCross.ViewModels;
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
