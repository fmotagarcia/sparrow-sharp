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

namespace SparrowSharp.Core.iOS
{
    [Register("EAGLView")]
    public class EAGLView : iPhoneOSGameView
    {
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
    }
}
