using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace RetriX.UWP.Controls
{
    public sealed partial class VirtualPad : UserControl
    {
        public GamePlayerViewModel ViewModel
        {
            get { return (GamePlayerViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(ViewModel), typeof(GamePlayerViewModel), typeof(PlayerOverlay), new PropertyMetadata(null));

        public InjectedInputTypes UpButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(UpButtonInputTypeProperty); }
            set { SetValue(UpButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UpButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpButtonInputTypeProperty = DependencyProperty.Register(nameof(UpButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadUp));

        public InjectedInputTypes DownButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(DownButtonInputTypeProperty); }
            set { SetValue(DownButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DownButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DownButtonInputTypeProperty = DependencyProperty.Register(nameof(DownButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadDown));

        public InjectedInputTypes LeftButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(LeftButtonInputTypeProperty); }
            set { SetValue(LeftButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LeftButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftButtonInputTypeProperty = DependencyProperty.Register(nameof(LeftButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadLeft));

        public InjectedInputTypes RightButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(RightButtonInputTypeProperty); }
            set { SetValue(RightButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RightButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightButtonInputTypeProperty = DependencyProperty.Register(nameof(RightButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadRight));

        public InjectedInputTypes CenterButtonInputType
        {
            get { return (InjectedInputTypes)GetValue(CenterButtonInputTypeProperty); }
            set { SetValue(CenterButtonInputTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CenterButtonInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterButtonInputTypeProperty = DependencyProperty.Register(nameof(CenterButtonInputType), typeof(InjectedInputTypes), typeof(VirtualPad), new PropertyMetadata(InjectedInputTypes.DeviceIdJoypadSelect));

        public VirtualPad()
        {
            this.InitializeComponent();
        }

        private void UpButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.InjectInputCommand.Execute(UpButtonInputType);
        }

        private void DownButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.InjectInputCommand.Execute(DownButtonInputType);
        }

        private void LeftButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.InjectInputCommand.Execute(LeftButtonInputType);
        }

        private void RightButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.InjectInputCommand.Execute(RightButtonInputType);
        }

        private void CenterButtonClick(object sender, RoutedEventArgs e)
        {
            ViewModel.InjectInputCommand.Execute(CenterButtonInputType);
        }
    }
}
