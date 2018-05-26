using RetriX.Shared.StreamProviders;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.StreamProviders
{
    public class FolderStreamProviderTest : StreamProviderTestBase
    {
        const string HandledScheme = "SCHEME";

        protected override async Task<IStreamProvider> GetTargetAsync()
        {
            var folder = await GetTestFilesFolderAsync();
            return new FolderStreamProvider(HandledScheme, folder);
        }

        [Fact]
        public Task ListingEntriesWorks()
        {
            return ListingEntriesWorksInternal(4);
        }

        [Theory]
        [InlineData("SCHEME\\TestFile.txt", true)]
        [InlineData("SCHEME\\Archive.zip", true)]
        [InlineData("SCHEME\\A\\B\\AnotherFile.cds", true)]
        [InlineData("SCHEME\\A\\C\\AnotherFile.cds", false)]
        [InlineData("ASCHEME\\Archive.zip", false)]
        [InlineData("ASCHEME\\SomeFi.ext", false)]
        [InlineData("ASCHEME\\Dir\\file.ext", false)]
        public Task OpeningFileWorks(string path, bool expectedSuccess)
        {
            return OpeningFileWorksInternal(path, expectedSuccess);
        }
    }
}
