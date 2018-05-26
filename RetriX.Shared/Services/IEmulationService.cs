using LibRetriX;
using RetriX.Shared.StreamProviders;
using System;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public enum InjectedInputTypes
    {
        DeviceIdJoypadB = 0,
        DeviceIdJoypadY = 1,
        DeviceIdJoypadSelect = 2,
        DeviceIdJoypadStart = 3,
        DeviceIdJoypadUp = 4,
        DeviceIdJoypadDown = 5,
        DeviceIdJoypadLeft = 6,
        DeviceIdJoypadRight = 7,
        DeviceIdJoypadA = 8,
        DeviceIdJoypadX = 9,
    };

    public interface IEmulationService
    {
        Task<bool> StartGameAsync(ICore core, IStreamProvider streamProvider, string mainFilePath);

        Task ResetGameAsync();
        Task StopGameAsync();

        Task PauseGameAsync();
        Task ResumeGameAsync();

        Task<bool> SaveGameStateAsync(uint slotID);
        Task<bool> LoadGameStateAsync(uint slotID);

        void InjectInputPlayer1(InjectedInputTypes inputType);

        event EventHandler GameStarted;
        event EventHandler GameStopped;
        event EventHandler<Exception> GameRuntimeExceptionOccurred;
    }
}
