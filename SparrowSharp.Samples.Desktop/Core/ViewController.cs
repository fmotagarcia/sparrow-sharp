using OpenTK;
using OpenTK.Graphics.ES20;
using Sparrow.Display;
using System;
using OpenTK.Graphics;
using SparrowSharp.Core;
using Sparrow.Core;
using ActiproSoftware.Products;
using OpenTK.Input;
using Sparrow;

namespace SparrowSharp.Samples.Desktop.Core
{
	public class ViewController : GameWindow, IViewController
    {
        public int DrawableWidth { get; set; }

        public int DrawableHeight { get; set; }

        public Context SPContext { get; set; }

        public DisplayObject Root { get; set; }

        public Stage Stage { get; set; }
        //public Juggler Juggler { get; set; }
        public float ContentScaleFactor { get; set; }

        public RenderSupport RenderSupport { get; set; }

        private Type _rootClass;
        private float _contentScaleFactor = 1.0f;
        // hardcode for now
        private float _viewScaleFactor = 1.0f;

        public ViewController() : base()
        {
			Load += HandleLoad;

			Resize += HandleResize;

			UpdateFrame += HandleUpdateFrame;

			RenderFrame += HandleRenderFrame;
        }

        void HandleRenderFrame (object sender, FrameEventArgs e)
        {
			// render graphics
			RenderSupport.NextFrame();
			Stage.Render(RenderSupport);
			RenderSupport.FinishQuadBatch();

			#if DEBUG
			RenderSupport.CheckForOpenGLError();
			#endif

			Stage.AdvanceTime((float)e.Time);

			//Console.WriteLine ("Number of draw calls: " + RenderSupport.NumDrawCalls);

			SwapBuffers();	
        }

		private void HandleUpdateFrame (object sender, FrameEventArgs e)
        {
			// add game logic, input handling
			if (Keyboard[Key.Escape])
			{
				Exit();
			}
        }

		private void HandleResize (object sender, EventArgs e)
        {
			GL.Viewport(0, 0, Width, Height);
			ReadjustStageSize();
        }

		private void HandleLoad (object sender, EventArgs e)
        {
			// setup settings, load textures, sounds
			VSync = VSyncMode.On;
			GL.Disable(EnableCap.CullFace);
			GL.Disable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);

			FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			//ContextRenderingApi = GLVersion.ES2;
			//MakeCurrent();
        }

        public void Start(Type RootClass)
        {
            if (_rootClass != null)
            {
                throw new Exception("Sparrow has already been started");
            }
            _rootClass = RootClass;

			Stage = new Stage();
			ReadjustStageSize(); 
			//Juggler = new Juggler();
			SPContext = new Context(null/*TODO get graphicsContext*/);
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


			// Run the game at 60 updates per second
			Run(60.0);
        }

        private void ReadjustStageSize()
        {
            // TODO check if Width/Height are not 0 here
            Stage.Width = Size.Width * _viewScaleFactor / _contentScaleFactor;
            Stage.Height = Size.Height * _viewScaleFactor / _contentScaleFactor;
        }

    }
}

