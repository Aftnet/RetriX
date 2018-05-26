using Acr.UserDialogs;
using MvvmCross.Core.ViewModels;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class FileImporterViewModel : MvxViewModel
    {
        public const string SerachLinkFormat = "https://www.google.com/search?q={0}";

        private readonly IFileSystem FileSystem;
        private readonly IUserDialogs DialogsService;
        private readonly IPlatformService PlatformService;
        private readonly ICryptographyService CryptographyService;

        public IDirectoryInfo TargetFolder { get; }
        public string TargetFileName { get; }
        public string TargetDescription { get; }
        public string TargetMD5 { get; }

        public string SearchLink => string.Format(SerachLinkFormat, TargetMD5);

        private bool fileAvailable = false;
        public bool FileAvailable
        {
            get => fileAvailable;
            private set { if (SetProperty(ref fileAvailable, value)) { ImportCommand.RaiseCanExecuteChanged(); } }
        }

        public IMvxCommand ImportCommand { get; }
        public IMvxCommand CopyMD5ToClipboardCommand { get; }

        protected FileImporterViewModel(IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService, IDirectoryInfo folder, string fileName, string description, string MD5)
        {
            FileSystem = fileSystem;
            DialogsService = dialogsService;
            PlatformService = platformService;
            CryptographyService = cryptographyService;

            TargetFolder = folder;
            TargetFileName = fileName;
            TargetDescription = description;
            TargetMD5 = MD5;

            ImportCommand = new MvxCommand(ImportHandler, () => !FileAvailable);
            CopyMD5ToClipboardCommand = new MvxCommand(() => PlatformService.CopyToClipboard(TargetMD5));
        }

        public static async Task<FileImporterViewModel> CreateFileImporterAsync(IFileSystem fileSystem, IUserDialogs dialogsService, IPlatformService platformService, ICryptographyService cryptographyService, IDirectoryInfo folder, string fileName, string description, string MD5)
        {
            var output = new FileImporterViewModel(fileSystem, dialogsService, platformService, cryptographyService, folder, fileName, description, MD5);
            var targetFile = await output.GetTargetFileAsync();
            output.FileAvailable = targetFile != null;
            return output;
        }

        public Task<IFileInfo> GetTargetFileAsync()
        {
            return TargetFolder.GetFileAsync(TargetFileName);
        }

        private async void ImportHandler()
        {
            var fileExt = Path.GetExtension(TargetFileName);
            var sourceFile = await FileSystem.PickFileAsync(new string[] { fileExt });
            if (sourceFile == null)
            {
                return;
            }

            var md5 = await CryptographyService.ComputeMD5Async(sourceFile);
            if (md5.ToLowerInvariant() != TargetMD5.ToLowerInvariant())
            {
                var title = Resources.Strings.FileHashMismatchTitle;
                var message = Resources.Strings.FileHashMismatchMessage;
                await DialogsService.AlertAsync(message, title);
                return;
            }

            using (var inStream = await sourceFile.OpenAsync(FileAccess.Read))
            {
                var targetFile = await TargetFolder.CreateFileAsync(TargetFileName);
                using (var outStream = await targetFile.OpenAsync(FileAccess.ReadWrite))
                {
                    await inStream.CopyToAsync(outStream);
                    await outStream.FlushAsync();
                }

                FileAvailable = true;
            }            
        }
    }
}
