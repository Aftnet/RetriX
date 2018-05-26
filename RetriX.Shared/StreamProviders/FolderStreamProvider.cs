using Plugin.FileSystem.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.StreamProviders
{
    public class FolderStreamProvider : StreamProviderBase
    {
        private readonly string HandledScheme;
        private readonly IDirectoryInfo RootFolder;
        private IDictionary<string, IFileInfo> Contents;

        public FolderStreamProvider(string handledScheme, IDirectoryInfo rootFolder)
        {
            HandledScheme = handledScheme;
            RootFolder = rootFolder;
        }

        public override async Task<IEnumerable<string>> ListEntriesAsync()
        {
            var contents = await GetContents();
            return contents.Keys.Select(d => HandledScheme + d).ToArray();
        }

        protected override async Task<Stream> OpenFileStreamAsyncInternal(string path, FileAccess accessType)
        {
            if (!path.StartsWith(HandledScheme))
            {
                return null;
            }

            var contents = await GetContents();
            path = path.Substring(HandledScheme.Length).ToLowerInvariant();
            contents.TryGetValue(path, out var file);
            if (accessType == FileAccess.Read && file == null)
            {
                return null;
            }

            if (file == null)
            {
                file = await RootFolder.CreateFileAsync(path);
                Contents.Add(GenerateSchemaName(file), file);
            }

            var output = await file.OpenAsync(accessType);
            return output;
        }

        private string GenerateSchemaName(IFileInfo file)
        {
            return file.FullName.Substring(RootFolder.FullName.Length).ToLowerInvariant();
        }

        private async Task<IDictionary<string, IFileInfo>> GetContents()
        {
            if (Contents == null)
            {
                Contents = new SortedDictionary<string, IFileInfo>();
                var list = await ListFilesRecursiveAsync(RootFolder);
                foreach (var i in list)
                {
                    Contents.Add(GenerateSchemaName(i), i);
                }
            }

            return Contents;
        }

        private async Task<IEnumerable<IFileInfo>> ListFilesRecursiveAsync(IDirectoryInfo folder)
        {
            IEnumerable<IFileInfo> files = await folder.EnumerateFilesAsync();
            var subfolders = await folder.EnumerateDirectoriesAsync();
            foreach (var i in subfolders)
            {
                var subfolderFiles = await ListFilesRecursiveAsync(i);
                files = files.Concat(subfolderFiles);
            }

            return files;
        }
    }
}
