using LibRetriX;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using Plugin.Settings.Abstractions;
using RetriX.Shared.Services;
using RetriX.Shared.StreamProviders;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class GamePlayerViewModel : MvxViewModel<GamePlayerViewModel.Parameter>
    {
        public class Parameter
        {
            public ICore Core { get; }
            public IStreamProvider StreamProvider { get; }       
            public string MainFilePath { get; }

            public Parameter(ICore core, IStreamProvider streamProvider, string mainFilePath)
            {
                Core = core;
                StreamProvider = streamProvider;
                MainFilePath = mainFilePath;
            }
        }

        private const string CurrentFilterKey = "CurrentFilter";
        private static readonly TimeSpan PriodicChecksInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan UIHidingTime = TimeSpan.FromSeconds(4);

        private IMvxNavigationService NavigationService { get; }
        private IPlatformService PlatformService { get; }
        private IEmulationService EmulationService { get; }
        private IVideoService VideoService { get; }
        private ISettings Settings { get; }

        public IMvxCommand TappedCommand { get; }
        public IMvxCommand PointerMovedCommand { get; }
        public IMvxCommand ToggleFullScreenCommand { get; }

        public IMvxCommand TogglePauseCommand { get; }
        public IMvxCommand ResetCommand { get; }
        public IMvxCommand StopCommand { get; }

        public IMvxCommand SaveStateSlot1 { get; }
        public IMvxCommand SaveStateSlot2 { get; }
        public IMvxCommand SaveStateSlot3 { get; }
        public IMvxCommand SaveStateSlot4 { get; }
        public IMvxCommand SaveStateSlot5 { get; }
        public IMvxCommand SaveStateSlot6 { get; }

        public IMvxCommand LoadStateSlot1 { get; }
        public IMvxCommand LoadStateSlot2 { get; }
        public IMvxCommand LoadStateSlot3 { get; }
        public IMvxCommand LoadStateSlot4 { get; }
        public IMvxCommand LoadStateSlot5 { get; }
        public IMvxCommand LoadStateSlot6 { get; }

        private TextureFilterTypes currentFilter = TextureFilterTypes.NearestNeighbor;
        public TextureFilterTypes CurrentFilter
        {
            get => currentFilter;
            private set
            {
                if (SetProperty(ref currentFilter, value))
                {
                    Settings.AddOrUpdateValue(CurrentFilterKey, CurrentFilter.ToString());
                    VideoService.SetFilter(value);
                    RaisePropertyChanged(nameof(NNFilteringSet));
                    RaisePropertyChanged(nameof(LinearFilteringSet));
                }
            }
        }

        public bool NNFilteringSet => CurrentFilter == TextureFilterTypes.NearestNeighbor;
        public IMvxCommand SetNNFiltering { get; }
        public bool LinearFilteringSet => CurrentFilter == TextureFilterTypes.Bilinear;
        public IMvxCommand SetLinearFiltering { get; }


        public IMvxCommand<InjectedInputTypes> InjectInputCommand { get; }

        private IMvxCommand[] AllCoreCommands { get; }

        private bool coreOperationsAllowed = false;
        public bool CoreOperationsAllowed
        {
            get => coreOperationsAllowed;
            set
            {
                if (SetProperty(ref coreOperationsAllowed, value))
                {
                    foreach (var i in AllCoreCommands)
                    {
                        i.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        public bool FullScreenChangingPossible => PlatformService.FullScreenChangingPossible;
        public bool IsFullScreenMode => PlatformService.IsFullScreenMode;

        private bool shouldDisplayTouchGamepad;
        public bool ShouldDisplayTouchGamepad
        {
            get => shouldDisplayTouchGamepad;
            private set => SetProperty(ref shouldDisplayTouchGamepad, value);
        }

        private bool gameIsPaused;
        public bool GameIsPaused
        {
            get => gameIsPaused;
            set => SetProperty(ref gameIsPaused, value);
        }

        private bool displayPlayerUI;
        public bool DisplayPlayerUI
        {
            get => displayPlayerUI;
            set
            {
                SetProperty(ref displayPlayerUI, value);
                if (value)
                {
                    PlayerUIDisplayTime = DateTimeOffset.UtcNow;
                }
            }
        }

        private Timer PeriodicChecksTimer;
        private DateTimeOffset PlayerUIDisplayTime = DateTimeOffset.UtcNow;
        private DateTimeOffset LastPointerMoveTime = DateTimeOffset.UtcNow;

        public GamePlayerViewModel(IMvxNavigationService navigationService, IPlatformService platformService, IVideoService videoService, IEmulationService emulationService, ISettings settings)
        {
            NavigationService = navigationService;
            PlatformService = platformService;
            EmulationService = emulationService;
            VideoService = videoService;
            Settings = settings;

            ShouldDisplayTouchGamepad = PlatformService.ShouldDisplayTouchGamepad;

            TappedCommand = new MvxCommand(() =>
            {
                DisplayPlayerUI = !DisplayPlayerUI;
            });

            PointerMovedCommand = new MvxCommand(() =>
            {
                PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
                LastPointerMoveTime = DateTimeOffset.UtcNow;
                DisplayPlayerUI = true;
            });

            ToggleFullScreenCommand = new MvxCommand(() => RequestFullScreenChange(FullScreenChangeType.Toggle));

            TogglePauseCommand = new MvxCommand(() => { var task = TogglePause(false); }, () => CoreOperationsAllowed);
            ResetCommand = new MvxCommand(Reset, () => CoreOperationsAllowed);
            StopCommand = new MvxCommand(Stop, () => CoreOperationsAllowed);

            SaveStateSlot1 = new MvxCommand(() => SaveState(1), () => CoreOperationsAllowed);
            SaveStateSlot2 = new MvxCommand(() => SaveState(2), () => CoreOperationsAllowed);
            SaveStateSlot3 = new MvxCommand(() => SaveState(3), () => CoreOperationsAllowed);
            SaveStateSlot4 = new MvxCommand(() => SaveState(4), () => CoreOperationsAllowed);
            SaveStateSlot5 = new MvxCommand(() => SaveState(5), () => CoreOperationsAllowed);
            SaveStateSlot6 = new MvxCommand(() => SaveState(6), () => CoreOperationsAllowed);

            LoadStateSlot1 = new MvxCommand(() => LoadState(1), () => CoreOperationsAllowed);
            LoadStateSlot2 = new MvxCommand(() => LoadState(2), () => CoreOperationsAllowed);
            LoadStateSlot3 = new MvxCommand(() => LoadState(3), () => CoreOperationsAllowed);
            LoadStateSlot4 = new MvxCommand(() => LoadState(4), () => CoreOperationsAllowed);
            LoadStateSlot5 = new MvxCommand(() => LoadState(5), () => CoreOperationsAllowed);
            LoadStateSlot6 = new MvxCommand(() => LoadState(6), () => CoreOperationsAllowed);

            SetNNFiltering = new MvxCommand(() => CurrentFilter = TextureFilterTypes.NearestNeighbor);
            SetLinearFiltering = new MvxCommand(() => CurrentFilter = TextureFilterTypes.Bilinear);

            InjectInputCommand = new MvxCommand<InjectedInputTypes>(d => EmulationService.InjectInputPlayer1(d));

            AllCoreCommands = new IMvxCommand[] { TogglePauseCommand, ResetCommand, StopCommand,
                SaveStateSlot1, SaveStateSlot2, SaveStateSlot3, SaveStateSlot4, SaveStateSlot5, SaveStateSlot6,
                LoadStateSlot1, LoadStateSlot2, LoadStateSlot3, LoadStateSlot4, LoadStateSlot5, LoadStateSlot6
            };

            PlatformService.FullScreenChangeRequested += (d, e) => RequestFullScreenChange(e.Type);
            PlatformService.PauseToggleRequested += OnPauseToggleKey;
            PlatformService.GameStateOperationRequested += OnGameStateOperationRequested;

            var parseSuccessful = Enum.TryParse<TextureFilterTypes>(Settings.GetValueOrDefault(CurrentFilterKey, string.Empty), out var parsedFilter);
            if (parseSuccessful)
            {
                currentFilter = parsedFilter;
            }

            VideoService.SetFilter(CurrentFilter);
        }


        public override void Prepare(Parameter parameter)
        {
            EmulationService.StartGameAsync(parameter.Core, parameter.StreamProvider, parameter.MainFilePath);
        }

        private async void RequestFullScreenChange(FullScreenChangeType fullScreenChangeType)
        {
            await PlatformService.ChangeFullScreenStateAsync(fullScreenChangeType);
            RaisePropertyChanged(nameof(IsFullScreenMode));
        }

        public override void ViewAppeared()
        {
            CoreOperationsAllowed = true;
            PlatformService.HandleGameplayKeyShortcuts = true;
            DisplayPlayerUI = true;
            PeriodicChecksTimer = new Timer(d => PeriodicChecks(), null, PriodicChecksInterval, PriodicChecksInterval);
        }

        public override void ViewDisappearing()
        {
            EmulationService.StopGameAsync();
            PeriodicChecksTimer.Dispose();
            CoreOperationsAllowed = false;
            PlatformService.HandleGameplayKeyShortcuts = false;
            PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Visible);
            PlatformService.ChangeFullScreenStateAsync(FullScreenChangeType.Exit);
        }

        private async Task TogglePause(bool dismissOverlayImmediately)
        {
            if (!CoreOperationsAllowed)
            {
                return;
            }

            CoreOperationsAllowed = false;

            if (GameIsPaused)
            {
                await EmulationService.ResumeGameAsync();
                if (dismissOverlayImmediately)
                {
                    DisplayPlayerUI = false;
                }
            }
            else
            {
                await EmulationService.PauseGameAsync();
                DisplayPlayerUI = true;
            }

            GameIsPaused = !GameIsPaused;
            CoreOperationsAllowed = true;
        }

        private async void OnPauseToggleKey(object sender, EventArgs args)
        {
            await TogglePause(true);
            if (GameIsPaused)
            {
                PlatformService.ForceUIElementFocus();
            }
        }

        private async void Reset()
        {
            CoreOperationsAllowed = false;
            await EmulationService.ResetGameAsync();
            CoreOperationsAllowed = true;

            if (GameIsPaused)
            {
                await TogglePause(true);
            }
        }

        private void Stop()
        {
            NavigationService.Close(this);
        }

        private async void SaveState(uint slotID)
        {
            CoreOperationsAllowed = false;
            await EmulationService.SaveGameStateAsync(slotID);
            CoreOperationsAllowed = true;

            if (GameIsPaused)
            {
                await TogglePause(true);
            }
        }

        private async void LoadState(uint slotID)
        {
            CoreOperationsAllowed = false;
            await EmulationService.LoadGameStateAsync(slotID);
            CoreOperationsAllowed = true;

            if (GameIsPaused)
            {
                await TogglePause(true);
            }
        }

        private void OnGameStateOperationRequested(object sender, GameStateOperationEventArgs args)
        {
            if (!CoreOperationsAllowed)
            {
                return;
            }

            if (args.Type == GameStateOperationEventArgs.GameStateOperationType.Load)
            {
                LoadState(args.SlotID);
            }
            else
            {
                SaveState(args.SlotID);
            }
        }

        private void PeriodicChecks()
        {
            var displayTouchGamepad = PlatformService.ShouldDisplayTouchGamepad;
            if (ShouldDisplayTouchGamepad != displayTouchGamepad)
            {
                Dispatcher.RequestMainThreadAction(() => ShouldDisplayTouchGamepad = displayTouchGamepad);
            }

            if (GameIsPaused)
            {
                return;
            }

            var currentTime = DateTimeOffset.UtcNow;

            if (currentTime.Subtract(LastPointerMoveTime).CompareTo(UIHidingTime) >= 0)
            {
                Dispatcher.RequestMainThreadAction(() => PlatformService.ChangeMousePointerVisibility(MousePointerVisibility.Hidden));
            }

            if (currentTime.Subtract(PlayerUIDisplayTime).CompareTo(UIHidingTime) >= 0)
            {
                Dispatcher.RequestMainThreadAction(() => DisplayPlayerUI = false);
            }
        }
    }
}
