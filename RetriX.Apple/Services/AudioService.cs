using AudioToolbox;
using LibRetriX;
using RetriX.Shared.Services;
using System;
using System.Threading.Tasks;

namespace RetriX.Apple.Services
{
    public sealed class AudioService : IAudioService
    {
        private const int NumChannels = 2;

        private readonly object QueueLock = new object();

        private OutputAudioQueue queue;
        private OutputAudioQueue Queue
        {
            get => queue;
            set { if (queue != value) { queue?.Dispose(); queue = value; } }
        }

        public bool ShouldDelayNextFrame => throw new NotImplementedException();

        public Task DeinitAsync()
        {
            Queue = null;
            return Task.CompletedTask;
        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public void TimingChanged(SystemTimings timings)
        {
            var description = new AudioStreamBasicDescription(AudioFormatType.LinearPCM)
            {
                BitsPerChannel = 8 * sizeof(short),
                FormatFlags = AudioFormatFlags.IsSignedInteger | AudioFormatFlags.IsPacked,
                ChannelsPerFrame = NumChannels,
                FramesPerPacket = 1,
                SampleRate = timings.SampleRate,
                BytesPerFrame = NumChannels * sizeof(short)
            };

            lock (QueueLock)
            {
                Queue?.Stop(true);
                Queue = new OutputAudioQueue(description);
            }
        }

        public uint RenderAudioFrames(ReadOnlySpan<short> data, uint numFrames)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (Queue == null)
            {
                return;
            }

            Queue.Stop(true);
            Queue.Reset();
        }
    }
}