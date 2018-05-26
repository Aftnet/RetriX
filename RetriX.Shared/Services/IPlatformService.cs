using System;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public enum FullScreenChangeType { Enter, Exit, Toggle };

    public enum MousePointerVisibility { Visible, Hidden };

    public interface IPlatformService
    {
        bool FullScreenChangingPossible { get; }
        bool IsFullScreenMode { get; }
        bool ShouldDisplayTouchGamepad { get; }
        bool HandleGameplayKeyShortcuts { get; set; }

        Task<bool> ChangeFullScreenStateAsync(FullScreenChangeType changeType);
        void ChangeMousePointerVisibility(MousePointerVisibility visibility);
        void ForceUIElementFocus();

        void CopyToClipboard(string content);

        event EventHandler<FullScreenChangeEventArgs> FullScreenChangeRequested;

        event EventHandler PauseToggleRequested;

        event EventHandler<GameStateOperationEventArgs> GameStateOperationRequested;
    }
}