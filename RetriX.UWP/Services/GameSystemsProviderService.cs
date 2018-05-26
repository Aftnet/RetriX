using Plugin.FileSystem.Abstractions;
using RetriX.Shared.Services;
using RetriX.Shared.ViewModels;
using System.Collections.Generic;

namespace RetriX.UWP.Services
{
    public class GameSystemsProviderService : GameSystemsProviderServiceBase
    {
        public GameSystemsProviderService(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        protected override IReadOnlyList<GameSystemViewModel> GenerateSystemsList(IFileSystem fileSystem)
        {
            return new GameSystemViewModel[]
            {
                GameSystemViewModel.MakeNES(LibRetriX.FCEUMM.Core.Instance, fileSystem),
                GameSystemViewModel.MakeSNES(LibRetriX.Snes9X.Core.Instance, fileSystem),
                //GameSystemViewModel.MakeN64(LibRetriX.ParallelN64.Core.Instance, fileSystem),
                GameSystemViewModel.MakeGB(LibRetriX.Gambatte.Core.Instance, fileSystem),
                GameSystemViewModel.MakeGBA(LibRetriX.VBAM.Core.Instance, fileSystem),
                GameSystemViewModel.MakeDS(LibRetriX.MelonDS.Core.Instance, fileSystem),
                GameSystemViewModel.MakeSG1000(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem),
                GameSystemViewModel.MakeMasterSystem(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem),
                GameSystemViewModel.MakeGameGear(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem),
                GameSystemViewModel.MakeMegaDrive(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem),
                GameSystemViewModel.MakeMegaCD(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem),
                //GameSystemViewModel.MakeSaturn(LibRetriX.BeetleSaturn.Core.Instance, fileSystem),
                GameSystemViewModel.MakePlayStation(LibRetriX.BeetlePSX.Core.Instance, fileSystem),
                GameSystemViewModel.MakePCEngine(LibRetriX.BeetlePCEFast.Core.Instance, fileSystem),
                GameSystemViewModel.MakePCEngineCD(LibRetriX.BeetlePCEFast.Core.Instance, fileSystem),
                GameSystemViewModel.MakePCFX(LibRetriX.BeetlePCFX.Core.Instance, fileSystem),
                GameSystemViewModel.MakeWonderSwan(LibRetriX.BeetleWSwan.Core.Instance, fileSystem),
                GameSystemViewModel.MakeArcade(LibRetriX.FBAlpha.Core.Instance, fileSystem),
                GameSystemViewModel.MakeNeoGeoPocket(LibRetriX.BeetleNGP.Core.Instance, fileSystem),
                GameSystemViewModel.MakeNeoGeo(LibRetriX.FBAlpha.Core.Instance, fileSystem),
            };
        }
    }
}
