using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class CombinedStreamProvider : IStreamProvider
    {
        private readonly ISet<IStreamProvider> Providers;

        public CombinedStreamProvider(ISet<IStreamProvider> providers)
        {
            Providers = providers;
        }

        public void Dispose()
        {
            foreach (var i in Providers)
            {
                i.Dispose();
            }
        }

        public async Task<IEnumerable<string>> ListEntriesAsync()
        {
            var tasks = Providers.Select(d => d.ListEntriesAsync()).ToArray();
            var results = await Task.WhenAll(tasks);
            var output = results.SelectMany(d => d.ToArray()).OrderBy(d => d).ToArray();
            return output;
        }

        public async Task<Stream> OpenFileStreamAsync(string path, FileAccess accessType)
        {
            foreach(var i in Providers)
            {
                var stream = await i.OpenFileStreamAsync(path, accessType);
                if (stream != null)
                {
                    return stream;
                }
            }

            return null;
        }

        public void CloseStream(Stream stream)
        {
            foreach (var i in Providers)
            {
                i.CloseStream(stream);
            }
        }
    }
}
