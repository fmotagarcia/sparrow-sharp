using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Utils;
using System;
using SparrowSharp.Core.Styles;
using SparrowSharp.Core.Utils;
using Sparrow.Textures;

namespace SparrowSharp.Core.Rendering
{

    /// <summary>
    ///  The base class for all tangible (non-container) display objects, spawned up by a number
    ///  of triangles.
    ///
    ///  <p>Since Starling uses Stage3D for rendering, all rendered objects must be constructed
    ///  from triangles. A mesh stores the information of its triangles through VertexData and
    ///  IndexData structures. The default format stores position, color and texture coordinates
    ///  for each vertex.</p>
    ///
    ///  <p>How a mesh is rendered depends on its style. Per default, this is an instance
    ///  of the <code>MeshStyle</code> base class; however, subclasses may extend its behavior
    ///  to add support for color transformations, normal mapping, etc.</p>
    ///
    ///  @see MeshBatch
    ///  @see starling.styles.MeshStyle
    ///  @see starling.rendering.VertexData
    ///  @see starling.rendering.IndexData
    /// </summary>
    public class Mesh : DisplayObject
    {
        internal MeshStyle _style;
        internal VertexData _vertexData;
        internal IndexData _indexData;
        internal bool _pixelSnapping;

        private static Type sDefaultStyle = typeof(MeshStyle);

        public delegate MeshStyle DefaultStyleFactoryFunction();
        private static DefaultStyleFactoryFunction sDefaultStyleFactory;

        /// <summary> Creates a new mesh with the given vertices and indices.
        ///  If you don't pass a style, an instance of <code>MeshStyle</code> will be created
        ///  for you. Note that the format of the vertex data will be matched to the
        ///  given style right away.
        /// </summary>
        public Mesh(VertexData vertexData, IndexData indexData, MeshStyle style = null)
        {
            if (vertexData == null) throw new ArgumentException("VertexData must not be null");
            if (indexData == null) throw new ArgumentException("IndexData must not be null");

            _vertexData = vertexData;
            _indexData = indexData;

            SetStyle(style, false);
        }
        
        override public void Dispose()
        {
            _vertexData.Clear();
            _indexData.Clear();
            base.Dispose();
        }
        
        override public DisplayObject HitTest(Point localPoint)
        {
            if (!Visible || !Touchable || !HitTestMask(localPoint)) return null;
            else return MeshUtil.ContainsPoint(_vertexData, _indexData, localPoint) ? this : null;
        }
        
        override public Rectangle GetBounds(DisplayObject targetSpace)
        {
            return MeshUtil.CalculateBounds(_vertexData, this, targetSpace);
        }

        override public void Render(Painter painter)
        {
            if (_pixelSnapping)
            {
                MatrixUtil.SnapToPixels(painter.State.ModelviewMatrix, painter.PixelSize);
            }
            painter.BatchMesh(this);
        }

        /** Sets the style that is used to render the mesh. Styles (which are always subclasses of
         *  <code>MeshStyle</code>) provide a means to completely modify the way a mesh is rendered.
         *  For example, they may add support for color transformations or normal mapping.
         *
         *  <p>When assigning a new style, the vertex format will be changed to fit it.
         *  Do not use the same style instance on multiple objects! Instead, make use of
         *  <code>style.clone()</code> to assign an identical style to multiple meshes.</p>
         *
         *  @param meshStyle             the style to assign. If <code>null</code>, the default
         *                               style will be created.
         *  @param mergeWithPredecessor  if enabled, all attributes of the previous style will be
         *                               be copied to the new one, if possible.
         *  @see #defaultStyle
         *  @see #defaultStyleFactory
         */
        virtual public void SetStyle(MeshStyle meshStyle = null, bool mergeWithPredecessor= true)
        {
            if (meshStyle == null) meshStyle = CreateDefaultMeshStyle();
            else if (meshStyle == _style) return;
            else if (meshStyle.Target != null) meshStyle.Target.SetStyle();

            if (_style != null)
            {
                if (mergeWithPredecessor) meshStyle.CopyFrom(_style);
                _style.SetTarget(null);
            }

            _style = meshStyle;
            _style.SetTarget(this, _vertexData, _indexData);
        }

        private MeshStyle CreateDefaultMeshStyle()
        {
            MeshStyle meshStyle = null;
            if (sDefaultStyleFactory != null)
            {
                meshStyle = sDefaultStyleFactory();
            }
            if (meshStyle == null)
            {
                meshStyle = (MeshStyle)Activator.CreateInstance(sDefaultStyle);
            }
            return meshStyle;
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
            return _style.GetVertexPosition(vertexID);
        }

        public void SetVertexPosition(int vertexID, float x, float y)
        {
            _style.SetVertexPosition(vertexID, x, y);
        }

        /** Returns the alpha value of the vertex at the specified index. */
        public float GetVertexAlpha(int vertexID)
        {
            return _style.GetVertexAlpha(vertexID);
        }

        /** Sets the alpha value of the vertex at the specified index to a certain value. */
        public void SetVertexAlpha(int vertexID, float alpha)
        {
            _style.SetVertexAlpha(vertexID, alpha);
        }

        /** Returns the RGB color of the vertex at the specified index. */
        public uint GetVertexColor(int vertexID)
        {
            return _style.GetVertexColor(vertexID);
        }

        /** Sets the RGB color of the vertex at the specified index to a certain value. */
        public void SetVertexColor(int vertexID, uint color)
        {
            _style.SetVertexColor(vertexID, color);
        }

        /** Returns the texture coordinates of the vertex at the specified index. */
        public Point GetTexCoords(int vertexID)
        {
            return _style.GetTexCoords(vertexID);
        }

        /** Sets the texture coordinates of the vertex at the specified index to the given values. */
        public void SetTexCoords(int vertexID, float u, float v)
        {
            _style.SetTexCoords(vertexID, u, v);
        }

        // properties

        /** The vertex data describing all vertices of the mesh.
         *  Any change requires a call to <code>setRequiresRedraw</code>. */
        protected VertexData VertexData { get { return _vertexData; } }

        /** The index data describing how the vertices are interconnected.
         *  Any change requires a call to <code>setRequiresRedraw</code>. */
        protected IndexData IndexData { get { return _indexData; } }

        /** The style that is used to render the mesh. Styles (which are always subclasses of
         *  <code>MeshStyle</code>) provide a means to completely modify the way a mesh is rendered.
         *  For example, they may add support for color transformations or normal mapping.
         *
         *  <p>The setter will simply forward the assignee to <code>setStyle(value)</code>.</p>
         *
         *  @default MeshStyle
         */
        public MeshStyle Style { 
            get { return _style; }
            set { SetStyle(value);}
        }

        /** The texture that is mapped to the mesh (or <code>null</code>, if there is none). */
        virtual public Texture Texture { 
            get { return _style.Texture; }
            set { _style.Texture = value; }
        }

        /** Changes the color of all vertices to the same value.
         *  The getter simply returns the color of the first vertex. */
        public uint Color {
            get { return _style.Color; }
            set { _style.Color = value;}
        }

        /** The smoothing filter that is used for the texture.
         *  @default bilinear */
        public TextureSmoothing TextureSmoothing { 
            get { return _style.TextureSmoothing; }
            set { _style.TextureSmoothing = value; }
        }

        /** Indicates if pixels at the edges will be repeated or clamped. Only works for
         *  power-of-two textures; for a solution that works with all kinds of textures,
         *  see <code>Image.tileGrid</code>. @default false */
        public bool TextureRepeat { 
            get { return _style.TextureRepeat; }
            set { _style.TextureRepeat = value; }
        }

        /** Controls whether or not the instance snaps to the nearest pixel. This can prevent the
         *  object from looking blurry when it's not exactly aligned with the pixels of the screen.
         *  @default false */
        public bool PixelSnapping {
            get { return _pixelSnapping; }
            set { _pixelSnapping = value;  }
        }

        /** The total number of vertices in the mesh. */
        virtual public int NumVertices {
            set { }
            get { return _vertexData.NumVertices; }
        }

        /** The total number of indices referencing vertices. */
        virtual public int NumIndices {
            set { }
            get { return _indexData.NumIndices;}
        }

        /** The total number of triangles in this mesh.
         *  (In other words: the number of indices divided by three.) */
        public int NumTriangles { get { return _indexData.NumTriangles; } }

        // static properties

        /** The default style used for meshes if no specific style is provided. The default is
         *  <code>starling.rendering.MeshStyle</code>, and any assigned class must be a subclass
         *  of the same. */
        public static Type DefaultStyle { 
            get { return sDefaultStyle; }
            set { sDefaultStyle = value; }
        }

        /** A factory method that is used to create the 'MeshStyle' for a mesh if no specific
         *  style is provided. That's useful if you are creating a hierarchy of objects, all
         *  of which need to have a certain style. Different to the <code>defaultStyle</code>
         *  property, this method allows plugging in custom logic and passing arguments to the
         *  constructor. Return <code>null</code> to fall back to the default behavior (i.e.
         *  to instantiate <code>defaultStyle</code>). The <code>mesh</code>-parameter is optional
         *  and may be omitted.
         *
         *  <listing>
         *  Mesh.defaultStyleFactory = function(mesh:Mesh):MeshStyle
         *  {
         *      return new ColorizeMeshStyle(Math.random() * 0xffffff);
         *  }</listing>
         */
        public static DefaultStyleFactoryFunction DefaultStyleFactory { 
            get { return sDefaultStyleFactory; }
            set { sDefaultStyleFactory = value; }
        }
}
}