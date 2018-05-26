using LibRetriX;
using Microsoft.Graphics.Canvas.UI.Xaml;
using RetriX.Shared.Services;
using RetriX.UWP.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;

namespace RetriX.UWP
{
    public sealed class VideoService : IVideoService
    {
        public event EventHandler RequestRunCoreFrame;

        private CanvasAnimatedControl renderPanel;
        public CanvasAnimatedControl RenderPanel
        {
            get => renderPanel;
            set
            {
                if (renderPanel == value)
                {
                    return;
                }

                RenderTargetManager.Dispose();

                if (renderPanel != null)
                {
                    renderPanel.Update -= RenderPanelUpdate;
                    renderPanel.Draw -= RenderPanelDraw;
                    renderPanel.GameLoopStopped -= RenderPanelLoopStopping;
                }

                renderPanel = value;
                if (renderPanel != null)
                {
                    RenderPanel.ClearColor = Color.FromArgb(0xff, 0, 0, 0);
                    renderPanel.Update += RenderPanelUpdate;
                    renderPanel.Draw += RenderPanelDraw;
                    renderPanel.GameLoopStopped += RenderPanelLoopStopping;
                }
            }
        }

        private readonly RenderTargetManager RenderTargetManager = new RenderTargetManager();

        private TaskCompletionSource<object> InitTCS;

        public Task InitAsync()
        {
            if (InitTCS == null)
            {
                InitTCS = new TaskCompletionSource<object>();
            }
            
            return InitTCS.Task;
        }

        public Task DeinitAsync()
        {
            RenderPanel = null;
            return Task.CompletedTask;
        }

        public void RenderVideoFrameRGB0555(IReadOnlyList<ushort> data, uint width, uint height, ulong pitch)
        {
            if (RenderPanel == null)
            {
                return;
            }

            RenderTargetManager.UpdateFromCoreOutputRGB0555(RenderPanel.Device, data, width, height, pitch);
        }

        public void RenderVideoFrameRGB565(IReadOnlyList<ushort> data, uint width, uint height, ulong pitch)
        {
            if (RenderPanel == null)
            {
                return;
            }

            RenderTargetManager.UpdateFromCoreOutputRGB565(RenderPanel.Device, data, width, height, pitch);
        }

        public void RenderVideoFrameXRGB8888(IReadOnlyList<uint> data, uint width, uint height, ulong pitch)
        {
            if (RenderPanel == null)
            {
                return;
            }

            RenderTargetManager.UpdateFromCoreOutputXRGB8888(RenderPanel.Device, data, width, height, pitch);
        }

        public void GeometryChanged(GameGeometry geometry)
        {
            if (RenderPanel == null)
            {
                return;
            }

            RenderTargetManager.UpdateRenderTargetSize(RenderPanel.Device, geometry);
        }

        public void PixelFormatChanged(PixelFormats format)
        {
            RenderTargetManager.CurrentCorePixelFormat = format;
        }

        public void TimingsChanged(SystemTimings timings)
        {
            if (RenderPanel == null)
            {
                return;
            }

            var targetTimeTicks = (long)(TimeSpan.TicksPerSecond / timings.FPS);
            RenderPanel.TargetElapsedTime = TimeSpan.FromTicks(targetTimeTicks);
        }

        public void RotationChanged(Rotations rotation)
        {
            RenderTargetManager.CurrentRotation = rotation;
        }

        public void SetFilter(TextureFilterTypes filterType)
        {
            RenderTargetManager.RenderTargetFilterType = filterType;
        }

        private void RenderPanelLoopStopping(ICanvasAnimatedControl sender, object args)
        {
            RenderPanel = null;
        }

        private void RenderPanelUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            if (InitTCS != null)
            {
                InitTCS.SetResult(null);
                InitTCS = null;
            }

            RequestRunCoreFrame?.Invoke(this, EventArgs.Empty);
        }

        private void RenderPanelDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            RenderTargetManager.Render(args.DrawingSession, sender.Size);
        }
    }
}
