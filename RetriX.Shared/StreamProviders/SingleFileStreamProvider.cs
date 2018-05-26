using Plugin.FileSystem.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class SingleFileStreamProvider : StreamProviderBase
    {
        private readonly string Path;
        private readonly IFileInfo File;

        public SingleFileStreamProvider(string path, IFileInfo file)
        {
            Path = path;
            File = file;
        }
        
        public override Task<IEnumerable<string>> ListEntriesAsync()
        {
            var output = new string[] { Path };
            return Task.FromResult(output as IEnumerable<string>);
        }

        protected override Task<Stream> OpenFileStreamAsyncInternal(string path, FileAccess accessType)
        {
            if (Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                return File.OpenAsync(accessType);
            }

            return Task.FromResult(default(Stream));
        }
    }
}
