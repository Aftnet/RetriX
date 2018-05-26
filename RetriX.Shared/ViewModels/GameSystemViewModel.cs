using LibRetriX;
using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Resources;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RetriX.Shared.ViewModels
{
    public class GameSystemViewModel
    {
        private static HashSet<string> CDImageExtensions { get; } = new HashSet<string> { ".bin", ".cue", ".iso", ".mds", ".mdf" };

        public static GameSystemViewModel MakeNES(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameNES, Strings.ManufacturerNameNintendo, "\uf118");
        public static GameSystemViewModel MakeSNES(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameSNES, Strings.ManufacturerNameNintendo, "\uf119");
        public static GameSystemViewModel MakeN64(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameNintendo64, Strings.ManufacturerNameNintendo, "\uf116");
        public static GameSystemViewModel MakeGB(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameBoy, Strings.ManufacturerNameNintendo, "\uf11b");
        public static GameSystemViewModel MakeGBA(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameBoyAdvance, Strings.ManufacturerNameNintendo, "\uf115");
        public static GameSystemViewModel MakeDS(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameDS, Strings.ManufacturerNameNintendo, "\uf117");
        public static GameSystemViewModel MakeSG1000(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameSG1000, Strings.ManufacturerNameSega, "\uf102", true, new HashSet<string> { ".sg" });
        public static GameSystemViewModel MakeMasterSystem(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameMasterSystem, Strings.ManufacturerNameSega, "\uf118", true, new HashSet<string> { ".sms" });
        public static GameSystemViewModel MakeGameGear(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameGameGear, Strings.ManufacturerNameSega, "\uf129", true, new HashSet<string> { ".gg" });
        public static GameSystemViewModel MakeMegaDrive(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameMegaDrive, Strings.ManufacturerNameSega, "\uf124", true, new HashSet<string> { ".mds", ".md", ".smd", ".gen" });
        public static GameSystemViewModel MakeMegaCD(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameMegaCD, Strings.ManufacturerNameSega, "\uf124", false, new HashSet<string> { ".bin", ".cue", ".iso", ".chd" }, CDImageExtensions);
        public static GameSystemViewModel MakeSaturn(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameSaturn, Strings.ManufacturerNameSega, "\uf124", false, null, CDImageExtensions);
        public static GameSystemViewModel MakePlayStation(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNamePlayStation, Strings.ManufacturerNameSony, "\uf128", false, null, CDImageExtensions);
        public static GameSystemViewModel MakePCEngine(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCEngine, Strings.ManufacturerNameNEC, "\uf124", true, new HashSet<string> { ".pce" });
        public static GameSystemViewModel MakePCEngineCD(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCEngineCD, Strings.ManufacturerNameNEC, "\uf124", false, new HashSet<string> { ".cue", ".ccd", ".chd" }, CDImageExtensions);
        public static GameSystemViewModel MakePCFX(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNamePCFX, Strings.ManufacturerNameNEC, "\uf124", false, null, CDImageExtensions);
        public static GameSystemViewModel MakeWonderSwan(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameWonderSwan, Strings.ManufacturerNameBandai, "\uf129");
        public static GameSystemViewModel MakeNeoGeo(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameNeoGeo, Strings.ManufacturerNameSNK, "\uf102", false);
        public static GameSystemViewModel MakeNeoGeoPocket(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameNeoGeoPocket, Strings.ManufacturerNameSNK, "\uf129");
        public static GameSystemViewModel MakeArcade(ICore core, IFileSystem fileSystem) => new GameSystemViewModel(core, fileSystem, Strings.SystemNameArcade, Strings.ManufacturerNameFBAlpha, "\uf102", true);

        private IFileSystem FileSystem { get; }

        public ICore Core { get; }

        public string Name { get; }
        public string Manufacturer { get; }
        public string Symbol { get; }
        public IEnumerable<string> SupportedExtensions { get; }
        public IEnumerable<string> MultiFileExtensions { get; }

        private readonly bool DependenciesOverride;

        private GameSystemViewModel(ICore core, IFileSystem fileSystem, string name, string manufacturer, string symbol, bool dependenciesOverride = false, IEnumerable<string> supportedExtensionsOverride = null, IEnumerable<string> multiFileExtensions = null)
        {
            FileSystem = fileSystem;

            Core = core;
            Name = name;
            Manufacturer = manufacturer;
            Symbol = symbol;
            SupportedExtensions = supportedExtensionsOverride != null ? supportedExtensionsOverride : Core.SupportedExtensions;
            MultiFileExtensions = multiFileExtensions == null ? new string[0] : multiFileExtensions;
            DependenciesOverride = dependenciesOverride;
        }

        public bool CheckRootFolderRequired(IFileInfo file)
        {
            var extension = Path.GetExtension(file.Name);
            return MultiFileExtensions.Contains(extension);
        }

        public async Task<bool> CheckDependenciesMetAsync()
        {
            if (DependenciesOverride)
            {
                return true;
            }

            var systemFolder = await GetSystemDirectoryAsync();
            foreach (var i in Core.FileDependencies)
            {
                var file = await systemFolder.GetFileAsync(i.Name);
                if (file == null)
                {
                    return false;
                }
            }

            return true;
        }

        public Task<IDirectoryInfo> GetSystemDirectoryAsync()
        {
            return GetCoreStorageDirectoryAsync($"{Core.Name} - System");
        }

        public Task<IDirectoryInfo> GetSaveDirectoryAsync()
        {
            return GetCoreStorageDirectoryAsync($"{Core.Name} - Saves");
        }

        private async Task<IDirectoryInfo> GetCoreStorageDirectoryAsync(string directoryName)
        {
            var output = await FileSystem.LocalStorage.GetDirectoryAsync(directoryName);
            if (output == null)
            {
                output = await FileSystem.LocalStorage.CreateDirectoryAsync(directoryName);
            }

            return output;
        }
    }
}
