using Acr.UserDialogs;
using Moq;
using Plugin.FileSystem.Abstractions;
using Plugin.LocalNotifications.Abstractions;
using RetriX.Shared.Services;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test
{
    [Collection(nameof(Test))]
    public abstract class TestBase<T> where T : class
    {
        protected abstract T InstantiateTarget();

        protected readonly T Target;

        protected readonly Mock<IFileSystem> FileSystemMock = new Mock<IFileSystem>();
        protected readonly Mock<IUserDialogs> DialogsServiceMock = new Mock<IUserDialogs>();
        protected readonly Mock<IPlatformService> PlatformServiceMock = new Mock<IPlatformService>();
        protected readonly Mock<ILocalNotifications> NotificationServiceMock = new Mock<ILocalNotifications>();
        protected readonly Mock<ICryptographyService> CryptographyServiceMock = new Mock<ICryptographyService>();

        public TestBase()
        {
            Target = InstantiateTarget();
        }

        protected Task<IDirectoryInfo> GetTestFilesFolderAsync()
        {
            return Plugin.FileSystem.CrossFileSystem.Current.GetDirectoryFromPathAsync("TestFiles");
        }
    }
}
