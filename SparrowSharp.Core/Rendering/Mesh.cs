using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Utils;
using SparrowSharp.Core.Desktop.Styles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sparrow.Core;

namespace SparrowSharp.Core.Desktop.Rendering
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

        //private static Class sDefaultStyle = MeshStyle;
        //private static Function sDefaultStyleFactory = null;

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

            //SetStyle(style, false);
        }
        
        public void Dispose()
        {
            //_vertexData.Clear();
            //_indexData.Clear();
            base.Dispose();
        }

        public override void Render(RenderSupport support)
        {
            throw new NotImplementedException();
        }

        public override Rectangle BoundsInSpace(DisplayObject targetSpace)
        {
            throw new NotImplementedException();
        }
    }
}