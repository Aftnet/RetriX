using System.IO;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public interface ISaveStateService
    {
        void SetGameId(string id);

        Task<Stream> GetStreamForSlotAsync(uint slotId, FileAccess access);
        Task<bool> SlotHasDataAsync(uint slotId);
        Task ClearSavesAsync();
    };
}
