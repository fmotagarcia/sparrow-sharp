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

namespace componenttest
{
	class GLView : AndroidGameView
	{
		private Program _program;

		public GLView (Android.Content.Context context) : base (context)
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

			// simple shareds, that work
			string vertexShaderSrc =  
				"attribute vec4 aPosition;    \n" + 
				"void main()                  \n" +
				"{                            \n" +
				"   gl_Position = aPosition;  \n" +
				"}                            \n";

			string fragmentShaderSrc =
				"precision mediump float;\n" +
				"void main()                                  \n" +
				"{                                            \n" +
				"  gl_FragColor = vec4 (1.0, 0.0, 0.0, 1.0);  \n" +
				"}                                            \n";
			if (_program == null) {
				_program = new Program(VertexShaderForTexture(null, true), FragmentShaderForTexture(null, true) );
				//_program = new Program(vertexShaderSrc, fragmentShaderSrc );
			}
			int _programName = _program.Name;

			GL.UseProgram(_programName);

			///////// Stuff Im trying to get to work now
			int _aPosition = _program.Attributes["aPosition"];
			int _aColor = _program.Attributes["aColor"];
			int _uMvpMatrix = _program.Uniforms["uMvpMatrix"];
			int _uAlpha = _program.Uniforms["uAlpha"];

			GL.EnableVertexAttribArray (_aPosition);
			//			GL.EnableVertexAttribArray (_aColor);

			Matrix4 glkMvpMatrix = new Matrix4(
				0.0026f, 0, 0, 0,
				0, -0.0019f, 0, 0, 
				0, 0, 0, 0,
				-1f, 1.0f, 0, 1.0f); // values from debugging

//			Matrix4 glkMvpMatrix = new Matrix4(
//				1.0f, 0, 0, 0,
//				0, 1.0f, 0, 0, 
//				0, 0, 1.0f, 0,
//				0, 0, 0, 1.0f);

			GL.UniformMatrix4(_uMvpMatrix, false, ref glkMvpMatrix);
			GL.Uniform4(_uAlpha, 0.7f, 1.0f, 1.0f, 1.0f);

			//GL.BindAttribLocation (program, 0, "vPosition");

			float[] vertices = new float [] {
				100f, 200f, 0.0f,
				0f, 0f, 0.0f,
				200f, 0f, 0.0f
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

			GL.VertexAttribPointer(_aPosition, 3, All.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (_aPosition);

			GL.VertexAttribPointer(_aColor, 4, All.Float, false, 0, triangleColors);
			GL.EnableVertexAttribArray (_aColor);

			GL.DrawArrays (All.Triangles, 0, 3);

			SwapBuffers ();
		}

		// shaders from BaseEffect
		private String VertexShaderForTexture(Texture texture, bool useTinting)
		{
			bool hasTexture = (texture != null);
			System.Text.StringBuilder source = new System.Text.StringBuilder("");
			// variables
			source.AppendLine("attribute vec4 aPosition;");
			if (useTinting)	source.AppendLine("attribute vec4 aColor;");
			if (hasTexture)	source.AppendLine("attribute vec2 aTexCoords;");

			source.AppendLine("uniform mat4 uMvpMatrix;");
			if (useTinting)	source.AppendLine("uniform vec4 uAlpha;");
			if (useTinting)	source.AppendLine("varying lowp vec4 vColor;");
			if (hasTexture)	source.AppendLine("varying lowp vec2 vTexCoords;");

			// main
			source.AppendLine("void main() {");
			source.AppendLine("  gl_Position = uMvpMatrix * aPosition;");
			if (useTinting)	source.AppendLine("  vColor = aColor * uAlpha;");
			if (hasTexture)	source.AppendLine("  vTexCoords  = aTexCoords;");
			source.Append("}");

			return source.ToString();
		}

		private String FragmentShaderForTexture(Texture texture, bool useTinting)
		{
			bool hasTexture = (texture != null);
			System.Text.StringBuilder source = new System.Text.StringBuilder("");

			// variables
			if (useTinting) source.AppendLine("varying lowp vec4 vColor;");
			if (hasTexture)
			{
				source.AppendLine("varying lowp vec2 vTexCoords;");
				source.AppendLine("uniform lowp sampler2D uTexture;");
			}
			// main
			source.AppendLine("void main() {");
			if (hasTexture)
			{
				if (useTinting)	source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords) * vColor;");
				else source.AppendLine("  gl_FragColor = texture2D(uTexture, vTexCoords);");
			}
			else
			{
				source.AppendLine("  gl_FragColor = vColor;");
			}
			source.Append("}");

			return source.ToString();
		}


	}
}

