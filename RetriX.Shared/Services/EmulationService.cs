using Acr.UserDialogs;
using LibRetriX;
using MvvmCross.Platform.Core;
using Plugin.FileSystem.Abstractions;
using Plugin.LocalNotifications.Abstractions;
using RetriX.Shared.StreamProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public class EmulationService : IEmulationService
    {
        private static readonly IReadOnlyDictionary<InjectedInputTypes, InputTypes> InjectedInputMapping = new Dictionary<InjectedInputTypes, InputTypes>
        {
            { InjectedInputTypes.DeviceIdJoypadA, InputTypes.DeviceIdJoypadA },
            { InjectedInputTypes.DeviceIdJoypadB, InputTypes.DeviceIdJoypadB },
            { InjectedInputTypes.DeviceIdJoypadDown, InputTypes.DeviceIdJoypadDown },
            { InjectedInputTypes.DeviceIdJoypadLeft, InputTypes.DeviceIdJoypadLeft },
            { InjectedInputTypes.DeviceIdJoypadRight, InputTypes.DeviceIdJoypadRight },
            { InjectedInputTypes.DeviceIdJoypadSelect, InputTypes.DeviceIdJoypadSelect },
            { InjectedInputTypes.DeviceIdJoypadStart, InputTypes.DeviceIdJoypadStart },
            { InjectedInputTypes.DeviceIdJoypadUp, InputTypes.DeviceIdJoypadUp },
            { InjectedInputTypes.DeviceIdJoypadX, InputTypes.DeviceIdJoypadX },
            { InjectedInputTypes.DeviceIdJoypadY, InputTypes.DeviceIdJoypadY },
        };

        private IPlatformService PlatformService { get; }
        private ISaveStateService SaveStateService { get; }
        private ILocalNotifications NotificationService { get; }
        private IMvxMainThreadDispatcher Dispatcher { get; }

        private IVideoService VideoService { get; }
        private IAudioService AudioService { get; }
        private IInputService InputService { get; }

        private bool CorePaused { get; set; } = false;
        private bool StartStopOperationInProgress { get; set; } = false;

        private SemaphoreSlim CoreSemaphore { get; } = new SemaphoreSlim(1, 1);

        private ICore currentCore;
        private ICore CurrentCore
        {
            get => currentCore;
            set
            {
                if (currentCore == value)
                {
                    return;
                }

                if (currentCore != null)
                {
                    currentCore.GeometryChanged -= VideoService.GeometryChanged;
                    currentCore.PixelFormatChanged -= VideoService.PixelFormatChanged;
                    currentCore.RenderVideoFrameRGB0555 -= VideoService.RenderVideoFrameRGB0555;
                    currentCore.RenderVideoFrameRGB565 -= VideoService.RenderVideoFrameRGB565;
                    currentCore.RenderVideoFrameXRGB8888 -= VideoService.RenderVideoFrameXRGB8888;
                    currentCore.TimingsChanged -= VideoService.TimingsChanged;
                    currentCore.RotationChanged -= VideoService.RotationChanged;
                    currentCore.TimingsChanged -= AudioService.TimingChanged;
                    currentCore.RenderAudioFrames -= AudioService.RenderAudioFrames;
                    currentCore.PollInput = null;
                    currentCore.GetInputState = null;
                    currentCore.OpenFileStream = null;
                    currentCore.CloseFileStream = null;
                }

                currentCore = value;

                if (currentCore != null)
                {
                    currentCore.GeometryChanged += VideoService.GeometryChanged;
                    currentCore.PixelFormatChanged += VideoService.PixelFormatChanged;
                    currentCore.RenderVideoFrameRGB0555 += VideoService.RenderVideoFrameRGB0555;
                    currentCore.RenderVideoFrameRGB565 += VideoService.RenderVideoFrameRGB565;
                    currentCore.RenderVideoFrameXRGB8888 += VideoService.RenderVideoFrameXRGB8888;
                    currentCore.TimingsChanged += VideoService.TimingsChanged;
                    currentCore.RotationChanged += VideoService.RotationChanged;
                    currentCore.TimingsChanged += AudioService.TimingChanged;
                    currentCore.RenderAudioFrames += AudioService.RenderAudioFrames;
                    currentCore.PollInput = InputService.PollInput;
                    currentCore.GetInputState = InputService.GetInputState;
                    currentCore.OpenFileStream = OnCoreOpenFileStream;
                    currentCore.CloseFileStream = OnCoreCloseFileStream;
                }
            }
        }

        private IStreamProvider streamProvider;
        private IStreamProvider StreamProvider
        {
            get => streamProvider;
            set { if (streamProvider != value) streamProvider?.Dispose(); streamProvider = value; }
        }

        public event EventHandler GameStarted;
        public event EventHandler GameStopped;
        public event EventHandler<Exception> GameRuntimeExceptionOccurred;

        public EmulationService(IFileSystem fileSystem, IUserDialogs dialogsService,
            IPlatformService platformService, ISaveStateService saveStateService,
            ILocalNotifications notificationService, ICryptographyService cryptographyService,
            IVideoService videoService, IAudioService audioService,
            IInputService inputService, IMvxMainThreadDispatcher dispatcher)
        {
            PlatformService = platformService;
            SaveStateService = saveStateService;
            NotificationService = notificationService;
            Dispatcher = dispatcher;

            VideoService = videoService;
            AudioService = audioService;
            InputService = inputService;

            VideoService.RequestRunCoreFrame += OnRunFrameRequested;
        }

        public async Task<bool> StartGameAsync(ICore core, IStreamProvider streamProvider, string mainFilePath)
        {
            if (StartStopOperationInProgress)
            {
                return false;
            }

            StartStopOperationInProgress = true;

            var initTasks = new Task[] { InputService.InitAsync(), AudioService.InitAsync(), VideoService.InitAsync() };
            await Task.WhenAll(initTasks);

            var loadSuccessful = false;
            await CoreSemaphore.WaitAsync();
            try
            {
                if (CurrentCore != null)
                {
                    await Task.Run(() => CurrentCore.UnloadGame());
                }

                StreamProvider = streamProvider;
                SaveStateService.SetGameId(mainFilePath);
                CorePaused = false;
                CurrentCore = core;

                loadSuccessful = await Task.Run(() =>
                {
                    try
                    {
                        return CurrentCore.LoadGame(mainFilePath);
                    }
                    catch
                    {
                        return false;
                    }
                });

                if (!loadSuccessful)
                {
                    await StopGameAsyncInternal();
                    StartStopOperationInProgress = false;
                    return loadSuccessful;
                }
            }
            finally
            {
                CoreSemaphore.Release();
            }

            GameStarted?.Invoke(this, EventArgs.Empty);
            StartStopOperationInProgress = false;
            return loadSuccessful;
        }

        public async Task ResetGameAsync()
        {
            await CoreSemaphore.WaitAsync();
            try
            {
                if (CurrentCore != null)
                {
                    await Task.Run(() => CurrentCore.Reset());
                }
            }
            finally
            {
                CoreSemaphore.Release();
            }
        }

        public async Task StopGameAsync()
        {
            if (StartStopOperationInProgress)
            {
                return;
            }

            StartStopOperationInProgress = true;

            await CoreSemaphore.WaitAsync();
            try
            {
                await StopGameAsyncInternal();
            }
            finally
            {
                CoreSemaphore.Release();
            }

            GameStopped?.Invoke(this, EventArgs.Empty);
            StartStopOperationInProgress = false;
        }

        private async Task StopGameAsyncInternal()
        {
            if (CurrentCore != null)
            {
                await Task.Run(() => CurrentCore.UnloadGame());
                CurrentCore = null;
            }

            SaveStateService.SetGameId(null);
            StreamProvider = null;

            var initTasks = new Task[] { InputService.DeinitAsync(), AudioService.DeinitAsync(), VideoService.DeinitAsync() };
            await Task.WhenAll(initTasks);
        }

        public Task PauseGameAsync()
        {
            return SetCorePaused(true);
        }

        public Task ResumeGameAsync()
        {
            return SetCorePaused(false);
        }

        private async Task SetCorePaused(bool value)
        {
            await CoreSemaphore.WaitAsync();
            if (value)
            {
                await Task.Run(() => AudioService.Stop());
            }

            CorePaused = value;
            CoreSemaphore.Release();
        }

        public async Task<bool> SaveGameStateAsync(uint slotID)
        {
            var success = false;
            if (CurrentCore == null)
            {
                return success;
            }

            using (var stream = await SaveStateService.GetStreamForSlotAsync(slotID, FileAccess.ReadWrite))
            {
                if (stream == null)
                {
                    return success;
                }

                await CoreSemaphore.WaitAsync();
                try
                {
                    success = await Task.Run(() => CurrentCore.SaveState(stream));
                }
                finally
                {
                    CoreSemaphore.Release();
                }

                await stream.FlushAsync();
            }

            if (success)
            {
                var notificationBody = string.Format(Resources.Strings.StateSavedToSlotMessageBody, slotID);
                NotificationService.Show(Resources.Strings.StateSavedToSlotMessageTitle, notificationBody);
            }

            return success;
        }

        public async Task<bool> LoadGameStateAsync(uint slotID)
        {
            var success = false;
            if (CurrentCore == null)
            {
                return success;
            }

            using (var stream = await SaveStateService.GetStreamForSlotAsync(slotID, FileAccess.Read))
            {
                if (stream == null)
                {
                    return false;
                }

                await CoreSemaphore.WaitAsync();
                try
                {
                    success = await Task.Run(() => CurrentCore.LoadState(stream));
                }
                finally
                {
                    CoreSemaphore.Release();
                }
            }

            return success;
        }

        public void InjectInputPlayer1(InjectedInputTypes inputType)
        {
            InputService.InjectInputPlayer1(InjectedInputMapping[inputType]);
        }

        //Synhronous since it's going to be called by a non UI thread
        private async void OnRunFrameRequested(object sender, EventArgs args)
        {
            if (CurrentCore == null || CorePaused || AudioService.ShouldDelayNextFrame)
            {
                return;
            }

            await CoreSemaphore.WaitAsync();
            try
            {
                CurrentCore.RunFrame();
            }
            catch (Exception e)
            {
                if (!StartStopOperationInProgress)
                {
                    StartStopOperationInProgress = true;
                    StopGameAsyncInternal().Wait();
                    StartStopOperationInProgress = false;
                }

                var task = Dispatcher.RequestMainThreadAction(() => GameRuntimeExceptionOccurred?.Invoke(this, e));
            }
            finally
            {
                CoreSemaphore.Release();
            }
        }

        private Stream OnCoreOpenFileStream(string path, FileAccess fileAccess)
        {
            var stream = StreamProvider.OpenFileStreamAsync(path, fileAccess).Result;
            return stream;
        }

        private void OnCoreCloseFileStream(Stream stream)
        {
            StreamProvider.CloseStream(stream);
        }        
    }
}
