using System.Threading.Tasks;
using Xunit;

namespace RetriX.Shared.Test.StreamProviders
{
    /*public class ArchiveStreamProviderTest : FileProviderTestBase<ArchiveStreamProvider>
    {
        protected override ArchiveStreamProvider InstantiateTarget()
        {
            var file = GetTestFilesFolderAsync().Result.GetFileAsync("Archive.zip").Result;
            return new ArchiveStreamProvider("scheme:\\", file);
        }

        [Fact]
        public Task ListingEntriesWorks()
        {
            return ListingEntriesWorks(4);
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
    }*/
}
