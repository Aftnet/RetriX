using LibRetriX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RetriX.Shared.Services
{
    public abstract class AudioServiceBase : IAudioService
    {
        protected abstract Task CreateResourcesAsync(uint sampleRate);
        protected abstract void DestroyResources();
        protected abstract void StartPlayback();
        protected abstract void StopPlayback();

        protected const uint NumChannels = 2;
        protected const uint SampleSizeBytes = sizeof(short);

        protected readonly Queue<short> SamplesBuffer = new Queue<short>();

        private const uint NullSampleRate = 0;
        private const uint MaxSamplesQueueSize = 44100 * 4;
        private const float PlaybackDelaySeconds = 0.2f; //Have some buffer to avoid crackling
        private const float MaxAllowedDelaySeconds = 0.4f; //Limit maximum delay

        private int MinNumSamplesForPlayback { get; set; } = 0;
        private int MaxNumSamplesForTargetDelay { get; set; } = 0;

        private Task ResourcesCreationTask { get; set; }
        private bool IsPlaying { get; set; }

        private uint sampleRate = NullSampleRate;
        private uint SampleRate
        {
            get => sampleRate;
            set
            {
                sampleRate = value;
                MinNumSamplesForPlayback = (int)(sampleRate * PlaybackDelaySeconds);
                MaxNumSamplesForTargetDelay = (int)(sampleRate * MaxAllowedDelaySeconds);
            }
        }

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

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public Task DeinitAsync()
        {
            Stop();
            if (ResourcesCreationTask == null)
            {
                DestroyResources();
            }

            SampleRate = NullSampleRate;
            return Task.CompletedTask;
        }

        public async void TimingChanged(SystemTimings timings)
        {
            uint sampleRate = (uint)timings.SampleRate;
            if (SampleRate == sampleRate || ResourcesCreationTask != null)
            {
                return;
            }

            SampleRate = sampleRate;

            Stop();
            DestroyResources();
            try
            {
                ResourcesCreationTask = CreateResourcesAsync(SampleRate);
                await ResourcesCreationTask.ConfigureAwait(false);
                ResourcesCreationTask = null;
            }
            catch
            {
                DestroyResources();
                SampleRate = NullSampleRate;
            }
        }

        public uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames)
        {
            var numSrcSamples = (uint)numFrames * NumChannels;
            var bufferRemainingCapacity = Math.Max(0, MaxSamplesQueueSize - SamplesBuffer.Count);
            var numSamplesToCopy = Math.Min(numSrcSamples, bufferRemainingCapacity);

            lock (SamplesBuffer)
            {
                for (var i = 0; i < numSamplesToCopy; i++)
                {
                    SamplesBuffer.Enqueue(data[i]);
                }

                if (ResourcesCreationTask == null && !IsPlaying && SamplesBuffer.Count >= MinNumSamplesForPlayback)
                {
                    StartPlayback();
                    IsPlaying = true;
                }
            }

            return numFrames;
        }

        public void Stop()
        {
            if (ResourcesCreationTask == null && IsPlaying)
            {
                StopPlayback();
                IsPlaying = false;
            }

            lock (SamplesBuffer)
            {
                SamplesBuffer.Clear();
            }
        }
    }
}
