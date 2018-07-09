using LibRetriX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public abstract class AudioServiceBase : IAudioService
    {
        protected abstract bool AllowPlaybackControl { get; }
        protected abstract void StartPlayback();
        public abstract Task InitAsync();
        public abstract Task DeinitAsync();
        public abstract void TimingChanged(SystemTimings timings);

        protected const uint NullSampleRate = 0;
        protected const uint MaxSamplesQueueSize = 44100 * 4;
        protected const uint NumChannels = 2;
        protected const float PlaybackDelaySeconds = 0.2f; //Have some buffer to avoid crackling
        protected const float MaxAllowedDelaySeconds = 0.4f; //Limit maximum delay

        protected int MinNumSamplesForPlayback { get; private set; } = 0;
        protected int MaxNumSamplesForTargetDelay { get; private set; } = 0;

        private uint sampleRate = NullSampleRate;
        protected uint SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                MinNumSamplesForPlayback = (int)(sampleRate * PlaybackDelaySeconds);
                MaxNumSamplesForTargetDelay = (int)(sampleRate * MaxAllowedDelaySeconds);
            }
        }

        protected readonly Queue<short> SamplesBuffer = new Queue<short>();

        public bool ShouldDelayNextFrame
        {
            get
            {
                if (SampleRate == NullSampleRate)
                {
                    return false; //Allow core a chance to init timings by runnning
                }

                lock (SamplesBuffer)
                {
                    return SamplesBuffer.Count >= MaxNumSamplesForTargetDelay;
                }
            }
        }

        public uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames)
        {
            if (!AllowPlaybackControl)
            {
                return numFrames;
            }

            var numSrcSamples = (uint)numFrames * NumChannels;
            var bufferRemainingCapacity = Math.Max(0, MaxSamplesQueueSize - SamplesBuffer.Count);
            var numSamplesToCopy = Math.Min(numSrcSamples, bufferRemainingCapacity);

            lock (SamplesBuffer)
            {
                for (var i = 0; i < numSamplesToCopy; i++)
                {
                    SamplesBuffer.Enqueue(data[i]);
                }

                if (SamplesBuffer.Count >= MinNumSamplesForPlayback)
                {
                    StartPlayback();
                }
            }

            return numFrames;
        }

        public virtual void Stop()
        {
            if (!AllowPlaybackControl)
            {
                return;
            }

            lock (SamplesBuffer)
            {
                SamplesBuffer.Clear();
            }
        }
    }
}
