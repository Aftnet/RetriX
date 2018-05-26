using MvvmCross.Core.ViewModels;
using Plugin.VersionTracking.Abstractions;

namespace RetriX.Shared.ViewModels
{
    public class AboutViewModel : MvxViewModel
    {
        public string Version { get; }

        public AboutViewModel(IVersionTracking versionTracker)
        {
            Version = versionTracker.CurrentVersion;
        }
    }
}