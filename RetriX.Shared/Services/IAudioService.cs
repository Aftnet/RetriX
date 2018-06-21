using LibRetriX;
using System;

namespace RetriX.Shared.Services
{
    public interface IAudioService : IInitializable
    {
        bool ShouldDelayNextFrame { get; }
        void TimingChanged(SystemTimings timings);
        uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames);
        void Stop();
    }
}