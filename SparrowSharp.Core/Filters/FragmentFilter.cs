using System;
using Sparrow.Utils;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using System.Collections.Generic;
using Sparrow.Core;
using Sparrow;
using OpenTK.Graphics.ES20;
using OpenTK;
using System.Text;

namespace SparrowSharp.Filters
{
	public abstract class FragmentFilter
	{
		public const int MIN_TEXTURE_SIZE = 64;
		private bool _cached;
		/// Indicates if the filter is cached (via the "cache" method).
		public bool Cached {get {return _cached;} }

		/// The resolution of the filter texture. "1" means stage resolution, "0.5" half the stage
		/// resolution. A lower resolution saves memory and execution time(depending on the GPU), but
		/// results in a lower output quality. Values greater than 1 are allowed; such values might make
		/// sense for a cached filter when it is scaled up. default 1
		public float Resolution;

		/// The filter mode, which is one of the constants defined in the 'FragmentFilterMode' enum.
		/// (default: FragmentFilterMode.Replace)
		public FragmentFilterMode Mode;

		/// Use the x-offset to move the filter output to the right or left.
		public float OffsetX;

		/// Use the y-offset to move the filter output to the top or bottom.
		public float OffsetY;

		/// The x-margin will extend the size of the filter texture along the x-axis.
		/// Useful when the filter will "grow" the rendered object.
		public float MarginX;

		/// The y-margin will extend the size of the filter texture along the y-axis.
		/// Useful when the filter will "grow" the rendered object.
		public float MarginY;

		/// The number of passes the filter is applied. The "activate" and "deactivate" methods will be
		/// called that often.
		public int NumPasses;

		/// The ID of the vertex buffer attribute that stores the vertex position.
		public int VertexPosID;

		/// The ID of the vertex buffer attribute that stores the SPTexture coordinates.
		public int TexCoordsID;

		private List<Texture> _passTextures;
		private Matrix _projMatrix;
		private QuadBatch _cache;
		private bool _cacheRequested;
		private VertexData _vertexData;
		private ushort[] _indexData = new ushort[6];
		private uint _vertexBufferName;
		private uint _indexBufferName;

		/// Initializes a fragment filter with the specified number of passes and resolution.
		/// This initializer must only be called by the initializer of a subclass.
		public FragmentFilter (int numPasses = 1, float resolution = 1.0f)
		{
			NumPasses = numPasses;
			Resolution = resolution;
			Mode = FragmentFilterMode.Replace;
			_passTextures = new List<Texture> (numPasses);
			_projMatrix = Matrix.Create(0,0,0,0,0,0);

			_vertexData = new VertexData(4, true);
			_vertexData.Vertices[1].TexCoords.X = 1.0f;
			_vertexData.Vertices[2].TexCoords.Y = 1.0f;
			_vertexData.Vertices[3].TexCoords.X = 1.0f;
			_vertexData.Vertices[3].TexCoords.Y = 1.0f;

			_indexData[0] = 0;
			_indexData[1] = 1;
			_indexData[2] = 2;
			_indexData[3] = 1;
			_indexData[4] = 3;
			_indexData[5] = 2;

			CreatePrograms();
		}

		/// Caches the filter output into a SPTexture. An uncached filter is rendered in every frame; a
		/// cached filter only once. However, if the filtered object or the filter settings change, it has
		/// to be updated manually; to do that, call "cache" again.
		public void Cache() 
		{
			_cacheRequested = true;
			DisposeCache ();
		}

		/// Clears the cached output of the filter. After calling this method, the filter will be executed
		/// once per frame again.
		public void ClearCache() 
		{
			_cacheRequested = false;
			DisposeCache ();
		}

		/// Applies the filter on a certain display object, rendering the output into the current render
		/// target. This method is called automatically by Sparrow's rendering system for the object the
		/// filter is attached to.
		public void RenderObject(DisplayObject obj, RenderSupport support) 
		{
			// bottom layer
			if (Mode == FragmentFilterMode.Above) {
				obj.Render(support);
			}
				
			// center layer
			if (_cacheRequested)
			{
				_cacheRequested = false;
				_cache = RenderPasses(obj, support, true);
				DisposePassTextures ();
			}

			if (_cache != null) {
				_cache.Render (support);
			} else {
				RenderPasses(obj, support, false);
			}

			// top layer
			if (Mode == FragmentFilterMode.Below) {
				obj.Render (support);
			}
		}

		public abstract void CreatePrograms();

		public abstract void ActivateWithPass (int pass, Texture texture, Matrix mvpMatrix);

		public void DeactivateWithPass (int pass, Texture texture)
		{
			// override in subclass
		}

		public static String StandardVertexShader()
		{
			StringBuilder source = new StringBuilder("");
			source.AppendLine("attribute vec4 aPosition;");
			source.AppendLine("attribute lowp vec2 aTexCoords;");
			source.AppendLine("uniform mat4 uMvpMatrix;");
			source.AppendLine("varying lowp vec2 vTexCoords;");
			source.AppendLine("void main() {");
			source.AppendLine("    gl_Position = uMvpMatrix * aPosition;");
			source.AppendLine("    vTexCoords  = aTexCoords;");
			source.AppendLine("}");
			return source.ToString ();
		}

		public static String StandardFragmentShader()
		{
			StringBuilder source = new StringBuilder("");
			source.AppendLine("uniform lowp sampler2D uTexture;");
			source.AppendLine("varying lowp vec2 vTexCoords;");
			source.AppendLine("void main() {");
			source.AppendLine("    gl_FragColor = texture2D(uTexture, vTexCoords);");
			source.AppendLine("}");
			return source.ToString ();
		}

		// TODO: do we want to support POT textures?
		private void CalcBounds(DisplayObject obj,
			Stage stage, 
			float scale,
			bool intersectWithStage,
			out Rectangle bounds,
			out Rectangle boundsPOT)
		{
			float marginX;
			float marginY;

			// optimize for full-screen effects
			if (obj == stage || obj == SparrowSharpApp.Root)
			{
				marginX = marginY = 0;
				bounds = new Rectangle(0, 0, stage.Width, stage.Height);
			}
			else
			{
				marginX = MarginX;
				marginY = MarginY;
				bounds = obj.BoundsInSpace(stage);
			}

			if (intersectWithStage)
				bounds = bounds.Intersection (stage.Bounds);
			boundsPOT = null;
			Rectangle result = bounds;
			if (!result.IsEmpty())
			{
				// the bounds are a rectangle around the object, in stage coordinates,
				// and with an optional margin.
				bounds.Inflate(marginX, marginY);

				// To fit into a POT-texture, we extend it towards the right and bottom.
				int minSize = (int)(MIN_TEXTURE_SIZE / scale);
				float minWidth  = result.Width  > minSize ? result.Width  : minSize;
				float minHeight = result.Height > minSize ? result.Height : minSize;

				boundsPOT = new Rectangle(result.X,
					result.Y,
					NumberUtil.NextPowerOfTwo(minWidth  * scale) / scale,
					NumberUtil.NextPowerOfTwo(minHeight * scale) / scale);
			}
		}

		private QuadBatch CompileWith(DisplayObject obj)
		{
			if (_cache != null)
				return _cache;
			else
			{
				if (obj.Stage == null)
					throw new  InvalidOperationException(@"Filtered object must be on the stage.");

				RenderSupport support = new RenderSupport();
				support.PushState(obj.TransformationMatrixToSpace(obj.Stage), obj.Alpha, obj.BlendMode);

				return RenderPasses(obj, support, true);
			}
		}

		private void DisposeCache()
		{
			_cache = null;
		}

		private void DisposePassTextures()
		{
			_passTextures.Clear ();
		}

		private Texture PassTextureForPass(int pass)
		{
			return _passTextures[pass % 2];
		}

		private QuadBatch RenderPasses(DisplayObject obj, RenderSupport support, bool intoCache)
		{
			Texture cacheTexture = null;
			Stage stage = obj.Stage;
			float scale = SparrowSharpApp.ContentScaleFactor * Resolution;

			if (stage == null)
				throw new InvalidOperationException ("Filtered object must be on the stage.");

			// the bounds of the object in stage coordinates
			Rectangle boundsPOT;
			Rectangle bounds;
			CalcBounds(obj, stage, scale, !intoCache, out bounds, out boundsPOT);

			if (bounds.IsEmpty())
			{
				DisposePassTextures();
				return intoCache ? new QuadBatch() : null;
			}

			UpdateBuffers(boundsPOT);
			UpdatePassTextures((int)boundsPOT.Width, (int)boundsPOT.Height, scale);

			support.FinishQuadBatch ();
			support.AddDrawCalls(NumPasses);
			support.PushState(Matrix.Create(), 1.0f, BlendMode.AUTO);

			// save original projection matrix and render target
			_projMatrix.CopyFromMatrix(support.ProjectionMatrix);
			Texture previousRenderTarget = support.RenderTarget;

			// use cache?
			if (intoCache) {
				cacheTexture = CreateTexture((int)boundsPOT.Width, (int)boundsPOT.Height, scale);
			}

			// draw the original object into a texture
			support.RenderTarget = _passTextures[0];
			SparrowSharpApp.Context.ScissorBox = null; // we want the entire texture cleared
			support.Clear ();
			support.BlendMode = BlendMode.NORMAL;
			support.SetupOrthographicProjection(boundsPOT.Left, boundsPOT.Right, boundsPOT.Bottom, boundsPOT.Top);
			obj.Render (support);
			support.FinishQuadBatch ();

			// prepare drawing of actual filter passes
			support.ApplyBlendMode (true);
			support.ModelViewMatrix.Identity ();
			support.PushClipRect (bounds);

			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);

			GL.EnableVertexAttribArray (VertexPosID);
			GL.VertexAttribPointer (VertexPosID, 2, All.Float, false, Vertex.SIZE, (IntPtr)Vertex.POSITION_OFFSET);

			GL.EnableVertexAttribArray (TexCoordsID);
			GL.VertexAttribPointer (TexCoordsID, 2, All.Float, false, Vertex.SIZE, (IntPtr)Vertex.TEXTURE_OFFSET);

			// draw all passes
			for (int i=0; i<NumPasses; ++i)
			{
				if (i < NumPasses - 1) // intermediate pass
				{
					// draw into pass texture
					support.RenderTarget = PassTextureForPass (i + 1);
					support.Clear ();
				}
				else // final pass
				{
					if (intoCache)
					{
						// draw into cache texture
						support.RenderTarget = cacheTexture;
						support.Clear ();
					}
					else
					{
						// draw into back buffer, at original (stage) coordinates
						support.RenderTarget = previousRenderTarget;
						support.ProjectionMatrix = _projMatrix;
						support.ModelViewMatrix.Translate (OffsetX, OffsetY);
						support.BlendMode = obj.BlendMode;
						support.ApplyBlendMode (true);
					}
				}

				Texture passTexture = PassTextureForPass (i);

				GL.ActiveTexture (All.Texture0);
				GL.BindTexture (All.Texture2D, passTexture.Name);

				ActivateWithPass (i, passTexture, support.MvpMatrix);
				GL.DrawElements (All.Triangles, 6, All.UnsignedShort, IntPtr.Zero);
				DeactivateWithPass (i, passTexture);
			}

			support.PopState ();
			support.PopClipRect ();

			QuadBatch cache = null;
			if (intoCache)
			{
				// restore support settings
				support.RenderTarget = previousRenderTarget;
				support.ProjectionMatrix = _projMatrix;

				// Create an image containing the cache. To have a display object that contains
				// the filter output in object coordinates, we wrap it in a QuadBatch: that way,
				// we can modify it with a transformation matrix.

				cache = new QuadBatch ();
				Image image = new Image (cacheTexture);

				Matrix matrix = stage.TransformationMatrixToSpace (obj);
				matrix.Translate (bounds.X + OffsetX, bounds.Y + OffsetY);
				cache.AddQuad (image, 1.0f, BlendMode.AUTO, matrix);
			}

			return cache;
		}

		private void UpdateBuffers(Rectangle bounds)
		{
			Vertex[] vertices = _vertexData.Vertices;
			vertices[0].Position = new Vector2(bounds.X, bounds.Y);
			vertices[1].Position = new Vector2(bounds.Right, bounds.Y);
			vertices[2].Position = new Vector2(bounds.X,     bounds.Bottom);
			vertices[3].Position = new Vector2(bounds.Right, bounds.Bottom);

			const int indexSize = sizeof(ushort) * 6;
			const int vertexSize = Vertex.SIZE * 4;

			if (_vertexBufferName == 0)
			{
				GL.GenBuffers (1, out _vertexBufferName);
				GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);

				GL.GenBuffers (1, out _indexBufferName);
				GL.BindBuffer (All.ElementArrayBuffer, _indexBufferName);
				GL.BufferData (All.ElementArrayBuffer, (IntPtr)indexSize, _indexData, All.StaticDraw);
			}

			GL.BindBuffer (All.ArrayBuffer, _vertexBufferName);
			GL.BufferData (All.ArrayBuffer, (IntPtr)vertexSize, _vertexData.Vertices, All.StaticDraw);
		}

		private void UpdatePassTextures(int width, int height, float scale)
		{
			int numPassTextures = NumPasses > 1 ? 2 : 1;
			bool needsUpdate = _passTextures.Count != numPassTextures ||
				_passTextures[0].Width != width ||
				_passTextures[0].Height != height;

			if (needsUpdate)
			{
				_passTextures.Clear ();
				for (int i = 0; i < numPassTextures; ++i)
					_passTextures.Add (CreateTexture (width, height, scale));
			}
		}

		private Texture CreateTexture(int width, int height, float scale)
		{
			int legalWidth = NumberUtil.NextPowerOfTwo (width * scale);
			int legalHeight = NumberUtil.NextPowerOfTwo (height * scale);

			TextureProperties texProps = new TextureProperties {
				TextureFormat = TextureFormat.Rgba4444,
				Scale  = scale,
				Width  = legalWidth,
				Height = legalHeight,
				NumMipmaps = 0,
				GenerateMipmaps = false,
				PremultipliedAlpha = true
			};

			return new GLTexture (IntPtr.Zero, texProps);
		}


	}
}

