using Moq;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.ViewModels
{
    public class FileImporterViewModelTest : TestBase<FileImporterViewModel>
    {
        protected override FileImporterViewModel InstantiateTarget()
        {
            return FileImporterViewModel.CreateFileImporterAsync(FileSystemMock.Object, DialogsServiceMock.Object, PlatformServiceMock.Object, CryptographyServiceMock.Object, GetTestFilesFolderAsync().Result, "TargetFile.ext", "Target file description", "SomeMD5").Result;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ImportingWorks(bool providedFileMD5Matches)
        {
            Assert.False(Target.FileAvailable);
            Assert.Null(await Target.GetTargetFileAsync());
            Assert.True(Target.ImportCommand.CanExecute(null));

            var folder = await GetTestFilesFolderAsync();
            var pickedFile = await folder.GetFileAsync("TestFile.txt");
            FileSystemMock.Setup(d => d.PickFileAsync(It.Is<IEnumerable<string>>(e => e.Contains(Path.GetExtension(Target.TargetFileName))))).Returns(Task.FromResult(pickedFile));

            var computedHash = providedFileMD5Matches ? Target.TargetMD5.ToUpperInvariant() : "otherHash";
            CryptographyServiceMock.Setup(d => d.ComputeMD5Async(pickedFile)).Returns(Task.FromResult(computedHash));

            Target.ImportCommand.Execute(null);
            await Task.Delay(100);

            var expectedDialogServiceCalledTimes = providedFileMD5Matches ? Times.Never() : Times.Once();
            DialogsServiceMock.Verify(d => d.AlertAsync(It.Is<string>(e => e == Resources.Strings.FileHashMismatchMessage), It.Is<string>(e => e == Resources.Strings.FileHashMismatchTitle), null, null), expectedDialogServiceCalledTimes);

            Assert.Equal(providedFileMD5Matches, Target.FileAvailable);
            Assert.Equal(!providedFileMD5Matches, Target.ImportCommand.CanExecute(null));

            if (providedFileMD5Matches)
            {
                var targetFile = await Target.GetTargetFileAsync();
                Assert.NotNull(targetFile);
                await targetFile.DeleteAsync();
            }
        }

        [Fact]
        public async Task NoFileSelectionIsHandled()
        {
            Assert.False(Target.FileAvailable);

            FileSystemMock.Setup(d => d.PickFileAsync(It.Is<IEnumerable<string>>(e => e.Contains(Path.GetExtension(Target.TargetFileName))))).Returns(Task.FromResult(default(IFileInfo)));

            CryptographyServiceMock.Verify(d => d.ComputeMD5Async(It.IsAny<IFileInfo>()), Times.Never);
            DialogsServiceMock.Verify(d => d.AlertAsync(It.IsAny<string>(), It.IsAny<string>(), null, null), Times.Never);

            Target.ImportCommand.Execute(null);
            await Task.Delay(100);

            Assert.False(Target.FileAvailable);
        }
    }
}
