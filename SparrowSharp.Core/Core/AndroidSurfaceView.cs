using System;

using Android.Views;
using Android.Content;
using Android.Util;
using Android.Opengl;

namespace Sparrow.Core
{
	public class AndroidSurfaceView : GLSurfaceView
	{
		private AndroidGLRenderer mRenderer;

		public AndroidSurfaceView (Android.Content.Context context) : base (context)
		{
			// Create an OpenGL ES 2.0 context.
			SetEGLContextClientVersion (2);

			// Set the Renderer for drawing on the GLSurfaceView
			mRenderer = new AndroidGLRenderer ();
			SetRenderer (mRenderer);

			// Render the view only when there is a change in the drawing data
			///this.RenderMode = Rendermode.WhenDirty;
		}

	}
}