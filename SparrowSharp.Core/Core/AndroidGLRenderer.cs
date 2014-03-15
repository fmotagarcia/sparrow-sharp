using System;

using Android.Views;
using Android.Content;
using Android.Util;
using Android.Opengl;
using Android.OS;

using Java.Lang;
using Sparrow;

namespace Sparrow.Core
{
	public class AndroidGLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
	{

		#region IRenderer implementation
		public void OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
		{
			GLES20.GlDisable(GLES20.GlBlend);
			//GLES20.GlDisable(GLES20.GlCullFaceMode);
			GLES20.GlDisable(GLES20.GlDepthTest);
			GLES20.GlDisable(GLES20.GlDither);
			GLES20.GlDisable(GLES20.GlPolygonOffsetFill);
			GLES20.GlDisable(GLES20.GlSampleAlphaToCoverage);
			GLES20.GlDisable(GLES20.GlScissorTest);
			GLES20.GlDisable(GLES20.GlStencilTest);
		}

		public void OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl)
		{
			SP.Step(1/60.0f); //todo  fix if we need this class
			// calls SwapBuffers automatically
		}

		public void OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height)
		{
			// Adjust the viewport based on geometry changes, such as screen rotation
			GLES20.GlViewport (0, 0, width, height);
			SP.InitApp (width, height);
		}
		#endregion

	}

}

