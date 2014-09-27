using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.ES20;
using Sparrow;
using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace SparrowSharp.Filters
{
    /// <summary>
    /// The FragmentFilter class is the base class for all filter effects in Sparrow.
    /// <para></para> 
    /// All other filters of this package extend this class. You can attach them to any display object
    /// through the 'filter' property.
    /// <para></para> 
    /// <para>A fragment filter works in the following way:</para> 
    /// <para>* The object that is filtered is rendered into a texture (in stage coordinates).</para>
    /// <para>* That texture is passed to the first filter pass.</para>
    /// <para>* Each pass processes the texture using a fragment shader (and optionally a vertex shader) to
    ///   achieve a certain effect.</para>
    /// <para>* The output of each pass is used as the input for the next pass; if it's the final pass, it will
    ///   be rendered directly to the back buffer.</para>
    /// <para></para>
    /// All of this is set up by the abstract FragmentFilter class. Concrete subclasses just need to 
    /// override the protected methods 'createPrograms', 'activateWithPass' and (optionally) 
    /// 'deactivateWithPass' to create and execute its custom shader code. Each filter can be configured to 
    /// either replace the original object, or be drawn below or above it. This can be done through the 
    /// 'mode' property, which accepts one of the enums defined in the 'SPFragmentFilterMode' enum.
    /// <para></para>
    /// Beware that each filter should be used only on one object at a time. Otherwise, it will get slower
    /// and require more resources; and caching will lead to undefined results.
    /// </summary>
    public abstract class FragmentFilter
    {
        protected const int MIN_TEXTURE_SIZE = 64;
        private bool _cached;

        /// <summary>
        /// Indicates if the filter is cached (via the "Cache()" method).
        /// </summary>
        /// <value><c>true</c> if cached; otherwise, <c>false</c>.</value>
        public bool Cached { get { return _cached; } }

        /// <summary>
        /// The resolution of the filter texture. "1" means stage resolution, "0.5" half the stage
        /// resolution. A lower resolution saves memory and execution time(depending on the GPU), but
        /// results in a lower output quality. Values greater than 1 are allowed; such values might make
        /// sense for a cached filter when it is scaled up. default 1
        /// </summary>
        public readonly float Resolution;
        /// <summary>
        /// The filter mode, which is one of the constants defined in the 'FragmentFilterMode' enum.
        /// (default: FragmentFilterMode.Replace)
        /// </summary>
        public FragmentFilterMode Mode;
        /// <summary>
        /// Use the x-offset to move the filter output to the right or left.
        /// </summary>
        public float OffsetX;
        /// <summary>
        /// Use the y-offset to move the filter output to the top or bottom.
        /// </summary>
        public float OffsetY;
        /// <summary>
        /// The x-margin will extend the size of the filter texture along the x-axis.
        /// Useful when the filter will "grow" the rendered object.
        /// </summary>
        public float MarginX;
        /// <summary>
        /// The y-margin will extend the size of the filter texture along the y-axis.
        /// Useful when the filter will "grow" the rendered object.
        /// </summary>
        public float MarginY;
        /// <summary>
        /// The number of passes the filter is applied. The "activate" and "deactivate" methods will be
        /// called that often.
        /// </summary>
        protected int NumPasses;
        /// <summary>
        /// The ID of the vertex buffer attribute that stores the Vertex position.
        /// </summary>
        protected int VertexPosID;
        /// <summary>
        /// The ID of the vertex buffer attribute that stores the Texture coordinates.
        /// </summary>
        protected int TexCoordsID;
        private readonly List<Texture> _passTextures;
        private readonly Matrix _projMatrix;
        private QuadBatch _cache;
        private bool _cacheRequested;
        private readonly VertexData _vertexData;
        private readonly ushort[] _indexData = new ushort[6];
        private uint _vertexBufferName;
        private uint _indexBufferName;

        /// <summary>
        /// Initializes a fragment filter with the specified number of passes and resolution.
        /// </summary>
        protected FragmentFilter(int numPasses = 1, float resolution = 1.0f)
        {
            NumPasses = numPasses;
            Resolution = resolution;
            Mode = FragmentFilterMode.Replace;
            _passTextures = new List<Texture>(numPasses);
            _projMatrix = Matrix.Create(0, 0, 0, 0, 0, 0);

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

        /// <summary>
        /// Caches the filter output into a Texture. An uncached filter is rendered in every frame; a
        /// cached filter only once. However, if the filtered object or the filter settings change, it has
        /// to be updated manually; to do that, call "cache" again.
        /// </summary>
        public void Cache()
        {
            _cacheRequested = true;
            DisposeCache();
            _cached = true;
        }

        /// <summary>
        /// Clears the cached output of the filter. After calling this method, the filter will be executed
        /// once per frame again.
        /// </summary>
        public void ClearCache()
        {
            _cacheRequested = false;
            DisposeCache();
            _cached = false;
        }

        /// <summary>
        /// Applies the filter on a certain display object, rendering the output into the current render
        /// target. This method is called automatically by Sparrow's rendering system for the object the
        /// filter is attached to.
        /// </summary>
        public void RenderObject(DisplayObject obj, RenderSupport support)
        {
            // bottom layer
            if (Mode == FragmentFilterMode.Above)
            {
                obj.Render(support);
            }
				
            // center layer
            if (_cacheRequested)
            {
                _cacheRequested = false;
                _cache = RenderPasses(obj, support, true);
                DisposePassTextures();
            }

            if (_cache != null)
            {
                _cache.Render(support);
            }
            else
            {
                RenderPasses(obj, support, false);
            }

            // top layer
            if (Mode == FragmentFilterMode.Below)
            {
                obj.Render(support);
            }
        }

        /// <summary>
        /// Subclasses must override this method and use it to create their fragment and vertex shaders.
        /// </summary>
        protected abstract void CreatePrograms();

        /// <summary>
        /// Subclasses must override this method and use it to activate their shader program.
        /// The 'ActivateWithPass' call directly precedes the call to 'GL.DrawElements'.
        /// </summary>
        protected abstract void ActivateWithPass(int pass, Texture texture, Matrix mvpMatrix);

        /// <summary>
        /// This method is called directly after 'GL.DrawElements'.
        /// If you need to clean up any resources, you can do so in this method
        /// </summary>
        protected void DeactivateWithPass(int pass, Texture texture)
        {
            // override in subclass
        }

        /// <summary>
        /// The standard vertex shader code.
        /// </summary>
        protected static String StandardVertexShader()
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
            return source.ToString();
        }

        /// <summary>
        /// The standard fragment shader code. It just forwards the texture color to the output.
        /// </summary>
        protected static String StandardFragmentShader()
        {
            StringBuilder source = new StringBuilder("");
            #if __WINDOWS__
			source.AppendLine("uniform sampler2D uTexture;");
            #else
            source.AppendLine("uniform lowp sampler2D uTexture;");
            #endif
            source.AppendLine("varying lowp vec2 vTexCoords;");
            source.AppendLine("void main() {");
            source.AppendLine("    gl_FragColor = texture2D(uTexture, vTexCoords);");
            source.AppendLine("}");
            return source.ToString();
        }

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
                bounds = bounds.Intersection(stage.Bounds);
            boundsPOT = null;
            Rectangle result = bounds;
            if (!result.IsEmpty())
            {
                // the bounds are a rectangle around the object, in stage coordinates,
                // and with an optional margin.
                bounds.Inflate(marginX, marginY);

                // To fit into a POT-texture, we extend it towards the right and bottom.
                int minSize = (int)(MIN_TEXTURE_SIZE / scale);
                float minWidth = result.Width > minSize ? result.Width : minSize;
                float minHeight = result.Height > minSize ? result.Height : minSize;

                boundsPOT = new Rectangle(result.X,
                    result.Top,
                    NumberUtil.NextPowerOfTwo(minWidth * scale) / scale,
                    NumberUtil.NextPowerOfTwo(minHeight * scale) / scale);
            }
        }

        private void DisposeCache()
        {
            _cache = null;
        }

        private void DisposePassTextures()
        {
            _passTextures.Clear();
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
            {
                throw new InvalidOperationException("Filtered object must be on the stage.");
            }
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

            support.FinishQuadBatch();
            support.AddDrawCalls(NumPasses);
            support.PushState(Matrix.Create(), 1.0f, BlendMode.AUTO);

            // save original projection matrix and render target
            _projMatrix.CopyFromMatrix(support.ProjectionMatrix);
            Texture previousRenderTarget = support.RenderTarget;

            // use cache?
            if (intoCache)
            {
                cacheTexture = CreateTexture((int)boundsPOT.Width, (int)boundsPOT.Height, scale);
            }

            // draw the original object into a texture
            support.RenderTarget = _passTextures[0];
            SparrowSharpApp.Context.ScissorBox = null; // we want the entire texture cleared
            support.Clear();
            support.BlendMode = BlendMode.NORMAL;
            support.SetupOrthographicProjection(boundsPOT.Left, boundsPOT.Right, boundsPOT.Bottom, boundsPOT.Top);
            obj.Render(support);
            support.FinishQuadBatch();

            // prepare drawing of actual filter passes
            support.ApplyBlendMode(true);
            support.ModelViewMatrix.Identity();
            support.PushClipRect(bounds);

			GL.BindBuffer (BufferTarget.ArrayBuffer, _vertexBufferName);
			GL.BindBuffer (BufferTarget.ElementArrayBuffer, _indexBufferName);

			GL.EnableVertexAttribArray (VertexPosID);
			GL.VertexAttribPointer (VertexPosID, 2, VertexAttribPointerType.Float, false, Vertex.SIZE, (IntPtr)Vertex.POSITION_OFFSET);

			GL.EnableVertexAttribArray (TexCoordsID);
			GL.VertexAttribPointer (TexCoordsID, 2, VertexAttribPointerType.Float, false, Vertex.SIZE, (IntPtr)Vertex.TEXTURE_OFFSET);

            // draw all passes
            for (int i = 0; i < NumPasses; ++i)
            {
                if (i < NumPasses - 1)
                { // intermediate pass
                    // draw into pass texture
                    support.RenderTarget = PassTextureForPass(i + 1);
                    support.Clear();
                }
                else
                { // final pass
                    if (intoCache)
                    {
                        // draw into cache texture
                        support.RenderTarget = cacheTexture;
                        support.Clear();
                    }
                    else
                    {
                        // draw into back buffer, at original (stage) coordinates
                        support.RenderTarget = previousRenderTarget;
                        support.ProjectionMatrix = _projMatrix;
                        support.ModelViewMatrix.Translate(OffsetX, OffsetY);
                        support.BlendMode = obj.BlendMode;
                        support.ApplyBlendMode(true);
                    }
                }

                Texture passTexture = PassTextureForPass(i);

				GL.ActiveTexture (TextureUnit.Texture0);
				GL.BindTexture (TextureTarget.Texture2D, passTexture.Name);

				ActivateWithPass (i, passTexture, support.MvpMatrix);
				GL.DrawElements (BeginMode.Triangles, 6, DrawElementsType.UnsignedShort, IntPtr.Zero);
                
                DeactivateWithPass(i, passTexture);
            }

            GL.DisableVertexAttribArray(VertexPosID);
            GL.DisableVertexAttribArray(TexCoordsID);

            support.PopState();
            support.PopClipRect();

            QuadBatch cache = null;
            if (intoCache)
            {
                // restore support settings
                support.RenderTarget = previousRenderTarget;
                support.ProjectionMatrix = _projMatrix;

                // Create an image containing the cache. To have a display object that contains
                // the filter output in object coordinates, we wrap it in a QuadBatch: that way,
                // we can modify it with a transformation matrix.
                cache = new QuadBatch();
                Image image = new Image(cacheTexture);

                Matrix matrix = stage.TransformationMatrixToSpace(obj);
                // Note: the next line was originally:
                // matrix.Translate (bounds.X + OffsetX, bounds.Y + OffsetY);
                // this seems like a sparrow-s bug; fix is from Starling
                matrix.PrependTranslation(bounds.X + OffsetX, bounds.Top + OffsetY);
                cache.AddQuad(image, 1.0f, BlendMode.AUTO, matrix);
            }

            return cache;
        }

        private void UpdateBuffers(Rectangle bounds)
        {
            Vertex[] vertices = _vertexData.Vertices;
            vertices[0].Position = new Vector2(bounds.X, bounds.Top);
            vertices[1].Position = new Vector2(bounds.Right, bounds.Top);
            vertices[2].Position = new Vector2(bounds.X, bounds.Bottom);
            vertices[3].Position = new Vector2(bounds.Right, bounds.Bottom);

            const int indexSize = sizeof(ushort) * 6;
            const int vertexSize = Vertex.SIZE * 4;

			if (_vertexBufferName == 0) {
				GL.GenBuffers (1, out _vertexBufferName);
				GL.BindBuffer (BufferTarget.ArrayBuffer, _vertexBufferName);

                GL.GenBuffers (1, out _indexBufferName);
                GL.BindBuffer (BufferTarget.ElementArrayBuffer, _indexBufferName);
				GL.BufferData (BufferTarget.ElementArrayBuffer, (IntPtr)indexSize, _indexData, BufferUsage.StaticDraw);
			}

			GL.BindBuffer (BufferTarget.ArrayBuffer, _vertexBufferName);
			GL.BufferData (BufferTarget.ArrayBuffer, (IntPtr)vertexSize, _vertexData.Vertices, BufferUsage.StaticDraw);
        }

        private void UpdatePassTextures(int width, int height, float scale)
        {
            int numPassTextures = NumPasses > 1 ? 2 : 1;
            bool needsUpdate = _passTextures.Count != numPassTextures ||
                      _passTextures[0].Width != width ||
                      _passTextures[0].Height != height;

            if (needsUpdate)
            {
                _passTextures.Clear();
                for (int i = 0; i < numPassTextures; ++i)
                    _passTextures.Add(CreateTexture(width, height, scale));
            }
        }

        private Texture CreateTexture(int width, int height, float scale)
        {
            int legalWidth = NumberUtil.NextPowerOfTwo(width * scale);
            int legalHeight = NumberUtil.NextPowerOfTwo(height * scale);

            TextureProperties texProps = new TextureProperties
            {
                TextureFormat = TextureFormat.Rgba8888,
                Scale = scale,
                Width = legalWidth,
                Height = legalHeight,
                NumMipmaps = 0,
                GenerateMipmaps = false,
                PremultipliedAlpha = true
            };

            return new GLTexture(IntPtr.Zero, texProps);
        }
    }
}

