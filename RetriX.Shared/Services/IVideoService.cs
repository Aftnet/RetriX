using LibRetriX;
using System;

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
        void RenderVideoFrame(ReadOnlySpan<byte> data, uint width, uint height, ulong pitch);
        void SetFilter(TextureFilterTypes filterType);
    }
}
