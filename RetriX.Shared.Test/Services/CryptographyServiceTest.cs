using RetriX.Shared.Services;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.Services
{
    public class CryptographyServiceTest : TestBase<CryptographyService>
    {
        protected override CryptographyService InstantiateTarget()
        {
            return new CryptographyService();
        }

        [Theory]
        [InlineData("TestFile.txt", "669539c50a711d6ae846d4e554600798")]
        public async Task MD5ComputingWorks(string fileName, string expectedMD5)
        {
            var folder = await GetTestFilesFolderAsync();
            var file = await folder.GetFileAsync(fileName);
            var md5 = await Target.ComputeMD5Async(file);

            Assert.Equal(expectedMD5.ToLowerInvariant(), md5.ToLowerInvariant());
        }
    }
}
