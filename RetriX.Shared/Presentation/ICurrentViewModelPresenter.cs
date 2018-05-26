using MvvmCross.Core.ViewModels;
using MvvmCross.Core.Views;

namespace RetriX.Shared.Presentation
{
    public interface ICurrentViewModelPresenter : IMvxViewPresenter
    {
        IMvxViewModel CurrentViewModel { get; }
    }
}
