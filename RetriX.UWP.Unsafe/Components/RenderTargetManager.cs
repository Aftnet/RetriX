using LibRetriX;
using Microsoft.Graphics.Canvas;
using Retrix.UWP.Native;
using RetriX.Shared.Services;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.DirectX.Direct3D11;

namespace RetriX.UWP.Components
{
    internal class RenderTargetManager : IDisposable
    {
        private const uint RenderTargetMinSize = 1024;

        private static readonly IReadOnlyDictionary<PixelFormats, int> PixelFormatsSizeMapping = new Dictionary<PixelFormats, int>
        {
            { PixelFormats.XRGB8888, 4 },
            { PixelFormats.RGB565, 2 },
            { PixelFormats.RGB0555, 2 },
            { PixelFormats.Unknown, 0 },
        };

        private object RenderTargetLock { get; } = new object();
        private CanvasBitmap RenderTarget { get; set; } = null;
        public TextureFilterTypes RenderTargetFilterType { get; set; } = TextureFilterTypes.Bilinear;
        private IDirect3DSurface RenderTargetSurface { get; set; } = null;
        private Rect RenderTargetViewport = new Rect();
        //This may be different from viewport's width/haight.
        private float RenderTargetAspectRatio { get; set; } = 1.0f;

        private PixelFormats currentCorePixelFormat = PixelFormats.Unknown;
        public PixelFormats CurrentCorePixelFormat
        {
            get { return currentCorePixelFormat; }
            set { currentCorePixelFormat = value; CurrentCorePixelSize = PixelFormatsSizeMapping[currentCorePixelFormat]; }
        }

        public Rotations CurrentRotation { get; set; }

        private int CurrentCorePixelSize = 0;

        public void Dispose()
        {
            RenderTarget?.Dispose();
            RenderTarget = null;

            RenderTargetSurface?.Dispose();
            RenderTargetSurface = null;
        }

        public void Render(CanvasDrawingSession drawingSession, Size canvasSize)
        {
            var viewportWidth = RenderTargetViewport.Width;
            var viewportHeight = RenderTargetViewport.Height;
            var aspectRatio = RenderTargetAspectRatio;
            if (RenderTarget == null || viewportWidth <= 0 || viewportHeight <= 0)
                return;

            var rotAngle = 0.0;
            switch (CurrentRotation)
            {
                case Rotations.CCW90:
                    rotAngle = -0.5 * Math.PI;
                    aspectRatio = 1.0f / aspectRatio;
                    break;
                case Rotations.CCW180:
                    rotAngle = -Math.PI;
                    break;
                case Rotations.CCW270:
                    rotAngle = -1.5 * Math.PI;
                    aspectRatio = 1.0f / aspectRatio;
                    break;
            }

            var destinationSize = ComputeBestFittingSize(canvasSize, aspectRatio);
            var scaleMatrix = Matrix3x2.CreateScale((float)destinationSize.Width, (float)destinationSize.Height);
            var rotMatrix = Matrix3x2.CreateRotation((float)rotAngle);
            var transMatrix = Matrix3x2.CreateTranslation((float)(0.5 * canvasSize.Width), (float)(0.5f * canvasSize.Height));
            var transformMatrix = rotMatrix * scaleMatrix * transMatrix;

            lock (RenderTargetLock)
            {
                drawingSession.Transform = transformMatrix;
                var interpolation = RenderTargetFilterType == TextureFilterTypes.NearestNeighbor ? CanvasImageInterpolation.NearestNeighbor : CanvasImageInterpolation.Linear;
                drawingSession.DrawImage(RenderTarget, new Rect(-0.5, -0.5, 1.0, 1.0), RenderTargetViewport, 1.0f, interpolation);
                drawingSession.Transform = Matrix3x2.Identity;
            }
        }

        public unsafe void UpdateFromCoreOutputRGB0555(CanvasDevice device, IReadOnlyList<ushort> data, uint width, uint height, ulong pitch)
        {
            if (data == null || RenderTarget == null || CurrentCorePixelSize == 0)
                return;

            lock (RenderTargetLock)
            {
                RenderTargetViewport.Width = width;
                RenderTargetViewport.Height = height;

                using (var renderTargetMap = new D3DSurfaceMap(device, RenderTargetSurface))
                {
                    var dataPtr = (byte*)new IntPtr(renderTargetMap.Data).ToPointer();
                    FramebufferConverter.ConvertFrameBufferRGB0555ToXRGB8888(width, height, data, (int)pitch, dataPtr, (int)renderTargetMap.PitchBytes);
                }
            }
        }

        public unsafe void UpdateFromCoreOutputRGB565(CanvasDevice device, IReadOnlyList<ushort> data, uint width, uint height, ulong pitch)
        {
            if (data == null || RenderTarget == null || CurrentCorePixelSize == 0)
                return;

            lock (RenderTargetLock)
            {
                RenderTargetViewport.Width = width;
                RenderTargetViewport.Height = height;

                using (var renderTargetMap = new D3DSurfaceMap(device, RenderTargetSurface))
                {
                    var dataPtr = (byte*)new IntPtr(renderTargetMap.Data).ToPointer();
                    FramebufferConverter.ConvertFrameBufferRGB565ToXRGB8888(width, height, data, (int)pitch, dataPtr, (int)renderTargetMap.PitchBytes);
                }
            }
        }

        public unsafe void UpdateFromCoreOutputXRGB8888(CanvasDevice device, IReadOnlyList<uint> data, uint width, uint height, ulong pitch)
        {
            if (data == null || RenderTarget == null || CurrentCorePixelSize == 0)
                return;

            lock (RenderTargetLock)
            {
                RenderTargetViewport.Width = width;
                RenderTargetViewport.Height = height;

                using (var renderTargetMap = new D3DSurfaceMap(device, RenderTargetSurface))
                {
                    var dataPtr = (byte*)new IntPtr(renderTargetMap.Data).ToPointer();
                    FramebufferConverter.ConvertFrameBufferXRGB8888(width, height, data, (int)pitch, dataPtr, (int)renderTargetMap.PitchBytes);
                }
            }
        }

        public void UpdateRenderTargetSize(CanvasDevice device, GameGeometry geometry)
        {
            RenderTargetAspectRatio = geometry.AspectRatio;
            if (RenderTargetAspectRatio < 0.1f)
            {
                RenderTargetAspectRatio = (float)(geometry.BaseWidth) / geometry.BaseHeight;
            }

            if (RenderTarget != null)
            {
                var currentSize = RenderTarget.Size;
                if (currentSize.Width >= geometry.MaxWidth && currentSize.Height >= geometry.MaxHeight)
                {
                    return;
                }
            }

            lock (RenderTargetLock)
            {
                var size = Math.Max(Math.Max(geometry.MaxWidth, geometry.MaxHeight), RenderTargetMinSize);
                size = ClosestGreaterPowerTwo(size);

                RenderTarget?.Dispose();
                RenderTargetSurface?.Dispose();
                RenderTargetSurface = D3DSurfaceMap.CreateMappableD3DSurface(device, size, size);
                RenderTarget = CanvasBitmap.CreateFromDirect3D11Surface(device, RenderTargetSurface);
            }
        }

        private static Size ComputeBestFittingSize(Size viewportSize, float aspectRatio)
        {
            var candidateWidth = Math.Floor(viewportSize.Height * aspectRatio);
            var size = new Size(candidateWidth, viewportSize.Height);
            if (viewportSize.Width < candidateWidth)
            {
                var height = viewportSize.Width / aspectRatio;
                size = new Size(viewportSize.Width, height);
            }

            return size;
        }

        private static uint ClosestGreaterPowerTwo(uint value)
        {
            uint output = 1;
            while (output < value)
            {
                output *= 2;
            }

            return output;
        }
    }
}
