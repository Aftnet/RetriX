using LibRetriX;
using System;
using System.Collections.Generic;

namespace RetriX.Shared.Services
{
    public enum TextureFilterTypes { NearestNeighbor, Bilinear };

    public interface IVideoService : IInitializable
    {
        event EventHandler RequestRunCoreFrame;

        void GeometryChanged(GameGeometry geometry);
        void PixelFormatChanged(PixelFormats format);
        void RotationChanged(Rotations rotation);
        void TimingsChanged(SystemTimings timings);
        void RenderVideoFrameRGB0555(IReadOnlyList<ushort> data, uint width, uint height, ulong pitch);
        void RenderVideoFrameRGB565(IReadOnlyList<ushort> data, uint width, uint height, ulong pitch);
        void RenderVideoFrameXRGB8888(IReadOnlyList<uint> data, uint width, uint height, ulong pitch);
        void SetFilter(TextureFilterTypes filterType);
    }
}
