using LibRetriX;
using RetriX.Shared.Services;
using RetriX.UWP.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;

namespace RetriX.UWP.Services
{
    public sealed class AudioService : IAudioService
    {
        private const uint NullSampleRate = 0;
        private const uint MaxSamplesQueueSize = 44100 * 4;
        private const uint NumChannels = 2;
        private const float PlaybackDelaySeconds = 0.2f; //Have some buffer to avoid crackling
        private const float MaxAllowedDelaySeconds = 0.4f; //Limit maximum delay

        private int MinNumSamplesForPlayback = 0;
        private int MaxNumSamplesForTargetDelay = 0;

        private uint SampleRate { get; set; }

        public bool ShouldDelayNextFrame
        {
            get
            {
                if (SampleRate == NullSampleRate)
                    return false; //Allow core a chance to init timings by runnning

                lock (SamplesBuffer)
                {
                    return SamplesBuffer.Count >= MaxNumSamplesForTargetDelay;
                }
            }
        }

        private readonly Queue<short> SamplesBuffer = new Queue<short>();

        private bool GraphReconstructionInProgress = false;
        private bool AllowPlaybackControl => !GraphReconstructionInProgress && InputNode != null;

        private AudioGraph graph;
        private AudioGraph Graph
        {
            get => graph;
            set { if (graph != value) { graph?.Dispose(); graph = value; } }
        }

        private AudioDeviceOutputNode outputNode;
        private AudioDeviceOutputNode OutputNode
        {
            get => outputNode;
            set { if (outputNode != value) { outputNode?.Dispose(); outputNode = value; } }
        }

        private AudioFrameInputNode inputNode;
        private AudioFrameInputNode InputNode
        {
            get => inputNode;
            set { if (inputNode != value) { inputNode?.Dispose(); inputNode = value; } }
        }

        public AudioService()
        {
            SampleRate = NullSampleRate;
        }

        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public Task DeinitAsync()
        {
            Stop();
            DisposeGraph();
            return Task.CompletedTask;
        }

        public void TimingChanged(SystemTimings timings)
        {
            uint sampleRate = (uint)timings.SampleRate;
            if (SampleRate == sampleRate || GraphReconstructionInProgress)
                return;

            Stop();
            var operation = ReconstructGraph(sampleRate);
        }

        public void RenderAudioFrames(IReadOnlyList<short> data, ulong numFrames)
        {
            if (!AllowPlaybackControl)
                return;

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
                    Graph.Start();
                }
            }
        }

        public void Stop()
        {
            if (!AllowPlaybackControl)
                return;

            Graph.Stop();

            lock (SamplesBuffer)
            {
                SamplesBuffer.Clear();
            }
        }

        private async Task ReconstructGraph(uint sampleRate)
        {
            GraphReconstructionInProgress = true;

            if (sampleRate == NullSampleRate) //If invalid sample rate do not create graph but return to allow trying again
            {
                GraphReconstructionInProgress = false;
                return;
            }

            DisposeGraph();

            SampleRate = sampleRate;
            MinNumSamplesForPlayback = (int)(SampleRate * PlaybackDelaySeconds);
            MaxNumSamplesForTargetDelay = (int)(SampleRate * MaxAllowedDelaySeconds);

            var graphResult = await AudioGraph.CreateAsync(new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.GameMedia));
            if (graphResult.Status != AudioGraphCreationStatus.Success)
            {
                DisposeGraph();
                throw new Exception($"Unable to create audio graph: {graphResult.Status.ToString()}");
            }
            Graph = graphResult.Graph;
            Graph.Stop();

            var outNodeResult = await Graph.CreateDeviceOutputNodeAsync();
            if (outNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                DisposeGraph();
                throw new Exception($"Unable to create device node: {outNodeResult.Status.ToString()}");
            }
            OutputNode = outNodeResult.DeviceOutputNode;

            var nodeProperties = Graph.EncodingProperties;
            nodeProperties.ChannelCount = NumChannels;
            nodeProperties.SampleRate = SampleRate;

            InputNode = Graph.CreateFrameInputNode(nodeProperties);
            InputNode.QuantumStarted += InputNodeQuantumStartedHandler;
            InputNode.AddOutgoingConnection(OutputNode);

            GraphReconstructionInProgress = false;
        }

        private void DisposeGraph()
        {
            Stop();
            InputNode = null;
            OutputNode = null;
            Graph = null;
            SampleRate = NullSampleRate;
        }

        private void InputNodeQuantumStartedHandler(AudioFrameInputNode sender, FrameInputNodeQuantumStartedEventArgs args)
        {
            if (args.RequiredSamples < 1)
                return;

            AudioFrame frame = GenerateAudioData(args.RequiredSamples);
            sender.AddFrame(frame);
        }

        unsafe private AudioFrame GenerateAudioData(int requiredSamples)
        {
            // Buffer size is (number of samples) * (size of each sample)
            // We choose to generate single channel (mono) audio. For multi-channel, multiply by number of channels
            uint bufferSizeElements = (uint)requiredSamples * NumChannels;
            uint bufferSizeBytes = bufferSizeElements * sizeof(float);
            AudioFrame frame = new AudioFrame(bufferSizeBytes);

            using (AudioBuffer buffer = frame.LockBuffer(AudioBufferAccessMode.Write))
            using (IMemoryBufferReference reference = buffer.CreateReference())
            {
                byte* dataInBytes;
                uint capacityInBytes;
                float* dataInFloat;

                // Get the buffer from the AudioFrame
                ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacityInBytes);

                // Cast to float since the data we are generating is float
                dataInFloat = (float*)dataInBytes;

                lock (SamplesBuffer)
                {
                    var numElementsToCopy = Math.Min(bufferSizeElements, SamplesBuffer.Count);
                    for (var i = 0; i < numElementsToCopy; i++)
                    {
                        var converted = (float)SamplesBuffer.Dequeue() / short.MaxValue;
                        dataInFloat[i] = converted;
                    }
                    //Should we not have enough samples in buffer, set the remaing data in audio frame to zeros
                    for (var i = numElementsToCopy; i < bufferSizeElements; i++)
                    {
                        dataInFloat[i] = 0f;
                    }
                }
            }

            return frame;
        }
    }
}
