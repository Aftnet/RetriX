using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Gaming.Input;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace RetriX.UWP.Services
{
    public class PlatformService : IPlatformService
    {
        private static readonly ISet<string> DeviceFamiliesAllowingFullScreenChange = new HashSet<string>
        {
            "Windows.Desktop", "Windows.Team", "Windows.Mobile"
        };

        private ApplicationView AppView => ApplicationView.GetForCurrentView();
        private CoreWindow CoreWindow => CoreWindow.GetForCurrentThread();
        private ISet<VirtualKey> PressedKeys { get; } = new HashSet<VirtualKey>();

        public bool FullScreenChangingPossible
        {
            get
            {
                var output = DeviceFamiliesAllowingFullScreenChange.Contains(AnalyticsInfo.VersionInfo.DeviceFamily);
                return output;
            }
        }

        public bool IsFullScreenMode => AppView.IsFullScreenMode;

        public bool ShouldDisplayTouchGamepad
        {
            get
            {
                var touchCapabilities = new TouchCapabilities();
                if (touchCapabilities.TouchPresent == 0)
                {
                    return false;
                }

                var keyboardCapabilities = new KeyboardCapabilities();
                if (keyboardCapabilities.KeyboardPresent != 0)
                {
                    return false;
                }

                if (Gamepad.Gamepads.Any())
                {
                    return false;
                }

                return true;
            }
        }

        private bool handleGameplayKeyShortcuts = false;
        public bool HandleGameplayKeyShortcuts
        {
            get => handleGameplayKeyShortcuts;
            set
            {
                PressedKeys.Clear();
                handleGameplayKeyShortcuts = value;
                var window = CoreWindow.GetForCurrentThread();
                window.KeyDown -= OnKeyDown;
                window.KeyUp -= OnKeyUp;
                if (handleGameplayKeyShortcuts)
                {
                    window.KeyDown += OnKeyDown;
                    window.KeyUp += OnKeyUp;
                }
            }
        }

        public event EventHandler<FullScreenChangeEventArgs> FullScreenChangeRequested;

        public event EventHandler PauseToggleRequested;

        public event EventHandler<GameStateOperationEventArgs> GameStateOperationRequested;

        public async Task<bool> ChangeFullScreenStateAsync(FullScreenChangeType changeType)
        {
            if ((changeType == FullScreenChangeType.Enter && IsFullScreenMode) || (changeType == FullScreenChangeType.Exit && !IsFullScreenMode))
            {
                return true;
            }

            if (changeType == FullScreenChangeType.Toggle)
            {
                changeType = IsFullScreenMode ? FullScreenChangeType.Exit : FullScreenChangeType.Enter;
            }

            var result = false;
            switch (changeType)
            {
                case FullScreenChangeType.Enter:
                    result = AppView.TryEnterFullScreenMode();
                    break;
                case FullScreenChangeType.Exit:
                    AppView.ExitFullScreenMode();
                    result = true;
                    break;
                default:
                    throw new Exception("this should never happen");
            }

            await Task.Delay(100);
            return result;
        }

        public void ChangeMousePointerVisibility(MousePointerVisibility visibility)
        {
            var pointer = visibility == MousePointerVisibility.Hidden ? null : new CoreCursor(CoreCursorType.Arrow, 0);
            Window.Current.CoreWindow.PointerCursor = pointer;
        }

        public void ForceUIElementFocus()
        {
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }

        public void CopyToClipboard(string content)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(content);
            Clipboard.SetContent(dataPackage);
        }

        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (!PressedKeys.Contains(key))
            {
                PressedKeys.Add(key);
            }

            switch (key)
            {
                //Shift+Enter: enter fullscreen
                case VirtualKey.Enter:
                    if (PressedKeys.Contains(VirtualKey.Shift))
                    {
                        FullScreenChangeRequested(this, new FullScreenChangeEventArgs(FullScreenChangeType.Toggle));
                    }
                    break;

                case VirtualKey.Shift:
                    if (PressedKeys.Contains(VirtualKey.Enter))
                    {
                        FullScreenChangeRequested(this, new FullScreenChangeEventArgs(FullScreenChangeType.Toggle));
                    }
                    break;

                case VirtualKey.Escape:
                    FullScreenChangeRequested(this, new FullScreenChangeEventArgs(FullScreenChangeType.Exit));
                    break;

                case VirtualKey.Space:
                    PauseToggleRequested(this, EventArgs.Empty);
                    break;

                case VirtualKey.GamepadMenu:
                    if (PressedKeys.Contains(VirtualKey.GamepadView))
                    {
                        PauseToggleRequested(this, EventArgs.Empty);
                    }
                    break;

                case VirtualKey.GamepadView:
                    if (PressedKeys.Contains(VirtualKey.GamepadMenu))
                    {
                        PauseToggleRequested(this, EventArgs.Empty);
                    }
                    break;

                case VirtualKey.F1:
                    HandleSaveSlotKeyPress(1);
                    break;

                case VirtualKey.F2:
                    HandleSaveSlotKeyPress(2);
                    break;

                case VirtualKey.F3:
                    HandleSaveSlotKeyPress(3);
                    break;

                case VirtualKey.F4:
                    HandleSaveSlotKeyPress(4);
                    break;

                case VirtualKey.F5:
                    HandleSaveSlotKeyPress(5);
                    break;

                case VirtualKey.F6:
                    HandleSaveSlotKeyPress(6);
                    break;
            }

            args.Handled = true;
        }

        private void OnKeyUp(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (PressedKeys.Contains(key))
            {
                PressedKeys.Remove(key);
            }

            args.Handled = true;
        }

        private void HandleSaveSlotKeyPress(uint slotID)
        {
            var modifierKeyPressed = PressedKeys.Contains(VirtualKey.Shift);
            var eventArgs = new GameStateOperationEventArgs(modifierKeyPressed ? GameStateOperationEventArgs.GameStateOperationType.Save : GameStateOperationEventArgs.GameStateOperationType.Load, slotID);
            GameStateOperationRequested(this, eventArgs);
        }
    }
}
