using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public abstract class StreamProviderBase : IStreamProvider
    {
        private readonly HashSet<Stream> OpenStreams = new HashSet<Stream>();

        public abstract Task<IEnumerable<string>> ListEntriesAsync();
        protected abstract Task<Stream> OpenFileStreamAsyncInternal(string path, FileAccess accessType);

        public virtual void Dispose()
        {
            foreach(var i in OpenStreams)
            {
                i.Dispose();
            }
        }

        public async Task<Stream> OpenFileStreamAsync(string path, FileAccess accessType)
        {
            var stream = await OpenFileStreamAsyncInternal(path, accessType);
            if (stream != null)
            {
                OpenStreams.Add(stream);
            }

            return stream;
        }

        public void CloseStream(Stream stream)
        {
            if (OpenStreams.Contains(stream))
            {
                OpenStreams.Remove(stream);
                stream.Dispose();
            }
        }
    }
}
