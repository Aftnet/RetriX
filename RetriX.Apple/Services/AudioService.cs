using AudioToolbox;
using RetriX.Shared.Services;
using System.Threading.Tasks;

namespace RetriX.Apple.Services
{
    public sealed class AudioService : AudioServiceBase
    {
        private OutputAudioQueue queue;
        private OutputAudioQueue Queue
        {
            get => queue;
            set { if (queue != value) { queue?.Dispose(); queue = value; } }
        }

        protected override Task CreateResourcesAsync(uint sampleRate)
        {
            var description = new AudioStreamBasicDescription(AudioFormatType.LinearPCM)
            {
                BitsPerChannel = 8 * sizeof(short),
                FormatFlags = AudioFormatFlags.IsSignedInteger | AudioFormatFlags.IsPacked,
                ChannelsPerFrame = (int)NumChannels,
                FramesPerPacket = 1,
                SampleRate = sampleRate,
                BytesPerFrame = (int)NumChannels * (int)SampleSizeBytes
            };

            Queue = new OutputAudioQueue(description);
            return Task.CompletedTask;
        }

        protected override void DestroyResources()
        {
            Queue = null;
        }

        protected override void StartPlayback()
        {
            Queue.Start();
        }

        protected override void StopPlayback()
        {
            Queue.Stop(true);
            Queue.Reset();
        }
    }
}