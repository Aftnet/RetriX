using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RetriX.UWP.Components
{
    internal unsafe static class FramebufferConverter
    {
        private const uint LookupTableSize = ushort.MaxValue + 1;

        private static readonly uint[] RGB0555LookupTable = new uint[LookupTableSize];
        private static readonly GCHandle RGB0555LookupTableHandle;
        private static readonly uint* RGB0555LookupTablePtr;

        private static readonly uint[] RGB565LookupTable = new uint[LookupTableSize];
        private static readonly GCHandle RGB565LookupTableHandle;
        private static readonly uint* RGB565LookupTablePtr;

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

            RGB0555LookupTableHandle = GCHandle.Alloc(RGB0555LookupTable, GCHandleType.Pinned);
            RGB0555LookupTablePtr = (uint*)RGB0555LookupTableHandle.AddrOfPinnedObject();

            RGB565LookupTableHandle = GCHandle.Alloc(RGB565LookupTable, GCHandleType.Pinned);
            RGB565LookupTablePtr = (uint*)RGB565LookupTableHandle.AddrOfPinnedObject();
        }

        public static void ConvertFrameBufferRGB0555ToXRGB8888(uint width, uint height, IReadOnlyList<ushort> input, int inputPitch, byte* output, int outputPitch)
        {
            ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, RGB0555LookupTablePtr);
        }

        public static void ConvertFrameBufferRGB565ToXRGB8888(uint width, uint height, IReadOnlyList<ushort> input, int inputPitch, byte* output, int outputPitch)
        {
            ConvertFrameBufferUshortToXRGB8888WithLUT(width, height, input, inputPitch, output, outputPitch, RGB565LookupTablePtr);
        }

        public static void ConvertFrameBufferXRGB8888(uint width, uint height, IReadOnlyList<uint> input, int inputPitch, byte* output, int outputPitch)
        {
            var inLineStartIx = 0;
            for (var i = 0; i < height; i++)
            {
                var outLineStart = (uint*)output;
                for (var j = 0; j < width; j++)
                {
                    outLineStart[j] = input[inLineStartIx + j];
                }

                inLineStartIx += inputPitch;
                output += outputPitch;
            }
        }

        private static void ConvertFrameBufferUshortToXRGB8888WithLUT(uint width, uint height, IReadOnlyList<ushort> input, int inputPitch, byte* output, int outputPitch, uint* lutPtr)
        {
            var inLineStartIx = 0;
            for (var i = 0; i < height; i++)
            {
                var outLineStart = (uint*)output;
                for (var j = 0; j < width; j++)
                {
                    outLineStart[j] = lutPtr[input[inLineStartIx + j]];
                }

                inLineStartIx += inputPitch;
                output += outputPitch;
            }
        }
    }
}
