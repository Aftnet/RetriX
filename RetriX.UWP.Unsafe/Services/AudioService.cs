using LibRetriX;
using RetriX.Shared.Services;
using RetriX.UWP.Components;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Audio;

namespace RetriX.UWP.Services
{
    public sealed class AudioService : AudioServiceBase
    {
        private bool GraphReconstructionInProgress = false;
        protected override bool AllowPlaybackControl => !GraphReconstructionInProgress && InputNode != null;

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

        public override Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public override Task DeinitAsync()
        {
            Stop();
            DisposeGraph();
            return Task.CompletedTask;
        }

        public override void TimingChanged(SystemTimings timings)
        {
            uint sampleRate = (uint)timings.SampleRate;
            if (SampleRate == sampleRate || GraphReconstructionInProgress)
                return;

            SampleRate = sampleRate;

            Stop();
            var operation = ReconstructGraph();
        }

        public override void Stop()
        {
            if (!AllowPlaybackControl)
            {
                return;
            }

            Graph.Stop();
            base.Stop();
        }

        protected override void StartPlayback()
        {
            Graph.Start();
        }

        private async Task ReconstructGraph()
        {
            GraphReconstructionInProgress = true;

            if (SampleRate == NullSampleRate) //If invalid sample rate do not create graph but return to allow trying again
            {
                GraphReconstructionInProgress = false;
                return;
            }

            DisposeGraph();

            var graphResult = await AudioGraph.CreateAsync(new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.GameMedia)).AsTask().ConfigureAwait(false);
            if (graphResult.Status != AudioGraphCreationStatus.Success)
            {
                DisposeGraph();
                throw new Exception($"Unable to create audio graph: {graphResult.Status.ToString()}");
            }
            Graph = graphResult.Graph;
            Graph.Stop();

            var outNodeResult = await Graph.CreateDeviceOutputNodeAsync().AsTask().ConfigureAwait(false);
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
