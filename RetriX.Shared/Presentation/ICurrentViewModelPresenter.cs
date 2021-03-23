using MvvmCross.Presenters;
using MvvmCross.ViewModels;

namespace RetriX.Shared.Presentation
{
    public interface ICurrentViewModelPresenter : IMvxViewPresenter
    {
        IMvxViewModel CurrentViewModel { get; }
    }
}
