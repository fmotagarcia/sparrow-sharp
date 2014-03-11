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
        public int DrawableWidth { get; set; }

        public int DrawableHeight { get; set; }

        public Context SPContext { get; set; }

        public DisplayObject Root { get; set; }

        public Stage Stage { get; set; }
        //public Juggler Juggler { get; set; }
        public float ContentScaleFactor { get; set; }

        public RenderSupport RenderSupport { get; set; }

        private bool _contextWasLost = false;
        private Type _rootClass;
        private float _contentScaleFactor = 1.0f;
		private long _previousFrameStartTime;

        // hardcode for now
        private float _viewScaleFactor = 1.0f;
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
                // todo reload context
                ReadjustStageSize();
            }

            MakeCurrent();
        }
        // This gets called when the drawing surface is ready
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            MakeCurrent();

            if (Root == null)
            {
                Stage = new Stage();
                ReadjustStageSize(); 
                //Juggler = new Juggler();
                SPContext = new Context(GraphicsContext);
                SP.CurrentController = this;
                SP.Context = SPContext;
                RenderSupport = new RenderSupport();

                Root = (DisplayObject)Activator.CreateInstance(_rootClass);
                if (Root.GetType().IsInstanceOfType(Stage))
                {
                    throw new Exception("Root extends 'Stage' but is expected to extend 'Sprite' instead");
                }
                else
                {
                    Stage.AddChild(Root);
                }
            }

			_previousFrameStartTime = SystemClock.ElapsedRealtime();

            // Run the render loop
            Run();
        }
        // This gets called on each frame render
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

			long currentTime = SystemClock.ElapsedRealtime();
			long elapsedTime = currentTime - _previousFrameStartTime;
			_previousFrameStartTime = currentTime;

            RenderSupport.NextFrame();
            Stage.Render(RenderSupport);
            RenderSupport.FinishQuadBatch();

            #if DEBUG
            RenderSupport.CheckForOpenGLError();
            #endif

			Stage.AdvanceTime(elapsedTime);
			
            SwapBuffers();
        }

        public void Start(Type RootClass)
        {
            if (_rootClass != null)
            {
                throw new Exception("Sparrow has already been started");
            }
            _rootClass = RootClass;
        }

        private void ReadjustStageSize()
        {
            // TODO check if Width/Height are not 0 here
            Stage.Width = Size.Width * _viewScaleFactor / _contentScaleFactor;
            Stage.Height = Size.Height * _viewScaleFactor / _contentScaleFactor;
        }

        protected override void DestroyFrameBuffer()
        {
            base.DestroyFrameBuffer();

            _contextWasLost = GraphicsContext == null || GraphicsContext.IsDisposed;
        }
    }
}

