using OpenTK;
using Sparrow.Geom;
using Sparrow.Textures;
using SparrowSharp.Core.Geom;
using SparrowSharp.Core.Utils;

namespace Sparrow.Core
{
    /** The RenderState stores a combination of settings that are currently used for rendering.
     *  This includes modelview and transformation matrices as well as context3D related settings.
     *
     *  <p>Starling's Painter instance stores a reference to the current RenderState.
     *  Via a stack mechanism, you can always save a specific state and restore it later.
     *  That makes it easy to write rendering code that doesn't have any side effects.</p>
     *
     *  <p>Beware that any context-related settings are not applied on the context
     *  right away, but only after calling <code>painter.prepareToDraw()</code>.
     *  However, the Painter recognizes changes to those settings and will finish the current
     *  batch right away if necessary.</p>
     *
     *  <strong>Matrix Magic</strong>
     *
     *  <p>On rendering, Starling traverses the display tree, constantly moving from one
     *  coordinate system to the next. Each display object stores its vertex coordinates
     *  in its local coordinate system; on rendering, they must be moved to a global,
     *  2D coordinate space (the so-called "clip-space"). To handle these calculations,
     *  the RenderState contains a set of matrices.</p>
     *
     *  <p>By multiplying vertex coordinates with the <code>modelviewMatrix</code>, you'll get the
     *  coordinates in "screen-space", or in other words: in stage coordinates. (Optionally,
     *  there's also a 3D version of this matrix. It comes into play when you're working with
     *  <code>Sprite3D</code> containers.)</p>
     *
     *  <p>By feeding the result of the previous transformation into the
     *  <code>projectionMatrix</code>, you'll end up with so-called "clipping coordinates",
     *  which are in the range <code>[-1, 1]</code> (just as needed by the graphics pipeline).
     *  If you've got vertices in the 3D space, this matrix will also execute a perspective
     *  projection.</p>
     *
     *  <p>Finally, there's the <code>mvpMatrix</code>, which is short for
     *  "modelviewProjectionMatrix". This is simply a combination of <code>modelview-</code> and
     *  <code>projectionMatrix</code>, combining the effects of both. Pass this matrix
     *  to the vertex shader and all your vertices will automatically end up at the right
     *  position.</p>
     *
     *  @see Painter
     *  @see starling.display.Sprite3D
     */
    public class RenderState
    {

        public float Alpha;
        protected uint _blendMode;
        internal Matrix _modelviewMatrix;

        private Texture _renderTarget;
        private uint _renderTargetOptions;
        private string _culling;
        private Rectangle _clipRect;
        public delegate void OnDrawRequiredFunction();
        public OnDrawRequiredFunction _onDrawRequired;
 
        private Matrix3D _modelviewMatrix3D;
        private Matrix3D _projectionMatrix3D;
        private Matrix3D _mvpMatrix3D;

        public RenderState()
        {
            Reset();
        }

        public void CopyFrom(RenderState renderState)
        {
            if (_onDrawRequired != null)
            {
                int currentTarget = _renderTarget != null ? _renderTarget.Base : 0;
                int nextTarget = renderState._renderTarget != null ? renderState._renderTarget.Base : 0;
                bool clipRectChanges = _clipRect != null || renderState._clipRect != null ?
                    !Rectangle.Compare(_clipRect, renderState._clipRect) : false;

                if (_blendMode != renderState._blendMode || _culling != renderState._culling ||
                    currentTarget != nextTarget || clipRectChanges)
                {
                    _onDrawRequired();
                }
            }

            Alpha = renderState.Alpha;
            _blendMode = renderState._blendMode;
            _renderTarget = renderState._renderTarget;
            _renderTargetOptions = renderState._renderTargetOptions;
            _culling = renderState._culling;
            _modelviewMatrix.CopyFromMatrix(renderState._modelviewMatrix);
            
            _projectionMatrix3D.CopyFrom(renderState._projectionMatrix3D);

            if (_modelviewMatrix3D != null || renderState._modelviewMatrix3D != null)
                this.ModelviewMatrix3D = renderState._modelviewMatrix3D;

            if (_clipRect != null || renderState._clipRect != null)
                ClipRect = renderState._clipRect;
        }

        /** Resets the RenderState to the default settings.
        *  (Check each property documentation for its default value.) */
        public void Reset()
        {
            Alpha = 1.0f;
            BlendMode = Display.BlendMode.NORMAL;
            RenderTarget = null;
            ClipRect = null;

            if (_modelviewMatrix != null) _modelviewMatrix.Identity();
            else _modelviewMatrix = Matrix.Create();

            if (_projectionMatrix3D != null) _projectionMatrix3D.Identity();
            else _projectionMatrix3D = Matrix3D.Create();

            if (_mvpMatrix3D == null) _mvpMatrix3D = Matrix3D.Create();
        }

        /** Prepends the given matrix to the 2D modelview matrix. */
        public void TransformModelviewMatrix(Matrix matrix)
        {
            _modelviewMatrix.PrependMatrix(matrix);
        }

        /** Prepends the given matrix to the 3D modelview matrix.
         *  The current contents of the 2D modelview matrix is stored in the 3D modelview matrix
         *  before doing so; the 2D modelview matrix is then reset to the identity matrix.
         */
        public void TransformModelviewMatrix3D(Matrix3D matrix)
        {
            if (_modelviewMatrix3D == null)
                _modelviewMatrix3D = Matrix3D.Create();

            _modelviewMatrix3D.Prepend(_modelviewMatrix.ConvertToMatrix3D());
            _modelviewMatrix3D.Prepend(matrix);
            _modelviewMatrix.Identity();
        }

        /** Creates a perspective projection matrix suitable for 2D and 3D rendering.
         *
         *  <p>The first 4 parameters define which area of the stage you want to view (the camera
         *  will 'zoom' to exactly this region). The final 3 parameters determine the perspective
         *  in which you're looking at the stage.</p>
         *
         *  <p>The stage is always on the rectangle that is spawned up between x- and y-axis (with
         *  the given size). All objects that are exactly on that rectangle (z equals zero) will be
         *  rendered in their true size, without any distortion.</p>
         *
         *  <p>If you pass only the first 4 parameters, the camera will be set up above the center
         *  of the stage, with a field of view of 1.0 rad.</p>
         */
        public void SetProjectionMatrix(float x, float y, float width, float height,
                                        float stageWidth = 0, float stageHeight = 0,
                                        float[] cameraPos = null)
        {
            _projectionMatrix3D = MatrixUtil.CreatePerspectiveProjectionMatrix(
                            x, y, width, height, stageWidth, stageHeight, cameraPos);
        }
        
        /** Changes the modelview matrices (2D and, if available, 3D) to identity matrices.
         *  An object transformed an identity matrix performs no transformation.
         */
        public void SetModelviewMatricesToIdentity()
        {
            _modelviewMatrix.Identity();
            if (_modelviewMatrix3D != null) _modelviewMatrix3D.Identity();
        }

        public Matrix ModelviewMatrix
        {
            get { return _modelviewMatrix; }
            set { _modelviewMatrix.CopyFromMatrix(value); }
        }

        /** Returns the current 3D modelview matrix, if there have been 3D transformations.
         *  CAUTION: Use with care! Each call returns the same instance.
         *  @default null */
         public Matrix3D ModelviewMatrix3D
        {
            get { return _modelviewMatrix3D; }
            set
            {
                if (value != null)
                {
                    if (_modelviewMatrix3D == null) _modelviewMatrix3D = Matrix3D.Create(value.RawData);
                    else _modelviewMatrix3D.CopyFrom(value);
                }
                else if (_modelviewMatrix3D != null)
                {
                    _modelviewMatrix3D = null;
                }
            }
        }

        /** Returns the current projection matrix. You can use the method 'setProjectionMatrix3D'
         *  to set it up in an intuitive way.
         *  CAUTION: Use with care! Each call returns the same instance.
         *  @default identity matrix */
         public Matrix3D ProjectionMatrix3D
        {
            get { return _projectionMatrix3D; }
            set { _projectionMatrix3D.CopyFrom(value); }
        }

        /** Calculates the product of modelview and projection matrix and stores it in a 3D matrix.
         *  CAUTION: Use with care! Each call returns the same instance. */
        public Matrix3D MvpMatrix3D
        {
            get {
                _mvpMatrix3D.CopyFrom(_projectionMatrix3D);
                if (_modelviewMatrix3D != null) _mvpMatrix3D.Prepend(_modelviewMatrix3D);
                _mvpMatrix3D.Prepend(_modelviewMatrix.ConvertToMatrix3D());
                return _mvpMatrix3D;
            }
        }

        // other methods

        /** Changes the the current render target.
         *
         *  @param target     Either a texture or <code>null</code> to render into the back buffer.
         *  @param enableDepthAndStencil  Indicates if depth and stencil testing will be available.
         *                    This parameter affects only texture targets.
         *  @param antiAlias  The anti-aliasing quality (<code>0</code> meaning: no anti-aliasing).
         *                    This parameter affects only texture targets. Note that at the time
         *                    of this writing, AIR supports anti-aliasing only on Desktop.
         */
        public void SetRenderTarget(Texture target, bool enableDepthAndStencil = true,
                                    int antiAlias = 0)
        {
            int currentTarget = _renderTarget != null? _renderTarget.Base : 0;
            int newTarget = target != null ? target.Base : 0;
            uint depthStencil = enableDepthAndStencil ? 1u : 0;
            uint newOptions = depthStencil | (uint)antiAlias << 4;

            if (currentTarget != newTarget || _renderTargetOptions != newOptions)
            {
                if (_onDrawRequired != null) _onDrawRequired();

                _renderTarget = target;
                _renderTargetOptions = newOptions;
            }
        }

        public uint BlendMode
        {
            get { return _blendMode; }
            set
            {
                if (value != Display.BlendMode.AUTO && _blendMode != value)
                {
                    if (_onDrawRequired != null) _onDrawRequired();
                    _blendMode = value;
                }
            }
        }

        public Texture RenderTarget
        {
            get { return _renderTarget; }
            set { SetRenderTarget(value); }
        }

        /** The clipping rectangle can be used to limit rendering in the current render target to
         *  a certain area. This method expects the rectangle in stage coordinates. To prevent
         *  any clipping, assign <code>null</code>.
         *
         *  @default null
        */
        public Rectangle ClipRect
        {
            get { return _clipRect; }
            set
            {
                if (!Rectangle.Compare(_clipRect, value))
                {
                    if (_onDrawRequired != null) _onDrawRequired();
                    if (value != null)
                    {
                        if (_clipRect == null) _clipRect = new Rectangle();
                        _clipRect.CopyFrom(value);
                    }
                    else if (_clipRect != null)
                    {
                        _clipRect = null;
                    }
                }
            }
        }

        /** The anti-alias setting used when setting the current render target
         *  via <code>setRenderTarget</code>. */
        public uint RenderTargetAntiAlias
        {
            get { return _renderTargetOptions >> 4; }
        }

        public bool is3D
        {
            get { return _modelviewMatrix3D != null; }
        }
}
}

