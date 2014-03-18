using OpenTK;
using OpenTK.Graphics.ES20;
using System;
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using Android.Util;
using SparrowSharp.Core;
using Android.OS;
using Sparrow.Textures;
using Sparrow.ResourceLoading;

namespace Sparrow.Core
{
    public class AndroidViewController : AndroidGameView, IViewController
    {
        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        private OnLoadedAction _onLoadedAction;
        private bool _contextWasLost = false;
        public static Android.Content.Context AndroidContext;

        public AndroidViewController(Android.Content.Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Setup(context);
        }

        public AndroidViewController(Android.Content.Context context, OnLoadedAction onLoadedAction) : base(context)
        {
            this._onLoadedAction = onLoadedAction;
            Setup(context);
        }

        public void Setup(Android.Content.Context context)
        {
            AndroidContext = context;
            TextureLoader._context = context;
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
            GL.Disable(All.Alpha);
            GL.Disable(All.Dither);
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

            _onLoadedAction(Size.Width, Size.Height);

            // Run the render loop
            Run();
        }
        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            SparrowSharpApp.Step(e.Time);

            SwapBuffers();
        }

        protected override void DestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();

            _contextWasLost = GraphicsContext == null || GraphicsContext.IsDisposed;
        }
    }
}

