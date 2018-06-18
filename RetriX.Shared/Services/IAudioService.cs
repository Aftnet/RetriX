using LibRetriX;
using System;

namespace RetriX.Shared.Services
{
    public interface IAudioService : IInitializable
    {
        bool ShouldDelayNextFrame { get; }
        void TimingChanged(SystemTimings timings);
        void RenderAudioFrames(ReadOnlySpan<short> data, ulong numFrames);
        void Stop();
    }
}