using AudioToolbox;
using RetriX.Shared.Services;
using System;
using System.Threading.Tasks;

namespace RetriX.Apple.Services
{
    public unsafe sealed class AudioService : AudioServiceBase
    {
        private const int NumQueueBuffers = 2;
        private const float BufferLengthSecs = 0.1f;
        private const int BufferLengthBytes = (int)(BufferLengthSecs * (float)NumChannels * (float)SampleSizeBytes);

        private AudioQueueBuffer*[] QueueBuffers { get; set; } = new AudioQueueBuffer*[0];

        private OutputAudioQueue queue;
        private OutputAudioQueue Queue
        {
            get => queue;
            set
            {
                if (queue != value)
                {
                    queue?.Dispose();
                    queue = value;
                }
            }
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
            Queue.BufferCompleted += OnQueueBufferCompleted;
            QueueBuffers = new AudioQueueBuffer*[NumQueueBuffers];
            for (var i = 0; i < NumQueueBuffers; i++)
            {
                Queue.AllocateBuffer(BufferLengthBytes, out AudioQueueBuffer* bufPtr);
                QueueBuffers[i] = bufPtr;
            }

            return Task.CompletedTask;
        }

        protected override void DestroyResources()
        {
            foreach (var i in QueueBuffers)
            {
                queue.FreeBuffer(new IntPtr(i));
            }

            QueueBuffers = new AudioQueueBuffer*[0];
            Queue = null;
        }

        protected override void StartPlayback()
        {
            foreach (var i in QueueBuffers)
            {
                FillAudioQueueBuffer(i);
                Queue.EnqueueBuffer(i, null);
            }

            Queue.Start();
        }

        protected override void StopPlayback()
        {
            Queue.Stop(true);
            Queue.Reset();
        }

        private void OnQueueBufferCompleted(object sender, BufferCompletedEventArgs e)
        {
            var outBuffer = e.UnsafeBuffer;
            FillAudioQueueBuffer(outBuffer);
            Queue.EnqueueBuffer(outBuffer, null);
        }

        private void FillAudioQueueBuffer(AudioQueueBuffer* buffer)
        {
            var outBufferNumSamples = buffer->AudioDataByteSize / SampleSizeBytes;

            var outSpan = new Span<short>((void*)buffer->AudioData, (int)outBufferNumSamples);
            lock (SamplesBuffer)
            {
                for (var i = 0; i < Math.Min(outBufferNumSamples, SamplesBuffer.Count); i++)
                {
                    outSpan[i] = SamplesBuffer.Dequeue();
                }
            }
        }
    }
}