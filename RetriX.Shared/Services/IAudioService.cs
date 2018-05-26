using LibRetriX;
using System.Collections.Generic;
using System.IO;

namespace RetriX.Shared.Services
{
    public interface IAudioService : IInitializable
    {
        bool ShouldDelayNextFrame { get; }
        void TimingChanged(SystemTimings timings);
        void RenderAudioFrames(IReadOnlyList<short> data, ulong numFrames);
        void Stop();
    }
}