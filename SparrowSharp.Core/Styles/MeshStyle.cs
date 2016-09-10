using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;
using SparrowSharp.Core.Rendering;
using System;

namespace SparrowSharp.Core.Styles
{
    /** MeshStyles provide a means to completely modify the way a mesh is rendered.
     *  The base class provides Starling's standard mesh rendering functionality: colored and
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
     *  var image:Image = new Image(heroTexture);
     *  var colorStyle:ColorStyle = new ColorStyle();
     *  colorStyle.redOffset = 0.5;
     *  colorStyle.redMultiplier = 2.0;
     *  image.style = colorStyle;</listing>
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
     *  <p>To create custom rendering code in Starling, you need to extend two classes:
     *  <code>MeshStyle</code> and <code>MeshEffect</code>. While the effect class contains
     *  the actual AGAL rendering code, the style provides the API that other developers will
     *  interact with.</p>
     *
     *  <p>Subclasses of <code>MeshStyle</code> will add specific properties that configure the
     *  style's outcome, like the <code>redOffset</code> and <code>redMultiplier</code> properties
     *  in the sample above. Here's how to properly create such a class:</p>
     *
     *  <ul>
     *    <li>Always provide a constructor that can be called without any arguments.</li>
     *    <li>Override <code>copyFrom</code> — that's necessary for batching.</li>
     *    <li>Override <code>createEffect</code> — this method must return the
     *        <code>MeshEffect</code> that will do the actual Stage3D rendering.</li>
     *    <li>Override <code>updateEffect</code> — this configures the effect created above
     *        right before rendering.</li>
     *    <li>Override <code>canBatchWith</code> if necessary — this method figures out if one
     *        instance of the style can be batched with another. If they all can, you can leave
     *        this out.</li>
     *  </ul>
     *
     *  <p>If the style requires a custom vertex format, you must also:</p>
     *
     *  <ul>
     *    <li>add a static constant called <code>VERTEX_FORMAT</code> to the class and</li>
     *    <li>override <code>get vertexFormat</code> and let it return exactly that format.</li>
     *  </ul>
     *
     *  <p>When that's done, you can turn to the implementation of your <code>MeshEffect</code>;
     *  the <code>createEffect</code>-override will return an instance of this class.
     *  Directly before rendering begins, Starling will then call <code>updateEffect</code>
     *  to set it up.</p>
     *
     *  @see MeshEffect
     *  @see VertexDataFormat
     *  @see starling.display.Mesh
     */
    public class MeshStyle
    {

        /** Dispatched every frame on styles assigned to display objects connected to the stage. */
        //[Event(name = "enterFrame", type = "starling.events.EnterFrameEvent")]
        public event DisplayObject.EnterFrameEventHandler EnterFrame;

        private Type _type;
        private Mesh _target;
        private Texture _texture;
        private GLTexture _textureBase;
        private TextureSmoothing _textureSmoothing;
        private bool _textureRepeat;
        private VertexData _vertexData;   // just a reference to the target's vertex data
        private IndexData _indexData;     // just a reference to the target's index data
       
        // helper objects
        private static Point sPoint = Point.Create();

        /** Creates a new MeshStyle instance.
         *  Subclasses must provide a constructor that can be called without any arguments. */
        public MeshStyle()
        {
            _textureSmoothing = TextureSmoothing.Bilinear;
            _type = GetType();
        }

        /** Copies all properties of the given style to the current instance (or a subset, if the
         *  classes don't match). Must be overridden by all subclasses!
         */
        public void CopyFrom(MeshStyle meshStyle)
        {
            _texture = meshStyle._texture;
            _textureBase = meshStyle._textureBase;
            _textureRepeat = meshStyle._textureRepeat;
            _textureSmoothing = meshStyle._textureSmoothing;
        }

        /** Creates a clone of this instance. The method will work for subclasses automatically,
         *  no need to override it. */
        public MeshStyle Clone()
        {
            MeshStyle clone = (MeshStyle)Activator.CreateInstance(_type);
            clone.CopyFrom(this);
            return clone;
        }

        /** Creates the effect that does the actual, low-level rendering.
         *  To be overridden by subclasses!
         */
        public MeshEffect CreateEffect()
        {
            return new MeshEffect();
        }

        /** Updates the settings of the given effect to match the current style.
         *  The given <code>effect</code> will always match the class returned by
         *  <code>createEffect</code>.
         *
         *  <p>To be overridden by subclasses!</p>
         */
        public void UpdateEffect(MeshEffect effect, RenderState state)
        {
            effect.Texture = _texture;
            effect.TextureRepeat = _textureRepeat;
            effect.TextureSmoothing = _textureSmoothing;
            effect.MvpMatrix3D = state.MvpMatrix3D;
            effect.Alpha = state.Alpha;
        }

        /** Indicates if the current instance can be batched with the given style.
         *  To be overridden by subclasses if default behavior is not sufficient.
         *  The base implementation just checks if the styles are of the same type
         *  and if the textures are compatible.
         */
        public bool CanBatchWith(MeshStyle meshStyle)
        {
            if (_type == meshStyle._type)
            {
                Texture newTexture = meshStyle._texture;

                if (_texture == null && newTexture == null) return true;
                else if (_texture != null && newTexture != null)
                    return _textureBase == meshStyle._textureBase &&
                           _textureSmoothing == meshStyle._textureSmoothing &&
                           _textureRepeat == meshStyle._textureRepeat;
                else return false;
            }
            else return false;
        }

        /// <summary>
        ///  Copies the vertex data of the style's current target to the target of another style.
        ///  If you pass a matrix, all vertices will be transformed during the process.
        ///
        ///  Subclasses may override this method if they need to modify the vertex data in that
        ///  process.
        /// </summary>
        /// <param name="targetStyle">Points to the style of a MeshBatch</param>
        /// <param name="targetVertexID">Where to start the copy in the target</param>
        /// <param name="matrix">If you pass a non-null matrix, the 2D position of each vertex 
        /// will be transformed by that matrix before storing it in the target object.</param>
        /// <param name="vertexID">position to start copyting from</param>
        /// <param name="numVertices">Number of vertices to copy</param>
        public void BatchVertexData(MeshStyle targetStyle, int targetVertexID = 0,
                                    Matrix matrix = null, int vertexID = 0, int numVertices= -1)
        {
            _vertexData.CopyTo(targetStyle._vertexData, vertexID, targetVertexID, numVertices, matrix);
        }

        /** Copies the index data of the style's current target to the target of another style.
         *  The given offset value will be added to all indices during the process.
         *
         *  <p>This method is used when batching meshes together for rendering. The parameter
         *  <code>targetStyle</code> will point to the style of a <code>MeshBatch</code> (a
         *  subclass of <code>Mesh</code>). Subclasses may override this method if they need
         *  to modify the index data in that process.</p>
         */
        public void BatchIndexData(MeshStyle targetStyle, int targetIndexID = 0, int offset= 0,
                                   int indexID= 0, int numIndices= -1)
        {
            _indexData.CopyTo(targetStyle._indexData, targetIndexID, offset, indexID, numIndices);
        }

        /** Call this method if the target needs to be redrawn.
         *  The call is simply forwarded to the mesh. */
        protected void SetRequiresRedraw()
        {
            if (_target != null)
            {
                _target.SetRequiresRedraw();
            } 
        }

        /** Called when assigning a target mesh. Override to plug in class-specific logic. */
        virtual protected void OnTargetAssigned(Mesh target)
        { }

        // enter frame event

        private void OnEnterFrame(DisplayObject target, float passedTime)
        {
            EnterFrame(target, passedTime); //dispatchEvent(event);
        }
        // internal methods

        internal void SetTarget(Mesh target, VertexData vertexData = null, IndexData indexData = null)
        {
            if (_target != target)
            {
                //if (_target != null) _target.RemoveEventListener(Event.ENTER_FRAME, OnEnterFrame);
                if (_target != null) _target.EnterFrame -= OnEnterFrame;

                _target = target;
                _vertexData = vertexData;
                _indexData = indexData;

                if (target != null)
                {
                    if (EnterFrame != null) //  if (hasEventListener(Event.ENTER_FRAME))
                    {
                        target.EnterFrame += OnEnterFrame; // addEventListener(Event.ENTER_FRAME)
                    }
                    OnTargetAssigned(target);
                }
            }
        }

        // vertex manipulation

        /** The position of the vertex at the specified index, in the mesh's local coordinate
         *  system.
         *
         *  <p>Only modify the position of a vertex if you know exactly what you're doing, as
         *  some classes might not work correctly when their vertices are moved. E.g. the
         *  <code>Quad</code> class expects its vertices to spawn up a perfectly rectangular
         *  area; some of its optimized methods won't work correctly if that premise is no longer
         *  fulfilled or the original bounds change.</p>
         */
        public Point GetVertexPosition(int vertexID)
        {
            return _vertexData.GetPoint(vertexID);
        }

        public void SetVertexPosition(int vertexID, float x, float y)
        {
            _vertexData.SetPoint(vertexID, x, y);
            SetRequiresRedraw();
        }

        /** Returns the alpha value of the vertex at the specified index. */
        public float GetVertexAlpha(int vertexID)
        {
            return _vertexData.GetAlpha(vertexID);
        }

        /** Sets the alpha value of the vertex at the specified index to a certain value. */
        public void SetVertexAlpha(int vertexID, float alpha)
        {
            _vertexData.SetAlpha(vertexID, alpha);
            SetRequiresRedraw();
        }

        /** Returns the RGB color of the vertex at the specified index. */
        public uint GetVertexColor(int vertexID)
        {
            return _vertexData.GetColor(vertexID);
        }

        /** Sets the RGB color of the vertex at the specified index to a certain value. */
        public void SetVertexColor(int vertexID, uint color)
        {
            _vertexData.SetColor(vertexID, color);
            SetRequiresRedraw();
        }

        /** Returns the texture coordinates of the vertex at the specified index. */
        public Point GetTexCoords(int vertexID)
        {
            /*           if (_texture != null) return _texture.GetTexCoords(_vertexData, vertexID, "texCoords", outPoint);
                       else return _vertexData.GetPoint(vertexID, "texCoords", outPoint);*/
            return null;
        }

        /** Sets the texture coordinates of the vertex at the specified index to the given values. */
        public void SetTexCoords(int vertexID, float u, float v)
        {
/*            if (_texture != null) _texture.SetTexCoords(_vertexData, vertexID, "texCoords", u, v);
            else _vertexData.SetPoint(vertexID, "texCoords", u, v);

            SetRequiresRedraw();*/
        }

        // properties

        /** Returns a reference to the vertex data of the assigned target (or <code>null</code>
         *  if there is no target). Beware: the style itself does not own any vertices;
         *  it is limited to manipulating those of the target mesh. */
        protected VertexData VertexData { get {return _vertexData; } }

        /** Returns a reference to the index data of the assigned target (or <code>null</code>
         *  if there is no target). Beware: the style itself does not own any indices;
         *  it is limited to manipulating those of the target mesh. */
        protected IndexData indexData  { get { return _indexData; } }

        /** The actual class of this style. */
        public Type Type { get { return _type; } }

        /** Changes the color of all vertices to the same value.
         *  The getter simply returns the color of the first vertex. */
        public uint Color
        {
            get
            {
                if (_vertexData.NumVertices > 0) return _vertexData.GetColor(0);
                else return 0x0;
            }
            set
            {
                int i;
                int numVertices = _vertexData.NumVertices;

                for (i = 0; i < numVertices; ++i)
                    _vertexData.SetColor(i, value);

                SetRequiresRedraw();
            }
            
        }

        /** The texture that is mapped to the mesh (or <code>null</code>, if there is none). */
        public Texture Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                /*if (value != _texture)
                {
                    if (value != null)
                    {
                        int i;
                        int numVertices = _vertexData != null ? _vertexData.NumVertices : 0;

                        for (i = 0; i < numVertices; ++i)
                        {
                            GetTexCoords(i, sPoint);
                            value.SetTexCoords(_vertexData, i, "texCoords", sPoint.X, sPoint.Y);
                        }
                    }

                    _texture = value;
                    _textureBase = value != null ? value.Base : null;
                    SetRequiresRedraw();
                }*/
            }
        }

        /** The smoothing filter that is used for the texture. @default bilinear */
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

        /** Indicates if pixels at the edges will be repeated or clamped.
         *  Only works for power-of-two textures. @default false */
        public bool TextureRepeat
        {
            get { return _textureRepeat; }
            set { _textureRepeat = value; }
        }

        /** The target the style is currently assigned to. */
        public Mesh Target
        {
            get
            {
                return _target;
            }
        }

    }
}
