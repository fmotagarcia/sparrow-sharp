using Android.Util;
using OpenTK;
using OpenTK.Graphics.ES20;
using OpenTK.Platform.Android;
using Sparrow.Display;
using System;
using System.Collections.Generic;
using sparrowsharp;
using Sparrow.Geom;
using OpenTK.Graphics;
using Android.Content.Res;

namespace Sparrow.Core
{
	public class ViewController : AndroidGameView
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
		public static Resources ResourcesRef;

		public ViewController (Android.Content.Context context, IAttributeSet attrs) : base (context, attrs)
		{
			Setup ();
		}

		public ViewController (IntPtr handle, Android.Runtime.JniHandleOwnership transfer) : base (handle, transfer)
		{
			Setup ();
		}

		public ViewController (Android.Content.Context context) : base (context)
		{
			Setup ();
		}

		public void Setup ()
		{
			ResourcesRef = Resources;
		}
		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			ReadjustStageSize (); // TODO check if Width/Height are not 0 here
			CreateRoot ();
			// Run the render loop
			Run ();
		}
		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		//
		// In this particular case, we demonstrate how to set
		// the graphics mode and fallback in case the device doesn't
		// support the defaults
		protected override void CreateFrameBuffer ()
		{
			ContextRenderingApi = GLVersion.ES2;

			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try {
				Log.Verbose ("Sparrow", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();

				Stage = new Stage ();

				//Juggler = new Juggler();
				SPContext = new Context (GraphicsContext);
				SP.CurrentController = this;
				SP.Context = SPContext;
				// Context.setCurrentContext() ??
				RenderSupport = new RenderSupport ();

				return;
			} catch (Exception ex) {
				Log.Verbose ("Sparrow", "{0}", ex);
			}
			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("Sparrow", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false); // TODO this is for GL 1.1

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("Sparrow", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}
		// This gets called on each frame render
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);

			//MakeCurrent();
			// same as (void)glkView:(GLKView *)view drawInRect:(CGRect)rect ??
			SP.CurrentController = this;  		 
			//??? neded? Context.SetCurrentContext(_context);

			GL.Disable (All.CullFace);
			GL.Disable (All.DepthTest);
			GL.Enable (All.Blend);

			RenderSupport.NextFrame ();
			Stage.Render (RenderSupport);
			RenderSupport.FinishQuadBatch ();

			#if DEBUG
			RenderSupport.CheckForOpenGLError ();
			#endif

			Stage.AdvanceTime ((float)e.Time);

//			Console.WriteLine ("Number of draw calls: " + RenderSupport.NumDrawCalls);

			SwapBuffers ();
		}

		public void Start (Type RootClass)
		{
			if (_rootClass != null) {
				throw new Exception ("Sparrow has already been started");
			}
			_rootClass = RootClass;
		}

		public void CreateRoot ()
		{
			if (Root == null) {
                // hope iOS wont complain about such dynamic stuff
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
		}

		private void ReadjustStageSize ()
		{
			Stage.Width = Size.Width * _viewScaleFactor / _contentScaleFactor;
			Stage.Height = Size.Height * _viewScaleFactor / _contentScaleFactor;
		}

		// this is called whenever android raises the SurfaceChanged event
		protected override void OnResize (EventArgs e)
		{
			// the surface change event makes your context
			// not be current, so be sure to make it current again
			MakeCurrent ();
			ReadjustStageSize ();
		}
	}
}

