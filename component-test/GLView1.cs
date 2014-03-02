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
using Sparrow.Display;
using Sparrow.Utils;

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

//			if (baseEffect == null) {
//				baseEffect = new BaseEffect ();
//				baseEffect.Alpha = 1.0f;
//
//				Matrix mvMatrix = new Matrix();
//				baseEffect.MvpMatrix = mvMatrix;
//			}
//			Random rnd = new Random ();
//			baseEffect.Alpha = rnd.Next(1, 100)/100.0f;
//			baseEffect.PrepareToDraw ();

			GL.ClearColor (0.7f, 0.7f, 0.7f, 1.0f);
			GL.Clear (ClearBufferMask.ColorBufferBit);

			GL.Viewport (0, 0, Width, Height);

			RenderQuadBatch ();
//			RenderVBO ();
//			RenderArray ();

			SwapBuffers ();
		}

		private VertexData _vertexData;
		private int _vertexBufferName;
		private ushort[] _indexData;
		private int _indexBufferName;

		private QuadBatch _quadBatch;

		private void RenderQuadBatch() {
			if (_quadBatch == null) {
				Quad quad = new Quad (0.5f, 0.5f, 0x00FF00);

				_quadBatch = new QuadBatch ();
				_quadBatch.AddQuad (quad);
			}

			_quadBatch.Render (new Matrix(), 1.0f, BlendMode.NORMAL);
		}

		private void CreateVBO ()
		{
			Quad quad = new Quad (0.5f, 0.5f, 0xFF0000);

			_vertexData = new VertexData (4);
			quad.CopyVertexDataTo (_vertexData, 0);

			int numVertices = _vertexData.NumVertices;
			int numIndices = numVertices / 4 * 6;

			_indexData = new ushort[numIndices];

			for (uint i = 0; i < numIndices / 6; ++i) {
				_indexData [i * 6] = (ushort)(i * 4);
				_indexData [i * 6 + 1] = (ushort)(i * 4 + 1);
				_indexData [i * 6 + 2] = (ushort)(i * 4 + 2);
				_indexData [i * 6 + 3] = (ushort)(i * 4 + 1);
				_indexData [i * 6 + 4] = (ushort)(i * 4 + 3);
				_indexData [i * 6 + 5] = (ushort)(i * 4 + 2);
			}

			GL.GenBuffers (1, out _indexBufferName);
			GL.GenBuffers (1, out _vertexBufferName);

			GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);
			GL.BufferData (All.ElementArrayBuffer, (IntPtr)(sizeof(ushort) * numIndices), _indexData, All.StaticDraw);

			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BufferData (All.ArrayBuffer, (IntPtr)(_vertexData.NumVertices * 5 * sizeof(float)), _vertexData.Vertices, All.StaticDraw);
		}

		private void RenderVBO ()
		{
			if (_vertexData == null) {
				CreateVBO ();
			}

			int attribPosition = baseEffect.AttribPosition;
			int attribColor = baseEffect.AttribColor;

			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);

			int sizeOfVertex = Marshal.SizeOf (typeof(Vertex));
			IntPtr positionOffset = Marshal.OffsetOf (typeof(Vertex), "Position");
			IntPtr colorOffset = Marshal.OffsetOf (typeof(Vertex), "Color");

			GL.VertexAttribPointer (attribPosition, 2, All.Float, false, sizeOfVertex, positionOffset);
			GL.EnableVertexAttribArray (attribPosition);

			GL.VertexAttribPointer (attribColor, 4, All.Byte, true, sizeOfVertex, colorOffset);
			GL.EnableVertexAttribArray (attribColor);

			int numIndices = 6;
			GL.DrawElements (All.Triangles, numIndices, All.UnsignedShort, IntPtr.Zero);
		}

		private void RenderArray ()
		{
			float[] vertices = new float [] {
				0.0f, 0.5f, 0.0f,
				-0.5f, -0.5f, 0.0f,
				0.5f, -0.5f, 0.0f
			};
			float[] triangleColors = new float[] {
				1.0f, 0.0f, 0.0f, 1.0f,
				0.0f, 1.0f, 0.0f, 1.0f,
				0.0f, 1.0f, 0.0f, 1.0f,
			};

			GL.VertexAttribPointer (baseEffect.AttribPosition, 3, All.Float, false, 0, vertices);
			GL.EnableVertexAttribArray (baseEffect.AttribPosition);
			
			GL.VertexAttribPointer (baseEffect.AttribColor, 4, All.Float, false, 0, triangleColors);
			GL.EnableVertexAttribArray (baseEffect.AttribColor);
			
			GL.DrawArrays (All.Triangles, 0, 3);
		}
	}
}

