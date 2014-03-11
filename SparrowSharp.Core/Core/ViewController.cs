using OpenTK;
using OpenTK.Graphics.ES20;
using Sparrow.Display;
using System;
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using Android.Content.Res;
using Android.Util;
using SparrowSharp.Core;
using Android.OS;

namespace Sparrow.Core
{
    public class ViewController : AndroidGameView, IViewController
    {

        private bool _contextWasLost = false;
		private long _elapsedTime = 0;
		public static Android.Content.Context ContextRef;

        public ViewController(Android.Content.Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Setup();
        }

        public ViewController(IntPtr handle, Android.Runtime.JniHandleOwnership transfer) : base(handle, transfer)
        {
            Setup();
        }

        public ViewController(Android.Content.Context context) : base(context)
        {
            Setup();
        }

        public void Setup()
		{
			ContextRef = Context;
            RequestFocus();
            FocusableInTouchMode = true;
        }
        // This method is called everytime the context needs to be recreated.
        protected override void CreateFrameBuffer()
        {
            Log.Verbose("Sparrow", "AndroidGameWindow.CreateFrameBuffer");
            try
            {
                ContextRenderingApi = GLVersion.ES2;
                try
                {
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 0, 0, 0, false);
                    base.CreateFrameBuffer();
                }
                catch (Exception)
                {
                    Log.Verbose("Sparrow", "Failed to create desired format, falling back to defaults");
                    // try again using a more basic mode with a 16 bit depth buffer which hopefully the device will support 
                    GraphicsMode = new AndroidGraphicsMode(new ColorFormat(0, 0, 0, 0), 16, 0, 0, 0, false);
                    try
                    {
                        base.CreateFrameBuffer();
                    }
                    catch (Exception)
                    {
                        // ok we are right back to getting the default
                        GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);
                        base.CreateFrameBuffer();
                    }
                }
                Log.Verbose("Sparrow", "Created format {0}", this.GraphicsContext.GraphicsMode);
                All status = GL.CheckFramebufferStatus(All.Framebuffer);
                Log.Verbose("Sparrow", "Framebuffer Status: " + status.ToString());
            }
            catch (Exception)
            {
                throw new NotSupportedException("Could not create OpenGLES 2.0 frame buffer");
            }
            GL.Disable(All.CullFace);
            GL.Disable(All.DepthTest);
            GL.Enable(All.Blend);

            if (_contextWasLost)
            {
				// todo reload context, ReadjustStageSize, ...
            }

            MakeCurrent();
        }
        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MakeCurrent();

			SP.InitApp (Size.Width, Size.Height);

            // Run the render loop
            Run();
        }
        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

			long elapsd = SystemClock.ElapsedRealtime () - _elapsedTime;
			Console.WriteLine ("elapsed: " + elapsd );
			SP.Step( elapsd );
			_elapsedTime = SystemClock.ElapsedRealtime();

            SwapBuffers();
        }

        protected override void DestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();

            _contextWasLost = GraphicsContext == null || GraphicsContext.IsDisposed;
        }
    }
}

