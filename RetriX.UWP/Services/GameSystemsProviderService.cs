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

        protected override IEnumerable<GameSystemViewModel> GenerateSystemsList(IFileSystem fileSystem)
        {
            yield return GameSystemViewModel.MakeNES(LibRetriX.FCEUMM.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeSNES(LibRetriX.Snes9X.Core.Instance, fileSystem);
            //yield return GameSystemViewModel.MakeN64(LibRetriX.ParallelN64.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeGB(LibRetriX.Gambatte.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeGBA(LibRetriX.VBAM.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeDS(LibRetriX.MelonDS.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeSG1000(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeMasterSystem(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeGameGear(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeMegaDrive(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeMegaCD(LibRetriX.GenesisPlusGX.Core.Instance, fileSystem);
            //yield return GameSystemViewModel.Make32X(LibRetriX.PicoDrive.Core.Instance, fileSystem);
            //yield return GameSystemViewModel.MakeSaturn(LibRetriX.BeetleSaturn.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakePlayStation(LibRetriX.BeetlePSX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakePCEngine(LibRetriX.BeetlePCEFast.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakePCEngineCD(LibRetriX.BeetlePCEFast.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakePCFX(LibRetriX.BeetlePCFX.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeWonderSwan(LibRetriX.BeetleWSwan.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeArcade(LibRetriX.FBAlpha.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeNeoGeoPocket(LibRetriX.BeetleNGP.Core.Instance, fileSystem);
            yield return GameSystemViewModel.MakeNeoGeo(LibRetriX.FBAlpha.Core.Instance, fileSystem);
        }
    }
}
