using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
using Android.Util;
using Sparrow.Core;
using System.Text;
using System.Runtime.InteropServices;
using Sparrow.Geom;
using Sparrow;

namespace componenttest
{
	class GLView1 : AndroidGameView
	{
		BaseEffect baseEffect;

		public GLView1 (Android.Content.Context context) : base (context)
		{
		}
		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

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
				Log.Verbose ("GLCube", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("GLCube", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}
			
		// This gets called on each frame render
		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);

			if (baseEffect == null) {

				baseEffect = new BaseEffect();
				baseEffect.Alpha = 1.0f;

				Matrix mvMatrix = new Matrix ();
			
				baseEffect.MvpMatrix = mvMatrix;
			}

			baseEffect.PrepareToDraw ();
			float[] vertices = new float [] {
				0.0f, 0.5f, 0.0f,
				-0.5f, -0.5f, 0.0f,
				0.5f, -0.5f, 0.0f
			};
			float[] triangleColors = new float[]
			{
				1.0f, 0.0f, 0.0f, 1.0f,
				0.0f, 1.0f, 0.0f, 1.0f,
				0.0f, 1.0f, 0.0f, 1.0f,
			};

			GL.ClearColor (0.7f, 0.7f, 0.7f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			GL.Viewport (0, 0, Width, Height);

			GL.VertexAttribPointer(baseEffect.AttribPosition, 3, All.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (baseEffect.AttribPosition);

			GL.VertexAttribPointer(baseEffect.AttribColor, 4, All.Float, false, 0, triangleColors);
			GL.EnableVertexAttribArray(baseEffect.AttribColor);

			GL.DrawArrays (All.Triangles, 0, 3);

			SwapBuffers ();
		}

	}
}

