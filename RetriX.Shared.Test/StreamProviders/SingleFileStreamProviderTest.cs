using RetriX.Shared.StreamProviders;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.StreamProviders
{
    public class SingleFileStreamProviderTest : StreamProviderTestBase
    {
        private const string FilePath = "SCHEME\\SomeFile.ext";

        protected override async Task<IStreamProvider> GetTargetAsync()
        {
            var folder = await GetTestFilesFolderAsync();
            var file = await folder.GetFileAsync("TestFile.txt");
            return new SingleFileStreamProvider(FilePath, file);
        }

        [Fact]
        public Task ListingEntriesWorks()
        {
            return ListingEntriesWorksInternal(1); 
        }

        [Theory]
        [InlineData(FilePath, true)]
        [InlineData("SCHEME2\\SomeFile.ext", false)]
        [InlineData("SCHEME\\SomeFi.ext", false)]
        [InlineData("SCHEME\\Dir\\file.ext", false)]
        public Task OpeningFileWorks(string path, bool expectedSuccess)
        {
            return OpeningFileWorksInternal(path, expectedSuccess);
        }
    }
}
