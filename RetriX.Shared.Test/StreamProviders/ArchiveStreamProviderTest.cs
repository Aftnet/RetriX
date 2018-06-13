using RetriX.Shared.StreamProviders;
using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.StreamProviders
{
    public class ArchiveStreamProviderTest : StreamProviderTestBase<ArchiveStreamProvider>
    {
        protected override Task<ArchiveStreamProvider> GetTargetAsync()
        {
            var file = GetTestFilesFolderAsync().Result.GetFileAsync("Archive.zip").Result;
            return Task.FromResult(new ArchiveStreamProvider("scheme:\\", file));
        }

        [Fact]
        public Task ListingEntriesWorks()
        {
            return ListingEntriesWorksInternal(4);
        }

        [Theory]
        [InlineData("scheme:\\TestFile.txt", true)]
        [InlineData("scheme:\\AnotherFile.cds", true)]
        [InlineData("scheme:\\Afolder\\File.zzz", true)]
        [InlineData("scheme2:\\SomeFile.ext", false)]
        [InlineData("scheme:\\SomeFi.ext", false)]
        [InlineData("scheme:\\Dir\\file.ext", false)]
        public Task OpeningFileWorks(string path, bool expectedSuccess)
        {
            return OpeningFileWorksInternal(path, expectedSuccess);
        }
    }
}
