using LibRetriX;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Gaming.Input;
using Windows.System;
using Windows.UI.Core;

namespace RetriX.UWP
{
    public sealed class InputService : IInputService
    {
        private const uint InjectedInputFramePermamence = 4;
        private const double GamepadAnalogDeadZoneSquareRadius = 0.0;

        private static readonly IReadOnlyDictionary<InputTypes, VirtualKey> LibretroGamepadToKeyboardKeyMapping = new Dictionary<InputTypes, VirtualKey>()
        {
            { InputTypes.DeviceIdJoypadUp, VirtualKey.Up },
            { InputTypes.DeviceIdJoypadDown, VirtualKey.Down },
            { InputTypes.DeviceIdJoypadLeft, VirtualKey.Left },
            { InputTypes.DeviceIdJoypadRight, VirtualKey.Right },
            { InputTypes.DeviceIdJoypadA, VirtualKey.A },
            { InputTypes.DeviceIdJoypadB, VirtualKey.S },
            { InputTypes.DeviceIdJoypadX, VirtualKey.Z },
            { InputTypes.DeviceIdJoypadY, VirtualKey.X },
            { InputTypes.DeviceIdJoypadL, VirtualKey.Q },
            { InputTypes.DeviceIdJoypadR, VirtualKey.W },
            { InputTypes.DeviceIdJoypadL2, VirtualKey.E },
            { InputTypes.DeviceIdJoypadR2, VirtualKey.R },
            { InputTypes.DeviceIdJoypadL3, VirtualKey.T },
            { InputTypes.DeviceIdJoypadR3, VirtualKey.Y },
            { InputTypes.DeviceIdJoypadSelect, VirtualKey.O },
            { InputTypes.DeviceIdJoypadStart, VirtualKey.P },
        };

        private static readonly ISet<InputTypes> LibretroGamepadAnalogTypes = new HashSet<InputTypes>()
        {
            { InputTypes.DeviceIdAnalogLeftX },
            { InputTypes.DeviceIdAnalogLeftY },
            { InputTypes.DeviceIdAnalogRightX },
            { InputTypes.DeviceIdAnalogRightY },
        };

        private static readonly IReadOnlyDictionary<InputTypes, GamepadButtons> LibretroGamepadToWindowsGamepadButtonMapping = new Dictionary<InputTypes, GamepadButtons>()
        {
            { InputTypes.DeviceIdJoypadUp, GamepadButtons.DPadUp },
            { InputTypes.DeviceIdJoypadDown, GamepadButtons.DPadDown },
            { InputTypes.DeviceIdJoypadLeft, GamepadButtons.DPadLeft },
            { InputTypes.DeviceIdJoypadRight, GamepadButtons.DPadRight },
            { InputTypes.DeviceIdJoypadA, GamepadButtons.B },
            { InputTypes.DeviceIdJoypadB, GamepadButtons.A },
            { InputTypes.DeviceIdJoypadX, GamepadButtons.Y },
            { InputTypes.DeviceIdJoypadY, GamepadButtons.X },
            { InputTypes.DeviceIdJoypadL, GamepadButtons.LeftShoulder },
            { InputTypes.DeviceIdJoypadR, GamepadButtons.RightShoulder },
            { InputTypes.DeviceIdJoypadL2, GamepadButtons.Paddle1 },
            { InputTypes.DeviceIdJoypadR2, GamepadButtons.Paddle2 },
            { InputTypes.DeviceIdJoypadL3, GamepadButtons.LeftThumbstick },
            { InputTypes.DeviceIdJoypadR3, GamepadButtons.RightThumbstick },
            { InputTypes.DeviceIdJoypadSelect, GamepadButtons.View },
            { InputTypes.DeviceIdJoypadStart, GamepadButtons.Menu },
        };

        private readonly Dictionary<InputTypes, uint> InjectedInput = new Dictionary<InputTypes, uint>();
        private readonly Dictionary<VirtualKey, bool> KeyStates = new Dictionary<VirtualKey, bool>();
        private readonly Dictionary<VirtualKey, bool> KeySnapshot = new Dictionary<VirtualKey, bool>();

        private readonly object GamepadReadingsLock = new object();
        private GamepadReading[] GamepadReadings;

        public InputService()
        {
            var window = CoreWindow.GetForCurrentThread();
            window.KeyDown -= WindowKeyDownHandler;
            window.KeyDown += WindowKeyDownHandler;
            window.KeyUp -= WindowKeyUpHandler;
            window.KeyUp += WindowKeyUpHandler;
        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public Task DeinitAsync()
        {
            return Task.CompletedTask;
        }

        public void InjectInputPlayer1(InputTypes inputType)
        {
            lock (InjectedInput)
            {
                InjectedInput[inputType] = InjectedInputFramePermamence;
            }
        }

        public void PollInput()
        {
            lock (KeyStates)
            lock (KeySnapshot)
            {
                foreach (var i in KeyStates.Keys)
                {
                    KeySnapshot[i] = KeyStates[i];
                }
            }

            lock (GamepadReadingsLock)
            {
                GamepadReadings = Gamepad.Gamepads.Select(d => d.GetCurrentReading()).ToArray();
            }
        }

        public short GetInputState(uint port, InputTypes inputType)
        {
            if (!Enum.IsDefined(typeof(InputTypes), inputType))
            {
                return 0;
            }

            lock (GamepadReadingsLock)
            {
                if (LibretroGamepadAnalogTypes.Contains(inputType) && port < GamepadReadings.Length)
                {
                    var reading = GamepadReadings[port];
                    switch (inputType)
                    {
                        case InputTypes.DeviceIdAnalogLeftX:
                            return ConvertAxisReading(reading.LeftThumbstickX, reading.LeftThumbstickY);
                        case InputTypes.DeviceIdAnalogLeftY:
                            return ConvertAxisReading(-reading.LeftThumbstickY, reading.LeftThumbstickX);
                        case InputTypes.DeviceIdAnalogRightX:
                            return ConvertAxisReading(reading.RightThumbstickX, reading.RightThumbstickY);
                        case InputTypes.DeviceIdAnalogRightY:
                            return ConvertAxisReading(-reading.RightThumbstickY, reading.RightThumbstickX);
                    }
                }

                var output = false;
                if (port == 0)
                {
                    lock (KeySnapshot)
                    {
                        output = GetKeyboardKeyState(KeySnapshot, inputType);
                    }
                    output = output || GetInjectedInputState(inputType);
                }

                if (port < GamepadReadings.Length)
                {
                    output = output || GetGamepadButtonState(GamepadReadings[port], inputType);
                }

                return output ? (short)1 : (short)0;
            }
        }

        private bool GetInjectedInputState(InputTypes inputType)
        {
            lock (InjectedInput)
            {
                var output = InjectedInput.Keys.Contains(inputType);
                if (output)
                {
                    output = InjectedInput[inputType] > 0;
                    if (output)
                    {
                        InjectedInput[inputType] -= 1;
                    }
                }

                return output;
            }
        }

        private static bool GetKeyboardKeyState(Dictionary<VirtualKey, bool> keyStates, InputTypes button)
        {
            if (!LibretroGamepadToWindowsGamepadButtonMapping.ContainsKey(button))
            {
                return false;
            }

            var nativeKey = LibretroGamepadToKeyboardKeyMapping[button];
            var output = keyStates.ContainsKey(nativeKey) && keyStates[nativeKey];
            return output;
        }

        private static bool GetGamepadButtonState(GamepadReading reading, InputTypes button)
        {
            if (!LibretroGamepadToWindowsGamepadButtonMapping.ContainsKey(button))
            {
                return false;
            }

            var nativeButton = LibretroGamepadToWindowsGamepadButtonMapping[button];
            var output = (reading.Buttons & nativeButton) == nativeButton;
            return output;
        }

        private static short ConvertAxisReading(double mainValue, double transverseValue)
        {
            var isInDeadZone = (mainValue * mainValue) + (transverseValue * transverseValue) < GamepadAnalogDeadZoneSquareRadius;
            var output = isInDeadZone ? 0 : mainValue * short.MaxValue;
            return (short)output;
        }

        private void WindowKeyUpHandler(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (Enum.IsDefined(typeof(VirtualKey), key))
            {
                lock (KeyStates)
                {
                    KeyStates[args.VirtualKey] = false;
                }
            }
        }

        private void WindowKeyDownHandler(CoreWindow sender, KeyEventArgs args)
        {
            var key = args.VirtualKey;
            if (Enum.IsDefined(typeof(VirtualKey), key))
            {
                lock (KeyStates)
                {
                    KeyStates[args.VirtualKey] = true;
                }
            }
        }
    }
}
