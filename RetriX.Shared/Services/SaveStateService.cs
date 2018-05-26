using Plugin.FileSystem.Abstractions;
using RetriX.Shared.ExtensionMethods;
using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class SaveStateService : ISaveStateService
    {
        private const string SaveStatesFolderName = "SaveStates";

        private readonly IFileSystem FileSystem;

        private string GameId { get; set; }

        private bool OperationInProgress = false;
        private bool AllowOperations => !(OperationInProgress || SaveStatesFolder == null || GameId == null);

        private IDirectoryInfo SaveStatesFolder;

        public SaveStateService(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;

            GetSubfolderAsync(FileSystem.LocalStorage, SaveStatesFolderName).ContinueWith(d =>
            {
                SaveStatesFolder = d.Result;
            });

            GameId = null;
        }

        public void SetGameId(string id)
        {
            GameId = null;
            if(string.IsNullOrEmpty(id) || string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            GameId = id.MD5();
        }

        public async Task<Stream> GetStreamForSlotAsync(uint slotId, FileAccess access)
        {
            if (!AllowOperations)
            {
                return null;
            }

            OperationInProgress = true;

            var statesFolder = await GetGameSaveStatesFolderAsync();
            var fileName = GenerateSaveFileName(slotId);
            var file = await statesFolder.GetFileAsync(fileName);
            if (file == null)
            {
                if (access == FileAccess.Read)
                {
                    OperationInProgress = false;
                    return null;
                }

                file = await statesFolder.CreateFileAsync(fileName);
                //This should never happen
                if (file == null)
                {
                    return null;
                }
            }

            var stream = await file.OpenAsync(access);
            OperationInProgress = false;
            return stream;
        }

        public async Task<bool> SlotHasDataAsync(uint slotId)
        {
            if (!AllowOperations)
            {
                return false;
            }

            OperationInProgress = true;

            var statesFolder = await GetGameSaveStatesFolderAsync();
            var fileName = GenerateSaveFileName(slotId);
            var file = await statesFolder.GetFileAsync(fileName);

            OperationInProgress = false;
            return file != null;
        }

        public async Task ClearSavesAsync()
        {
            if (!AllowOperations)
            {
                return;
            }

            OperationInProgress = true;

            var statesFolder = await GetGameSaveStatesFolderAsync();
            await statesFolder.DeleteAsync();
            await GetGameSaveStatesFolderAsync();

            OperationInProgress = false;
        }

        private string GenerateSaveFileName(uint slotId)
        {
            return $"{GameId}_S{slotId}.sav";
        }

        private Task<IDirectoryInfo> GetGameSaveStatesFolderAsync()
        {
            return GetSubfolderAsync(SaveStatesFolder, GameId);
        }

        private static async Task<IDirectoryInfo> GetSubfolderAsync(IDirectoryInfo parent, string name)
        {
            IDirectoryInfo output = await parent.GetDirectoryAsync(name);
            if (output == null)
            {
                output = await parent.CreateDirectoryAsync(name);
            }

            return output;
        }
    }
}
