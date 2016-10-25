using System;
using Sparrow.Core;
using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;
using Sparrow.Rendering;

namespace Sparrow.Filters
{
    /** The FragmentFilter class is the base class for all filter effects in Starling.
        *  All filters must extend this class. You can attach them to any display object through the
        *  <code>filter</code> property.
        *
        *  <p>A fragment filter works in the following way:</p>
        *  <ol>
        *    <li>The object to be filtered is rendered into a texture.</li>
        *    <li>That texture is passed to the <code>process</code> method.</li>
        *    <li>This method processes the texture using a <code>FilterEffect</code> subclass
        *        that processes the input via fragment and vertex shaders to achieve a certain
        *        effect.</li>
        *    <li>If the filter requires several passes, the process method may execute the
        *        effect several times, or even make use of other filters in the process.</li>
        *    <li>In the end, a quad with the output texture is added to the batch renderer.
        *        In the next frame, if the object hasn't changed, the filter is drawn directly
        *        from the render cache.</li>
        *    <li>Alternatively, the last pass may be drawn directly to the back buffer. That saves
        *        one draw call, but means that the object may not be drawn from the render cache in
        *        the next frame. Starling makes an educated guess if that makes sense, but you can
        *        also force it to do so via the <code>alwaysDrawToBackBuffer</code> property.</li>
        *  </ol>
        *
        *  <p>All of this is set up by the basic FragmentFilter class. Concrete subclasses
        *  just need to override the protected method <code>createEffect</code> and (optionally)
        *  <code>process</code>. Multi-pass filters must also override <code>numPasses</code>.</p>
        *
        *  <p>Typically, any properties on the filter are just forwarded to an effect instance,
        *  which is then used automatically by <code>process</code> to render the filter pass.
        *  For a simple example on how to write a single-pass filter, look at the implementation of
        *  the <code>ColorMatrixFilter</code>; for a composite filter (i.e. a filter that combines
        *  several others), look at the <code>GlowFilter</code>.
        *  </p>
        *
        *  <p>Beware that a filter instance may only be used on one object at a time!</p>
        *
        *  <p><strong>Animated filters</strong></p>
        *
        *  <p>The <code>process</code> method of a filter is only called when it's necessary, i.e.
        *  when the filter properties or the target display object changes. This means that you cannot
        *  rely on the method to be called on a regular basis, as needed when creating an animated
        *  filter class. Instead, you can do so by listening for an <code>ENTER_FRAME</code>-event.
        *  It is dispatched on the filter once every frame, as long as the filter is assigned to
        *  a display object that is connected to the stage.</p>
        *
        *  <p><strong>Caching</strong></p>
        *
        *  <p>Per default, whenever the target display object is changed in any way (i.e. the render
        *  cache fails), the filter is reprocessed. However, you can manually cache the filter output
        *  via the method of the same name: this will let the filter redraw the current output texture,
        *  even if the target object changes later on. That's especially useful if you add a filter
        *  to an object that changes only rarely, e.g. a TextField or an Image. Keep in mind, though,
        *  that you have to call <code>cache()</code> again in order for any changes to show up.</p>
        *
        *  @see starling.rendering.FilterEffect
        */
    public abstract class FragmentFilter
    {
        public event DisplayObject.EnterFrameEventHandler EnterFrame;

        public delegate void OnChanged();
        public OnChanged OnChangedEvent;

        private FilterQuad _quad;
        private DisplayObject _target;
        private FilterEffect _effect;
        private VertexData _vertexData;
        private IndexData _indexData;
        private Padding _padding;
        private FilterHelper _helper;
        private float _resolution;
        private TextureFormat _textureFormat;//was BGRA
        private TextureSmoothing _textureSmoothing;
        private bool _alwaysDrawToBackBuffer;
        private bool _cacheRequested;
        private bool _cached;

        // helpers
        private static Matrix3D sMatrix3D;

        /** Creates a new instance. The base class' implementation just draws the unmodified
         *  input texture. */
        public FragmentFilter()
        {
            _resolution = 1.0f;
            _textureFormat = TextureFormat.Rgba4444;
            _textureSmoothing = TextureSmoothing.Bilinear;

            SetRequiresRedraw();
        }

        /** Disposes all resources that have been created by the filter. */
        public void Dispose()
        {

            if (_helper != null) _helper.Dispose();
            if (_effect != null) _effect.Dispose();
            if (_quad != null)   _quad.Dispose();

            _effect = null;
            _quad = null;
        }

        /** Renders the filtered target object. Most users will never have to call this manually;
         *  it's executed automatically in the rendering process of the filtered display object.
         */
        public void Render(Painter painter)
        {
            if (_target == null)
            {
                throw new InvalidOperationException("Cannot render filter without target");
            }

            if (!_cached || _cacheRequested)
            {
                RenderPasses(painter, _cacheRequested);
                _cacheRequested = false;
            }
            else if (_quad.Visible)
            {
                _quad.Render(painter);
            }
        }

        private void RenderPasses(Painter painter, bool forCache)
        {
            if (_helper  == null) _helper = new FilterHelper(_textureFormat);
            if (_quad  == null) _quad  = new FilterQuad(_textureSmoothing);
            else { _helper.PutTexture(_quad.Texture); _quad.Texture = null; }

            Rectangle bounds = null;
            bool drawLastPassToBackBuffer = false;
            float origResolution = _resolution;
            DisplayObject renderSpace = _target.Stage != null ? _target.Stage : _target.Parent;
            bool isOnStage = renderSpace is Stage;
            Stage stage = SparrowSharp.Stage;
            Rectangle stageBounds;

            if (!forCache && (_alwaysDrawToBackBuffer || _target.RequiresRedraw))
            {
                // If 'requiresRedraw' is true, the object is non-static, and we guess that this
                // will be the same in the next frame. So we render directly to the back buffer.
                //
                // -- That, however, is only possible for full alpha values, because
                // (1) 'FilterEffect' can't handle alpha (and that will do the rendering)
                // (2) we don't want lower layers (CompositeFilter!) to shine through.

                drawLastPassToBackBuffer = painter.State.Alpha == 1.0f;
                painter.ExcludeFromCache(_target);
            }

            if (_target == SparrowSharp.Root)
            {
                // full-screen filters use exactly the stage bounds
                bounds = stage.GetStageBounds(_target);
            }
            else
            {
                // Unfortunately, the following bounds calculation yields the wrong result when
                // drawing a filter to a RenderTexture using a custom matrix. The 'modelviewMatrix'
                // should be used for the bounds calculation, but the API doesn't support this.
                // A future version should change this to: "getBounds(modelviewMatrix, bounds)"

                bounds = _target.GetBounds(renderSpace);

                if (!forCache && isOnStage) // normally, we don't need anything outside
                {
                    stageBounds = stage.GetStageBounds(null);
                    bounds = bounds.Intersection(stageBounds);
                }
            }

            _quad.Visible = !bounds.IsEmpty();
            if (!_quad.Visible) { return; }

            if (_padding != null) bounds.Extend(_padding.Left, _padding.Right, _padding.Top, _padding.Bottom);

            // integer bounds for maximum sharpness + to avoid jiggling
            bounds.SetTo((float)Math.Floor(bounds.X), (float)Math.Floor(bounds.Y),
                         (float)Math.Ceiling(bounds.Width), (float)Math.Ceiling(bounds.Height));

            _helper.TextureScale = SparrowSharp.ContentScaleFactor * _resolution;
            _helper.ProjectionMatrix3D = painter.State.ProjectionMatrix3D;
            _helper.RenderTarget = painter.State.RenderTarget;
            _helper.TargetBounds = bounds;
            _helper.Target = _target;
            _helper.Start(NumPasses, drawLastPassToBackBuffer);

            _quad.SetBounds(bounds);
            _resolution = 1.0f; // applied via '_helper.textureScale' already;
                                // only 'child'-filters use resolution directly (in 'process')

            bool wasCacheEnabled = painter.CacheEnabled;
            Texture input = _helper.GetTexture();
            Texture output = null;

            painter.CacheEnabled = false; // -> what follows should not be cached
            painter.PushState();
            painter.State.Alpha = 1.0f;
            painter.State.RenderTarget = input;
            painter.State.SetProjectionMatrix(bounds.X, bounds.Y,
                input.Root.Width, input.Root.Height,
                stage.StageWidth, stage.StageHeight, stage.CameraPosition);

            // OpenGL renders into textures with Y coordinates flipped :(
            painter.State.ModelviewMatrix.Scale(1, -1);
            painter.State.ModelviewMatrix.Translate(0, input.Root.Height + 2*bounds.Y);

            _target.Render(painter); // -> draw target object into 'input'
            
            painter.FinishMeshBatch();
            painter.State.SetModelviewMatricesToIdentity();
            painter.State.ClipRect = null;

            output = Process(painter, _helper, input); // -> feed 'input' to actual filter code

            painter.PopState();
            painter.CacheEnabled = wasCacheEnabled; // -> cache again

            if (output != null) // indirect rendering
            {
                painter.PushState();
                
                _quad.MoveVertices(renderSpace, _target);       // -> local coords

                _quad.Texture = output;
                _quad.Render(painter); // renders to the screen

                painter.FinishMeshBatch();
                painter.PopState();
            }

            _helper.Target = null;
            _helper.PutTexture(input);
            _resolution = origResolution;
        }

        /** Does the actual filter processing. This method will be called with up to four input
         *  textures and must return a new texture (acquired from the <code>helper</code>) that
         *  contains the filtered output. To to do this, it configures the FilterEffect
         *  (provided via <code>createEffect</code>) and calls its <code>render</code> method.
         *
         *  <p>In a standard filter, only <code>input0</code> will contain a texture; that's the
         *  object the filter was applied to, rendered into an appropriately sized texture.
         *  However, filters may also accept multiple textures; that's useful when you need to
         *  combine the output of several filters into one. For example, the DropShadowFilter
         *  uses a BlurFilter to create the shadow and then feeds both input and shadow texture
         *  into a CompositeFilter.</p>
         *
         *  <p>Never create or dispose any textures manually within this method; instead, get
         *  new textures from the provided helper object, and pass them to the helper when you do
         *  not need them any longer. Ownership of both input textures and returned texture
         *  lies at the caller; only temporary textures should be put into the helper.</p>
         */
        virtual public Texture Process(Painter painter, IFilterHelper helper,
                                       Texture input0 = null, Texture input1 = null,
                                       Texture input2 = null, Texture input3 = null)
        {
            FilterEffect effect = Effect;
            Texture output = helper.GetTexture(_resolution);
            Matrix3D projectionMatrix;
            Rectangle bounds = null;
            Texture renderTarget;

            if (output != null) // render to texture
            {
                renderTarget = output;
                projectionMatrix = MatrixUtil.CreatePerspectiveProjectionMatrix(0, 0,
                    output.Root.Width / _resolution, output.Root.Height / _resolution,
                    0, 0, null);
                // OpenGL renders into textures with Y coordinates flipped :(
                projectionMatrix.Flip(output.Height);
            }
            else // render to back buffer
            {
                bounds = helper.TargetBounds;
                renderTarget = (helper as FilterHelper).RenderTarget;
                projectionMatrix = (helper as FilterHelper).ProjectionMatrix3D;
                effect.TextureSmoothing = _textureSmoothing;
            }

            painter.State.RenderTarget = renderTarget;
            painter.PrepareToDraw();
            painter.DrawCount += 1;

            input0.SetupVertexPositions(VertexData, 0, bounds);
            input0.SetupTextureCoordinates(VertexData);

            effect.Texture = input0;
            effect.MvpMatrix3D = projectionMatrix;
            effect.UploadVertexData(VertexData);
            effect.UploadIndexData(IndexData);
            effect.Render(0, IndexData.NumTriangles);

            return output;
        }

        /** Creates the effect that does the actual, low-level rendering.
         *  Must be overridden by all subclasses that do any rendering on their own (instead
         *  of just forwarding processing to other filters).
         */
        virtual protected FilterEffect CreateEffect()
        {
            return new FilterEffect();
        }

        /** Caches the filter output into a texture.
        *
        *  <p>An uncached filter is rendered every frame (except if it can be rendered from the
        *  global render cache, which happens if the target object does not change its appearance
        *  or location relative to the stage). A cached filter is only rendered once; the output
        *  stays unchanged until you call <code>cache</code> again or change the filter settings.
        *  </p>
        *
        *  <p>Beware: you cannot cache filters on 3D objects; if the object the filter is attached
        *  to is a Sprite3D or has a Sprite3D as (grand-) parent, the request will be silently
        *  ignored. However, you <em>can</em> cache a 2D object that has 3D children!</p>
        */
        public void Cache()
        {
            _cached = _cacheRequested = true;
            SetRequiresRedraw();
        }

        /** Clears the cached output of the filter. After calling this method, the filter will be
        *  processed once per frame again. */
        public void ClearCache()
        {
            _cached = _cacheRequested = false;
            SetRequiresRedraw();
        }

        // enter frame event
        private void OnEnterFrame(DisplayObject target, float passedTime)
        {
            EnterFrame?.Invoke(target, passedTime);
        }
        
        // + here was some code so users can add enterFrame events to this one
        
        // properties

        /** The effect instance returning the FilterEffect created via <code>createEffect</code>. */
        protected FilterEffect Effect
        {
            get
            {
                if (_effect == null) _effect = CreateEffect();
                return _effect;
            }
           
        }

        /** The VertexData used to process the effect. Per default, uses the format provided
         *  by the effect, and contains four vertices enclosing the target object. */
        protected VertexData VertexData
        {
            get
            {
                if (_vertexData == null) _vertexData = new VertexData(4);
                return _vertexData;
            }
           
        }

        /** The IndexData used to process the effect. Per default, references a quad (two triangles)
         *  of four vertices. */
        protected IndexData IndexData
        {
            get
            {
                if (_indexData == null)
                {
                    _indexData = new IndexData();
                    _indexData.AddQuad(0, 1, 2, 3);
                }

                return _indexData;
            }
        }

        /** Call this method when any of the filter's properties changes.
         *  This will make sure the filter is redrawn in the next frame. */
        virtual protected void SetRequiresRedraw()
        {
            OnChangedEvent?.Invoke();
            if (_target != null) _target.SetRequiresRedraw();
            if (_cached) _cacheRequested = true;
        }

        /** Indicates the number of rendering passes required for this filter.
         *  Subclasses must override this method if the number of passes is not <code>1</code>. */
        virtual public int NumPasses
        {
            get { return 1; }
        }

        /** Called when assigning a target display object.
         *  Override to plug in class-specific logic. */
        virtual protected void OnTargetAssigned(DisplayObject target) { }

        /** Padding can extend the size of the filter texture in all directions.
        *  That's useful when the filter "grows" the bounds of the object in any direction. */
        public Padding Padding
        {
            get
            {
                if (_padding == null)
                {
                    _padding = new Padding();
                    _padding.OnChangedEvent += SetRequiresRedraw;
                }
                return _padding;
            }
            set
            {
                Padding.CopyFrom(value);
            }
            
        }

        /** Indicates if the filter is cached (via the <code>cache</code> method). */
        public bool IsCached { get { return _cached; } }

        /** The resolution of the filter texture. "1" means stage resolution, "0.5" half the stage
         *  resolution. A lower resolution saves memory and execution time, but results in a lower
         *  output quality. Values greater than 1 are allowed; such values might make sense for a
         *  cached filter when it is scaled up. @default 1
         */
        virtual public float Resolution {
            get { return _resolution; }
            set
            {
                if (value != _resolution)
                {
                    if (value > 0) _resolution = value;
                    else throw new ArgumentException("resolution must be > 0");
                    SetRequiresRedraw();
                }
            }
        }

        /** The smoothing mode of the filter texture. @default bilinear */
        public TextureSmoothing TextureSmoothing { 
            get { return _textureSmoothing; }
            set
            {
                if (value != _textureSmoothing)
                {
                    _textureSmoothing = value;
                    if (_quad != null) _quad.TextureSmoothing = value;
                    SetRequiresRedraw();
                }
            }
        }

        /** The format of the filter texture. @default BGRA */
        public TextureFormat TextureFormat { 
            get { return _textureFormat; }
            set
            {
                if (value != _textureFormat)
                {
                    _textureFormat = value;
                    if (_helper != null) _helper.TextureFormat = value;
                    SetRequiresRedraw();
                }
            }
        }

        /** Indicates if the last filter pass is always drawn directly to the back buffer.
         *
         *  <p>Per default, the filter tries to automatically render in a smart way: objects that
         *  are currently moving are rendered to the back buffer, objects that are static are
         *  rendered into a texture first, which allows the filter to be drawn directly from the
         *  render cache in the next frame (in case the object remains static).</p>
         *
         *  <p>However, this fails when filters are added to an object that does not support the
         *  render cache, or to a container with such a child (e.g. a Sprite3D object or a masked
         *  display object). In such a case, enable this property for maximum performance.</p>
         *
         *  @default false
         */
        public bool AlwaysDrawToBackBuffer {
            get { return _alwaysDrawToBackBuffer; }
            set { _alwaysDrawToBackBuffer = value; }

        }

        // internal methods

        /** @private */
        internal void SetTarget(DisplayObject target)
        {
            if (target != _target)
            {
                DisplayObject prevTarget = _target;
                _target = target;

                if (target == null)
                {
                    if (_helper != null) _helper.Purge();
                    if (_effect != null) _effect.PurgeBuffers();
                    if (_quad != null)   _quad.DisposeTexture();
                }

                if (prevTarget != null)
                {
                    prevTarget.Filter = null;
                    prevTarget.EnterFrame -= OnEnterFrame;
                }

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

}

internal class FilterQuad : Mesh
    {
        private static Matrix2D sMatrix = Matrix2D.Create();

        public FilterQuad(TextureSmoothing smoothing) : base(new VertexData(4), new IndexData())
        {
            IndexData.AddQuad(0, 1, 2, 3);
            VertexData.Colorize(0xFFFFFF, 1.0f);

            TextureSmoothing = smoothing;
            PixelSnapping = false;
        }

        override public void Dispose()
        {
            DisposeTexture();
            base.Dispose();
        }

        public void DisposeTexture()
        {
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }

        public void MoveVertices(DisplayObject sourceSpace, DisplayObject targetSpace)
        {
            if (sourceSpace != targetSpace)
            {
                sMatrix = targetSpace.GetTransformationMatrix(sourceSpace).Invert(); // ss could be null!
                VertexData.TransformVertices(sMatrix, 0, VertexData.NumVertices);
            }
        }

        public void SetBounds(Rectangle bounds)
        {
            VertexData vertexData = VertexData;

            vertexData.SetPoint(0, bounds.X, bounds.Y);
            vertexData.SetPoint(1, bounds.Right, bounds.Y);
            vertexData.SetPoint(2, bounds.X, bounds.Bottom);
            vertexData.SetPoint(3, bounds.Right, bounds.Bottom);
        }

        override public Texture Texture
        {
            set {
                base.Texture = value;
                if (value != null) value.SetupTextureCoordinates(VertexData);
            }
           
        }
}
}


