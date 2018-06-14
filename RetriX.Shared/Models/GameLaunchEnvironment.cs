using LibRetriX;
using RetriX.Shared.StreamProviders;

namespace RetriX.Shared.Models
{
    public class GameLaunchEnvironment
    {
        public enum GenerateResult { Success, RootFolderRequired, DependenciesUnmet, NoMainFileFound };

        public ICore Core { get; }
        public IStreamProvider StreamProvider { get; }
        public string MainFilePath { get; }

        public GameLaunchEnvironment(ICore core, IStreamProvider streamProvider, string mainFilePath)
        {
            Core = core;
            StreamProvider = streamProvider;
            MainFilePath = mainFilePath;
        }
    }
}
