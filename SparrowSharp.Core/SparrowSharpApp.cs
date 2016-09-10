using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sparrow.Core;
using Sparrow.Display;
using SparrowSharp.Utils;
using SparrowSharp.Core.Desktop.Rendering;
using SparrowSharp.Core.Rendering;
using Sparrow.Geom;

namespace Sparrow
{
    public static class SparrowSharpApp
    {

        private static Painter _painter;
        private static uint _frameID;
        private static readonly float ContentScaleFactor = 1.0f;

        private static Rectangle _viewPort;
        private static Rectangle _previousViewPort;
        private static Rectangle _clippedViewPort;

        public static Stage Stage { get; private set; }

        public static uint FrameID { get { return _frameID; } }

        public static readonly Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static object NativeWindow;

        public static DisplayObject Root { get; private set; }

        public static Juggler DefaultJuggler { get; private set; }

        private static readonly Stopwatch watch = new Stopwatch();

        private static StatsDisplay stats;

        private static uint _width;
        private static uint _height;

        public static void Start(uint width, uint height, Type rootType)
        {
            _width = width;
            _height = height;
            _viewPort = new Rectangle(0, 0, _width, _height);
            _previousViewPort = new Rectangle();
            GPUInfo.PrintGPUInfo();
#if DEBUG
            OpenGLDebugCallback.Init();
#endif
            if (rootType == null)
            {
                throw new InvalidOperationException("Root cannot be null!");
            }

            if (Root != null)
            {
                throw new InvalidOperationException("App already initialized!");
            }
            GPUInfo.HasOpenGLError = false;

            _painter = new Painter(width, height);
            Stage = new Stage(width, height);
            DefaultJuggler = new Juggler();

            UpdateViewPort(true);

            Root = (DisplayObject)Activator.CreateInstance(rootType);
            Stage.AddChild(Root);
        }

        public static void Step(double time)
        {
            long elapsed = watch.ElapsedMilliseconds;
            watch.Restart();

            Stage.AdvanceTime(elapsed);
            DefaultJuggler.AdvanceTime(elapsed / 1000.0f);
            Render();
        }

        public static void Render()
        {
            UpdateViewPort();

            bool doRedraw = Stage.RequiresRedraw;
            if (doRedraw)
            {
                //dispatchEventWith(starling.events.Event.RENDER);
                
                float scaleX = _viewPort.Width / Stage.Width;
                float scaleY = _viewPort.Height / Stage.Height;

                _painter.NextFrame();
                _painter.PixelSize = 1.0f / ContentScaleFactor;
                _painter.State.SetProjectionMatrix(
                    _viewPort.X < 0 ? -_viewPort.X / scaleX : 0.0f,
                    _viewPort.Y < 0 ? -_viewPort.Y / scaleY : 0.0f,
                    _clippedViewPort.Width / scaleX,
                    _clippedViewPort.Height / scaleY,
                    Stage.Width, Stage.Height, Stage.CameraPosition);
                
                _painter.Clear(Stage.Color, 1.0f);

                Stage.Render(_painter);
                _painter.FinishFrame();
                _painter.FrameID = ++_frameID;
                
                _painter.Present();
            }
            /*
            if (stats != null)
            {
                stats.DrawCount = _painter.DrawCount;
                if (!doRedraw) stats.MarkFrameAsSkipped();
            }
            */
            // old code:
            // renderSupport.NextFrame();
            //Stage.Render(_painter);
            //renderSupport.FinishQuadBatch();

#if DEBUG
            GPUInfo.CheckForOpenGLError();
#endif
        }

        private static void UpdateViewPort(bool forceUpdate = false)
        {
            // the last set viewport is stored in a variable; that way, people can modify the
            // viewPort directly (without a copy) and we still know if it has changed.
            if (forceUpdate || !Rectangle.Compare(_viewPort, _previousViewPort))
            {
                _clippedViewPort = new Rectangle(0, 0, _width, _height);
                _previousViewPort.SetTo(_viewPort.X, _viewPort.Y, _viewPort.Width, _viewPort.Height);

                _painter.ConfigureBackBuffer(_clippedViewPort);
            }
        }

        /** The painter, which is used for all rendering. The same instance is passed to all
         *  <code>render</code>methods each frame. */
        public static Painter Painter {get { return _painter; } }

        /** Makes sure that the next frame is actually rendered.
         *
         *  <p>When <code>skipUnchangedFrames</code> is enabled, some situations require that you
         *  manually force a redraw, e.g. when a RenderTexture is changed. This method is the
         *  easiest way to do so; it's just a shortcut to <code>stage.setRequiresRedraw()</code>.
         *  </p>
         */
        public static void SetRequiresRedraw()
        {
            Stage.SetRequiresRedraw();
        }
    
        public static bool ShowStats
        {
            set {
                if (stats == null) {
                    stats = new StatsDisplay();
                    Stage.AddChild(stats);
                }
                stats.Visible = value;
            }
        }

        public static void Destroy()
        {
            stats = null;
            Stage.RemoveAllChildren();
            Stage = null;
            Root = null;
            Programs.Clear();
        }
    }
}