using CoreGraphics;
using LibRetriX;
using MetalKit;
using RetriX.Shared.Services;
using System;
using System.Threading.Tasks;

namespace RetriX.Apple.Services
{
    public class VideoService : IVideoService, IMTKViewDelegate
    {
        private MTKView renderView;
        public MTKView RenderView
        {
            get => renderView;
            set
            {
                if (renderView == value)
                {
                    return;
                }

                renderView.Delegate = null;
                renderView = value;
                renderView.Delegate = this;
            }
        }

        public IntPtr Handle => throw new NotImplementedException();

        public event EventHandler RequestRunCoreFrame;

        public Task InitAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeinitAsync()
        {
            throw new NotImplementedException();
        }

        public void GeometryChanged(GameGeometry geometry)
        {
            throw new NotImplementedException();
        }

        public void PixelFormatChanged(PixelFormats format)
        {
            throw new NotImplementedException();
        }

        public void TimingsChanged(SystemTimings timings)
        {
            if (RenderView != null)
            {
                RenderView.PreferredFramesPerSecond = (nint)timings.FPS;
            }
        }

        public void RotationChanged(Rotations rotation)
        {
            throw new NotImplementedException();
        }

        public void RenderVideoFrame(ReadOnlySpan<byte> data, uint width, uint height, uint pitch)
        {
            throw new NotImplementedException();
        }

        public void SetFilter(TextureFilterTypes filterType)
        {
            throw new NotImplementedException();
        }

        public void DrawableSizeWillChange(MTKView view, CGSize size)
        {
            throw new NotImplementedException();
        }

        public void Draw(MTKView view)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}