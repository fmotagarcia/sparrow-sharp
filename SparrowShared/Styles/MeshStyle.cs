using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Rendering;
using System;

namespace Sparrow.Styles
{
    /** MeshStyles provide a means to completely modify the way a mesh is rendered.
     *  The base class provides Sparrow's standard mesh rendering functionality: colored and
     *  (optionally) textured meshes. Subclasses may add support for additional features like
     *  color transformations, normal mapping, etc.
     *
     *  <p><strong>Using styles</strong></p>
     *
     *  <p>First, create an instance of the desired style. Configure the style by updating its
     *  properties, then assign it to the mesh. Here is an example that uses a fictitious
     *  <code>ColorStyle</code>:</p>
     *
     *  <listing>
     *  Image image = new Image(heroTexture);
     *  ColorStyle colorStyle = new ColorStyle();
     *  colorStyle.RedOffset = 0.5;
     *  colorStyle.RedMultiplier = 2.0;
     *  image.Style = colorStyle;</listing>
     *
     *  <p>Beware:</p>
     *
     *  <ul>
     *    <li>A style instance may only be used on one object at a time.</li>
     *    <li>A style might require the use of a specific vertex format;
     *        when the style is assigned, the mesh is converted to that format.</li>
     *  </ul>
     *
     *  <p><strong>Creating your own styles</strong></p>
     *
     *  <p>To create custom rendering code in Sparrow, you need to extend two classes:
     *  <code>MeshStyle</code> and <code>MeshEffect</code>. While the effect class contains
     *  the actual OpenGL rendering code, the style provides the API that other developers will
     *  interact with.</p>
     *
     *  <p>Subclasses of <code>MeshStyle</code> will add specific properties that configure the
     *  style's outcome, like the <code>RedOffset</code> and <code>RedMultiplier</code> properties
     *  in the sample above. Here's how to properly create such a class:</p>
     *
     *  <ul>
     *    <li>Always provide a constructor that can be called without any arguments.</li>
     *    <li>Override <code>CopyFrom</code> — that's necessary for batching.</li>
     *    <li>Override <code>CreateEffect</code> — this method must return the
     *        <code>MeshEffect</code> that will do the actual OpenGL rendering.</li>
     *    <li>Override <code>UpdateEffect</code> — this configures the effect created above
     *        right before rendering.</li>
     *    <li>Override <code>CanBatchWith</code> if necessary — this method figures out if one
     *        instance of the style can be batched with another. If they all can, you can leave
     *        this out.</li>
     *  </ul>
     *
     *  <p>If the style requires a custom vertex format, you must also:</p>
     *
     *  <ul>
     *    <li>add a static constant called <code>VERTEX_FORMAT</code> to the class and</li>
     *    <li>override <code>get VertexFormat</code> and let it return exactly that format.</li>
     *  </ul>
     *
     *  <p>When that's done, you can turn to the implementation of your <code>MeshEffect</code>;
     *  the <code>CreateEffect</code>-override will return an instance of this class.
     *  Directly before rendering begins, Sparrow will then call <code>UpdateEffect()</code>
     *  to set it up.</p>
     *
     *  @see MeshEffect
     *  @see VertexDataFormat
     *  @see Sparrow.display.Mesh
     */
    public class MeshStyle
    {

        /// <summary>
        /// Dispatched every frame on styles assigned to display objects connected to the stage.
        /// </summary>
        public event DisplayObject.EnterFrameEventHandler EnterFrame;

        /// <summary>
        /// Indicates if pixels at the edges will be repeated or clamped.
        /// Only works for power-of-two textures. @default false
        /// </summary>
        public bool TextureRepeat;
        
        private readonly Type _type;
        private Mesh _target;
        private Texture _texture;
        private uint _textureBase;
        private TextureSmoothing _textureSmoothing;
        private VertexData _vertexData;   // just a reference to the target's vertex data
        private IndexData _indexData;     // just a reference to the target's index data

        /// <summary>
        /// Creates a new MeshStyle instance.
        /// Subclasses must provide a constructor that can be called without any arguments.
        /// </summary>
        public MeshStyle()
        {
            _textureSmoothing = TextureSmoothing.Bilinear;
            _type = GetType();
        }

        /// <summary>
        /// Copies all properties of the given style to the current instance (or a subset, if the
        /// classes don't match). Must be overridden by all subclasses!
        /// </summary>
        public virtual void CopyFrom(MeshStyle meshStyle)
        {
            _texture = meshStyle._texture;
            _textureBase = meshStyle._textureBase;
            TextureRepeat = meshStyle.TextureRepeat;
            _textureSmoothing = meshStyle._textureSmoothing;
        }

        /// <summary>
        /// Creates a clone of this instance. The method will work for subclasses automatically,
        /// no need to override it.
        /// </summary>
        public MeshStyle Clone()
        {
            MeshStyle clone = (MeshStyle)Activator.CreateInstance(_type);
            clone.CopyFrom(this);
            return clone;
        }

        /// <summary>
        /// Creates the effect that does the actual, low-level rendering.
        /// To be overridden by subclasses!
        /// </summary>
        public virtual MeshEffect CreateEffect()
        {
            return new MeshEffect();
        }

        /// <summary>
        /// Updates the settings of the given effect to match the current style.
        /// The given <code>effect</code> will always match the class returned by
        /// <code>CreateEffect()</code>. Called just before Render();
        ///
        /// <para>To be overridden by subclasses!</para>
        /// </summary>
        public virtual void UpdateEffect(MeshEffect effect, RenderState state)
        {
            effect.Texture = _texture;
            effect.TextureRepeat = TextureRepeat;
            effect.TextureSmoothing = _textureSmoothing;
            effect.MvpMatrix3D = state.MvpMatrix3D;
            effect.Alpha = state.Alpha;
        }

        /// <summary>
        /// Indicates if the current instance can be batched with the given style.
        /// To be overridden by subclasses if default behavior is not sufficient.
        /// The base implementation just checks if the styles are of the same type
        /// and if the textures are compatible.
        /// </summary>
        public virtual bool CanBatchWith(MeshStyle meshStyle)
        {
            if (_type == meshStyle._type)
            {
                Texture newTexture = meshStyle._texture;

                if (_texture == null && newTexture == null) return true;
                if (_texture != null && newTexture != null)
                    return _textureBase == meshStyle._textureBase &&
                           _textureSmoothing == meshStyle._textureSmoothing &&
                           TextureRepeat == meshStyle.TextureRepeat;
                return false;
            }
            return false;
        }

        /// <summary>
        /// Copies the vertex data of the style's current target to the target of another style.
        /// If you pass a matrix, all vertices will be transformed during the process.
        ///
        /// Subclasses may override this method if they need to modify the vertex data in that
        /// process.
        /// </summary>
        /// <param name="targetStyle">Points to the style of a MeshBatch</param>
        /// <param name="targetVertexId">Where to start the copy in the target</param>
        /// <param name="matrix">If you pass a non-null matrix, the 2D position of each vertex 
        /// will be transformed by that matrix before storing it in the target object.</param>
        /// <param name="vertexId">position to start copyting from</param>
        /// <param name="numVertices">Number of vertices to copy</param>
        public void BatchVertexData(MeshStyle targetStyle, int targetVertexId = 0,
                                    Matrix2D matrix = null, int vertexId = 0, int numVertices= -1)
        {
            _vertexData.CopyTo(targetStyle._vertexData, vertexId, targetVertexId, numVertices, matrix);
        }

        /// <summary>
        /// Copies the index data of the style's current target to the target of another style.
        /// The given offset value will be added to all indices during the process.
        ///
        /// <para>This method is used when batching meshes together for rendering. The parameter
        /// <code>targetStyle</code> will point to the style of a <code>MeshBatch</code> (a
        /// subclass of <code>Mesh</code>). Subclasses may override this method if they need
        /// to modify the index data in that process.</para>
        /// </summary>
        public virtual void BatchIndexData(MeshStyle targetStyle, int targetIndexId = 0, int offset = 0,
                                   int indexId = 0, int numIndices = -1)
        {
            _indexData.CopyTo(targetStyle._indexData, targetIndexId, offset, indexId, numIndices);
        }

        /// <summary>
        /// Call this method if the target needs to be redrawn. The call is simply forwarded to the mesh.
        /// </summary>
        protected void SetRequiresRedraw()
        {
            _target?.SetRequiresRedraw();
        }

        /// <summary>
        /// Called when assigning a target mesh. Override to plug in class-specific logic.
        /// </summary>
        protected virtual void OnTargetAssigned(Mesh target) { }

        // enter frame event

        private void OnEnterFrame(DisplayObject target, float passedTime)
        {
            EnterFrame?.Invoke(target, passedTime);
        }
        
        // internal methods

        internal void SetTarget(Mesh target, VertexData vertexData = null, IndexData targetIndexData = null)
        {
            if (_target != target)
            {
                if (_target != null) _target.EnterFrame -= OnEnterFrame;

                _target = target;
                _vertexData = vertexData;
                _indexData = targetIndexData;

                if (target != null)
                {
                    if (EnterFrame != null)
                    {
                        target.EnterFrame += OnEnterFrame;
                    }
                    OnTargetAssigned(target);
                }
            }
        }

        // vertex manipulation

        /// <summary>
        /// The position of the vertex at the specified index, in the mesh's local coordinate
        /// system.
        ///
        /// <para>Only modify the position of a vertex if you know exactly what you're doing, as
        /// some classes might not work correctly when their vertices are moved. E.g. the
        /// <code>Quad</code> class expects its vertices to spawn up a perfectly rectangular
        /// area; some of its optimized methods won't work correctly if that premise is no longer
        /// fulfilled or the original bounds change.</para>
        /// </summary>
        public Point GetVertexPosition(int vertexId)
        {
            return _vertexData.GetPoint(vertexId);
        }

        public void SetVertexPosition(int vertexId, float x, float y)
        {
            _vertexData.SetPoint(vertexId, x, y);
            SetRequiresRedraw();
        }

        /// <summary>
        /// Returns the alpha value of the vertex at the specified index.
        /// </summary>
        public float GetVertexAlpha(int vertexId)
        {
            return _vertexData.GetAlpha(vertexId);
        }

        /// <summary>
        /// Sets the alpha value of the vertex at the specified index to a certain value.
        /// </summary>
        public void SetVertexAlpha(int vertexId, float alpha)
        {
            _vertexData.SetAlpha(vertexId, alpha);
            SetRequiresRedraw();
        }

        /// <summary>
        /// Returns the RGB color of the vertex at the specified index.
        /// </summary>
        public uint GetVertexColor(int vertexId)
        {
            return _vertexData.GetColor(vertexId);
        }

        /// <summary>
        /// Sets the RGB color of the vertex at the specified index to a certain value.
        /// </summary>
        public void SetVertexColor(int vertexId, uint color)
        {
            _vertexData.SetColor(vertexId, color);
            SetRequiresRedraw();
        }

        /// <summary>
        /// Returns the texture coordinates of the vertex at the specified index.
        /// </summary>
        public Point GetTexCoords(int vertexId)
        {
            if (_texture != null) return _texture.GetTexCoords(_vertexData, vertexId);
            return _vertexData.GetTexCoords(vertexId);
        }

        /// <summary>
        /// Sets the texture coordinates of the vertex at the specified index to the given values.
        /// </summary>
        public void SetTexCoords(int vertexId, float u, float v)
        {
            if (_texture != null) _texture.SetTexCoords(_vertexData, vertexId, u, v);
            else _vertexData.SetTexCoords(vertexId, u, v);

            SetRequiresRedraw();
        }

        // properties

        /// <summary>
        ///  Returns a reference to the vertex data of the assigned target (or <code>null</code>
        /// if there is no target). Beware: the style itself does not own any vertices;
        /// it is limited to manipulating those of the target mesh.
        /// </summary>
        protected VertexData VertexData { get {return _vertexData; } }

        /// <summary>
        /// Returns a reference to the index data of the assigned target (or <code>null</code>
        /// if there is no target). Beware: the style itself does not own any indices;
        /// it is limited to manipulating those of the target mesh.
        /// </summary>
        protected IndexData indexData  { get { return _indexData; } }

        /// <summary>
        /// The actual class of this style.
        /// </summary>
        public Type Type { get { return _type; } }

        /// <summary>
        /// Changes the color of all vertices to the same value.
        /// The getter simply returns the color of the first vertex.
        /// </summary>
        public uint Color
        {
            get
            {
                if (_vertexData.NumVertices > 0) return _vertexData.GetColor(0);
                return 0x0;
            }
            set
            {
                int i;
                int numVertices = _vertexData.NumVertices;

                for (i = 0; i < numVertices; ++i)
                {
                    _vertexData.SetColor(i, value);
                }
                SetRequiresRedraw();
            }
            
        }

        /// <summary>
        /// The texture that is mapped to the mesh (or <code>null</code>, if there is none).
        /// </summary>
        public Texture Texture
        {
            get { return _texture; }
            set
            {
                if (value != _texture)
                {
                    if (value != null)
                    {
                        int i;
                        int numVertices = _vertexData != null ? _vertexData.NumVertices : 0;

                        for (i = 0; i < numVertices; ++i)
                        {
                            Point sPoint = GetTexCoords(i);
                            value.SetTexCoords(_vertexData, i, sPoint.X, sPoint.Y);
                        }
                    }

                    _texture = value;
                    _textureBase = value != null ? value.Base : 0;
                    SetRequiresRedraw();
                }
            }
        }

        /// <summary>
        /// The smoothing filter that is used for the texture. @default bilinear
        /// </summary>
        public TextureSmoothing TextureSmoothing
        {
            get
            {
                return _textureSmoothing;
            }
            set
            {
                if (value != _textureSmoothing)
                {
                    _textureSmoothing = value;
                    SetRequiresRedraw();
                }
            }
        }

        /// <summary>
        /// The target the style is currently assigned to.
        /// </summary>
        public Mesh Target
        {
            get { return _target; }
        }

    }
}
