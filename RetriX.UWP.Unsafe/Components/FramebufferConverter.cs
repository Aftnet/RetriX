using System;
using System.Runtime.InteropServices;

namespace RetriX.UWP.Components
{
    internal static class FramebufferConverter
    {
        private const uint LookupTableSize = ushort.MaxValue + 1;

        private static readonly uint[] RGB0555LookupTable = new uint[LookupTableSize];
        private static readonly uint[] RGB565LookupTable = new uint[LookupTableSize];

        static FramebufferConverter()
        {
            uint r, g, b;
            for (uint i = 0; i < LookupTableSize; i++)
            {
                r = (i >> 11) & 0x1F;
                g = (i >> 5) & 0x3F;
                b = (i & 0x1F);

                r = (uint)Math.Round(r * 255.0 / 31.0);
                g = (uint)Math.Round(g * 255.0 / 63.0);
                b = (uint)Math.Round(b * 255.0 / 31.0);

                RGB565LookupTable[i] = 0xFF000000 | r << 16 | g << 8 | b;
            }

            for (uint i = 0; i < LookupTableSize; i++)
            {
                r = (i >> 10) & 0x1F;
                g = (i >> 5) & 0x1F;
                b = (i & 0x1F);

                r = (uint)Math.Round(r * 255.0 / 31.0);
                g = (uint)Math.Round(g * 255.0 / 31.0);
                b = (uint)Math.Round(b * 255.0 / 31.0);

                RGB0555LookupTable[i] = 0xFF000000 | r << 16 | g << 8 | b;
            }
        }

        public static void ConvertFrameBufferRGB0555ToXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
           ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, RGB0555LookupTable);
        }

        public static void ConvertFrameBufferRGB565ToXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, RGB565LookupTable);
        }

        public static void ConvertFrameBufferXRGB8888(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch)
        {
            var castInput = MemoryMarshal.Cast<byte, uint>(input);
            var castInputPitch = inputPitch / sizeof(uint);
            var castOutput = MemoryMarshal.Cast<byte, uint>(output);
            var castOutputPitch = outputPitch / sizeof(uint);

            for (var i = 0; i < height; i++)
            {
                var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);
                for (var j = 0; j < width; j++)
                {
                    outputLine[j] = inputLine[j];
                }
            }
        }

        private static void ConvertFrameBufferUshortToXRGB8888WithLUT(uint width, uint height, ReadOnlySpan<byte> input, int inputPitch, Span<byte> output, int outputPitch, ReadOnlySpan<uint> lutPtr)
        {
            var castInput = MemoryMarshal.Cast<byte, ushort>(input);
            var castInputPitch = inputPitch / sizeof(ushort);
            var castOutput = MemoryMarshal.Cast<byte, uint>(output);
            var castOutputPitch = outputPitch / sizeof(uint);

            for (var i = 0; i < height; i++)
            {
                var inputLine = castInput.Slice(i * castInputPitch, castInputPitch);
                var outputLine = castOutput.Slice(i * castOutputPitch, castOutputPitch);
                for (var j = 0; j < width; j++)
                {
                    outputLine[j] = lutPtr[inputLine[j]];
                }
            }
        }
    }
}
