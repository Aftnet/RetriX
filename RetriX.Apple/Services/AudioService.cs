using AudioToolbox;
using LibRetriX;
using RetriX.Shared.Services;
using System;
using System.Threading.Tasks;

namespace RetriX.Apple.Services
{
    public sealed class AudioService : AudioServiceBase
    {
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
    }
}