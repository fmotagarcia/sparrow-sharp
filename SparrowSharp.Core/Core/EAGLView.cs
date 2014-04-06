using System;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.iPhoneOS;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using Sparrow;
using System.Drawing;
using Sparrow.Touches;
using System.Collections.Generic;

namespace SparrowSharp.Core.iOS
{
    [Register("EAGLView")]
    public class EAGLView : iPhoneOSGameView
    {
        double _lastTouchTimestamp;

        TouchProcessor _touchProcessor = new TouchProcessor();

        [Export("initWithCoder:")]
        public EAGLView(NSCoder coder) : base(coder)
        {
            LayerRetainsBacking = true;
            LayerColorFormat = EAGLColorFormat.RGBA8;
        }

        [Export("layerClass")]
        public static new Class GetLayerClass()
        {
            return iPhoneOSGameView.GetLayerClass();
        }

        protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
        {
            eaglLayer.Opaque = true;
        }

        protected override void CreateFrameBuffer()
        {
            try
            {
                ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
                base.CreateFrameBuffer();
            }
            catch (Exception)
            {
                Console.WriteLine("Minimum supported OpenGL ES version is 2.0");
            }

            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Dither);
            GL.Enable(EnableCap.Blend);
        }

        #region DisplayLink support

        int frameInterval;
        CADisplayLink displayLink;

        public bool IsAnimating { get; private set; }
        // How many display frames must pass between each time the display link fires.
        public int FrameInterval
        {
            get
            {
                return frameInterval;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentException();
                frameInterval = value;
                if (IsAnimating)
                {
                    StopAnimating();
                    StartAnimating();
                }
            }
        }

        public void StartAnimating()
        {
            if (IsAnimating)
                return;

            CreateFrameBuffer();
            displayLink = UIScreen.MainScreen.CreateDisplayLink(this, new Selector("drawFrame"));
            displayLink.FrameInterval = frameInterval;
            displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);

            IsAnimating = true;


        }

        public void StopAnimating()
        {
            if (!IsAnimating)
                return;

            displayLink.Invalidate();
            displayLink = null;
            DestroyFrameBuffer();
            IsAnimating = false;
        }

        [Export("drawFrame")]
        void DrawFrame()
        {
            OnRenderFrame(new FrameEventArgs());
        }

        #endregion

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            MakeCurrent();
			
            SparrowSharpApp.Step(e.Time);

            SwapBuffers();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            processTouches(evt);
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            processTouches(evt);
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            processTouches(evt);
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            _lastTouchTimestamp -= 0.0001f; // cancelled touch events have an old timestamp -> workaround

            processTouches(evt);
        }

        void processTouches(UIEvent evt)
        {
            if (_lastTouchTimestamp != evt.Timestamp)
            {
                SizeF size = Bounds.Size;

                float xConversion = SparrowSharpApp.Stage.Width / size.Width;
                float yConversion = SparrowSharpApp.Stage.Height / size.Height;

                // convert to Touches and forward to stage
                List<Touch> touches = new List<Touch>();

                double now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                foreach (UITouch uiTouch in evt.TouchesForView(this))
                {
                    PointF location = uiTouch.LocationInView(this);
                    PointF previousLocation = uiTouch.PreviousLocationInView(this);

                    Touch touch = new Touch();
                    touch.TouchID = uiTouch.Handle.ToInt32();
                    touch.TimeStamp = now;
                    touch.GlobalX = location.X * xConversion;
                    touch.GlobalY = location.Y * yConversion;
                    touch.PreviousGlobalX = previousLocation.X * xConversion;
                    touch.PreviousGlobalY = previousLocation.Y * yConversion;
                    if (uiTouch.Phase == UITouchPhase.Began)
                    {
                        touch.InitialGlobalX = touch.GlobalX;
                        touch.InitialGlobalY = touch.GlobalY;
                    }
                    touch.IsTap = (uiTouch.TapCount != 0);
                    touch.Phase = (TouchPhase) uiTouch.Phase;

                    touches.Add(touch);
                }

                _touchProcessor.ProcessTouches(touches);

                _lastTouchTimestamp = evt.Timestamp;
            }
        }
    }
}