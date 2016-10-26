using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sparrow.Display;
using Sparrow.Utils;
using Sparrow.Rendering;
using Sparrow.Geom;
using Sparrow.Animation;
using OpenGL;

namespace Sparrow.Core
{
    public static class SparrowSharp
    {

        public delegate void ContextCreatedHandler();
        public static event ContextCreatedHandler ContextCreated;

        private static Painter _painter;
        private static uint _frameID;

        private static Rectangle _viewPort;
        private static Rectangle _previousViewPort;
        private static Rectangle _clippedViewPort;
        private static bool _enableErrorChecking;

        public static Stage Stage { get; private set; }

        public static uint FrameID { get { return _frameID; } }

        public static readonly Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static object NativeWindow;

        public static DisplayObject Root { get; private set; }

        public static Juggler DefaultJuggler { get; private set; }

        /** When enabled, Starling will skip rendering the stage if it hasn't changed since the
        *  last frame. This is great for apps that remain static from time to time, since it will
        *  greatly reduce power consumption. You should activate this whenever possible!
        *
        *  <p>The reason why it's disabled by default is just that it causes problems with Render-
        *  and VideoTextures. When you use those, you either have to disable this property
        *  temporarily, or call <code>setRequiresRedraw()</code> (ideally on the stage) whenever
        *  those textures are changing. Otherwise, the changes won't show up.</p>
        *
        *  @default false
        */
        public static bool SkipUnchangedFrames = false;

        private static readonly Stopwatch watch = new Stopwatch();

        private static StatsDisplay _statsDisplay;

        private static uint _width;
        private static uint _height;

        public static void Start(uint width, uint height, Type rootType)
        {
            Gl.Disable(EnableCap.CullFace);
            Gl.Disable(EnableCap.Dither);

            Gl.Enable(EnableCap.Blend);
            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Always);
            
            int status = Gl.CheckFramebufferStatus(Gl.FRAMEBUFFER);
            if (status != Gl.FRAMEBUFFER_COMPLETE)
            {
                Console.WriteLine("Framebuffer error: " + status);
            }
            
            _width = width;
            _height = height;
            _viewPort = Rectangle.Create(0, 0, _width, _height);
            _previousViewPort = Rectangle.Create();
            GPUInfo.PrintGPUInfo();

            if (rootType == null)
            {
                throw new InvalidOperationException("Root cannot be null!");
            }

            if (Root != null)
            {
                throw new InvalidOperationException("App already initialized!");
            }

            _painter = new Painter(width, height);
            Stage = new Stage(width, height);
            DefaultJuggler = new Juggler();

            UpdateViewPort(true);

            Root = (DisplayObject)Activator.CreateInstance(rootType);
            Stage.AddChild(Root);
            _frameID = 1; // starts with 1, so things on the first frame are cached
        }

        public static bool Step()
        {
            long elapsed = watch.ElapsedMilliseconds;
            watch.Restart();

            Stage.AdvanceTime(elapsed);
            DefaultJuggler.AdvanceTime(elapsed / 1000.0f);
            bool doRedraw = Stage.RequiresRedraw || !SkipUnchangedFrames || _frameID == 1;
            Render(doRedraw);
            return doRedraw;
        }

        public static void Render(bool doRedraw)
        {
            UpdateViewPort();
            
            if (doRedraw)
            {
                //dispatchEventWith(starling.events.Event.RENDER);
                float scaleX = _viewPort.Width / Stage.StageWidth;
                float scaleY = _viewPort.Height / Stage.StageHeight;

                _painter.NextFrame();
                _painter.PixelSize = 1.0f / ContentScaleFactor;
                _painter.State.SetProjectionMatrix(
                    _viewPort.X < 0f ? -_viewPort.X / scaleX : 0.0f,
                    _viewPort.Y < 0f ? -_viewPort.Y / scaleY : 0.0f,
                    _clippedViewPort.Width / scaleX,
                    _clippedViewPort.Height / scaleY,
                    Stage.StageWidth, Stage.StageHeight, Stage.CameraPosition);
                
                _painter.Clear(Stage.Color, 0.0f);

                Stage.Render(_painter);
                _painter.FinishFrame();
                _painter.FrameID = ++_frameID;
                
                _painter.Present();
            }
            
            if (_statsDisplay != null)
            {
                _statsDisplay.DrawCount = _painter.DrawCount;
                if (!doRedraw) _statsDisplay.MarkFrameAsSkipped();
            }

            if (_enableErrorChecking)
            {
                GPUInfo.CheckForOpenGLError();
            }
        }

        private static void UpdateViewPort(bool forceUpdate = false)
        {
            // the last set viewport is stored in a variable; that way, people can modify the
            // viewPort directly (without a copy) and we still know if it has changed.
            if (forceUpdate || !Rectangle.Compare(_viewPort, _previousViewPort))
            {
                _clippedViewPort = Rectangle.Create(0, 0, _width, _height);
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
        
        public static void HideStats()
        {
            if (_statsDisplay != null)
            {
                _statsDisplay.RemoveFromParent();
            }
        }

        public static void ShowStats(HAlign horizontalAlign = HAlign.Left, 
                                     VAlign verticalAlign = VAlign.Top, float scale = 1f)
        {
            float stageWidth  = Stage.StageWidth;
            float stageHeight = Stage.StageHeight;

            if (_statsDisplay == null)
            {
                _statsDisplay = new StatsDisplay();
                _statsDisplay.Touchable = false;
            }

            Stage.AddChild(_statsDisplay);
            _statsDisplay.ScaleX = _statsDisplay.ScaleY = scale;

            if (horizontalAlign == HAlign.Left) _statsDisplay.X = 0f;
            else if (horizontalAlign == HAlign.Right) _statsDisplay.X = stageWidth - _statsDisplay.Width;
            else if (horizontalAlign == HAlign.Center) _statsDisplay.X = (stageWidth - _statsDisplay.Width) / 2;

            if (verticalAlign == VAlign.Top) _statsDisplay.Y = 0f;
            else if (verticalAlign == VAlign.Bottom) _statsDisplay.Y = stageHeight - _statsDisplay.Height;
            else if (verticalAlign == VAlign.Center) _statsDisplay.Y = (stageHeight - _statsDisplay.Height) / 2;
        }

        /// <summary>
        /// The viewport into which Sparrow contents will be rendered. This is the size of the window/screen in pixels
        /// </summary>
        public static Rectangle ViewPort
        {
            get { return _viewPort; }
            set { _viewPort = value.Clone(); }
        }

        /** The ratio between viewPort width and stage width. Useful for choosing a different
         *  set of textures depending on the display resolution. */
        public static float ContentScaleFactor
        {
            get { return _viewPort.Width / Stage.StageWidth; }
        }

        /** Indicates if Stage3D render methods will report errors. Activate only when needed,
         *  as this has a negative impact on performance. @default false */
        public static void EnableErrorChecking()
        {
            if (_enableErrorChecking == false)
            {
                _enableErrorChecking = true;
                OpenGLDebugCallback.Init();
            }
        }

        public static void Destroy()
        {
            _statsDisplay = null;
            Stage.RemoveAllChildren();
            Stage = null;
            Root = null;
            Programs.Clear();
            // TODO check
        }

        public static void OnContextCreated()
        {
            UpdateViewPort(true);
            ContextCreated?.Invoke();
        }
    }
}