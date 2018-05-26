using RetriX.Shared.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RetriX.UWP.Controls
{
    public sealed partial class FileImporter : UserControl
    {
        public FileImporterViewModel VM
        {
            get { return (FileImporterViewModel)GetValue(VMProperty); }
            set { SetValue(VMProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VM.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VMProperty = DependencyProperty.Register(nameof(VM), typeof(FileImporterViewModel), typeof(FileImporter), new PropertyMetadata(null));

        public FileImporter()
        {
            this.InitializeComponent();
        }
    }
}
