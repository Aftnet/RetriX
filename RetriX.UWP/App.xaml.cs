using MvvmCross;
using MvvmCross.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using RetriX.UWP.Pages;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace RetriX.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Frame RootFrame => Window.Current.Content as Frame;

        public App()
        {
            RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            await InitializeApp(e.PreviousExecutionState, e.PrelaunchActivated, null);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs e)
        {
            var file = e.Files.First(d => d is IStorageFile);
            var wrappedFile = new Plugin.FileSystem.FileInfo((StorageFile)file);

            await InitializeApp(e.PreviousExecutionState, false, wrappedFile);
        }

        private async Task InitializeApp(ApplicationExecutionState previousExecutionState, bool prelaunchActivated, IFileInfo file)
        {
            var rootFrame = RootFrame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (prelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    var setup = new Setup();
                    setup.PlatformInitialize(rootFrame);
                    setup.InitializePrimary();
                    setup.InitializeSecondary();

                    var start = Mvx.IoCProvider.Resolve<IMvxAppStart>();
                    await start.StartAsync(file);
                }
                else
                {
                    var postLoadService = Mvx.IoCProvider.Resolve<PostLoadService>();
                    await postLoadService.PerformNavigation(file);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
