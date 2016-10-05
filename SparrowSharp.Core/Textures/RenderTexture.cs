using System;
using Sparrow.Display;
using Sparrow.Core;
using Sparrow.Utils;
using Sparrow.Geom;
using Sparrow.Rendering;
using Sparrow.Filters;

namespace Sparrow.Textures
{
    /// <summary>
    // A RenderTexture is a dynamic texture on which you can draw any display object.

    // After creating a render texture, just call the 'DrawObject' method to render an object directly 
    // onto the texture. The object will be drawn onto the texture at its current position, adhering 
    // its current rotation, scale and alpha properties. 

    // Drawing is done very efficiently, as it is happening directly in graphics memory. After you have 
    // drawn objects on the texture, the performance will be just like that of a normal texture - no
    // matter how many objects you have drawn.

    // If you draw lots of objects at once, it is recommended to bundle the drawing calls in a block
    // via the 'DrawBundled' method, like shown below. That will speed it up immensely, allowing
    // you to draw hundreds of objects very quickly.

    // renderTexture.DrawBundled(delegate
    // {
    //     for (int i=0; i<numDrawings; ++i)
    //	   {
    //         image.Rotation = (2 * (float)Math.PI / numDrawings) * i;
    //	       renderTexture.DrawObject(image);            
    //     }             
    // });
    /// </summary>
    public class RenderTexture : SubTexture
    {
        private Texture _activeTexture;
        private bool _drawing;
        private bool _bufferReady;
        private bool _isPersistent;


        /** Creates a new RenderTexture with a certain size(in points). If the texture is
         *  persistent, its contents remains intact after each draw call, allowing you to use the
         *  texture just like a canvas.If it is not, it will be cleared before each draw call.
         *
         *  <p>Non-persistent textures can be used more efficiently on older devices; on modern
         *  hardware, it does not make a difference.For more information, have a look at the
         * documentation of the <code>useDoubleBuffering</code> property.</p>
         */
        public RenderTexture(int width, int height, bool persistent = true,
                             float scale = -1, TextureFormat format = null)
        {
            if (format == null)
            {
                format = TextureFormat.Rgba4444;
            }

            _isPersistent = persistent;
            _activeTexture = Empty(width, height, true, 0, true, scale, format);

            SetTo(_activeTexture, Rectangle.Create(0, 0, width, height), true, null, false);
        }

        public override void Dispose()
        {
            _activeTexture.Dispose();
            base.Dispose();
        }

        /** Draws an object into the texture. Note that any filters on the object will currently
        *  be ignored.
        * 
        *  @param object       The object to draw.
        *  @param matrix       If 'matrix' is null, the object will be drawn adhering its 
        *                      properties for position, scale, and rotation. If it is not null,
        *                      the object will be drawn in the orientation depicted by the matrix.
        *  @param alpha        The object's alpha value will be multiplied with this value.
        *  @param antiAliasing Values range from 0 (no antialiasing) to 4 (best quality).
        *                      Beginning with AIR 22, this feature is supported on all platforms
        *                      (except for software rendering mode).
        */
        public void Draw(DisplayObject obj, Matrix matrix = null, float alpha = 1.0f,
                         int antiAliasing = 0)
        {
            if (obj == null) return;

            if (_drawing)
            {
                Render(obj, matrix, alpha);
            }
            else
            {
                RenderBundled(Render, obj, matrix, alpha, antiAliasing);
            }
        }

        /** Bundles several calls to <code>draw</code> together in a block. This avoids buffer 
         *  switches and allows you to draw multiple objects into a non-persistent texture.
         *  Note that the 'antiAliasing' setting provided here overrides those provided in
         *  individual 'draw' calls.
         *  
         *  @param drawingBlock  a callback 
         *  @param antiAliasing  Values range from 0 (no antialiasing) to 4 (best quality).
         */
        public void DrawBundled(Action drawingBlock, int antiAliasing = 0)
        {
            RenderBundled((x,y,z) => { drawingBlock(); }, null, null, 1.0f, antiAliasing);
        }

        private void Render(DisplayObject obj, Matrix matrix = null, float alpha = 1.0f)
        {
            Painter painter = SparrowSharp.Painter;
            RenderState state = painter.State;
            FragmentFilter filter = obj.Filter;
            DisplayObject mask = obj.Mask;

            // The object might have been rendered already (to the back buffer or another
            // render texture), but not necessarily using the same render state / mvp matrix.
            // Thus, we need to force a redraw.
            obj.SetRequiresRedraw();

            painter.PushState();

            state.Alpha = obj.Alpha * alpha;
            state.SetModelviewMatricesToIdentity();
            state.BlendMode = obj.BlendMode == BlendMode.AUTO ?
                BlendMode.NORMAL : obj.BlendMode;

            if (matrix != null) state.TransformModelviewMatrix(matrix);
            else state.TransformModelviewMatrix(obj.TransformationMatrix);

            // OpenGL calculates y coordinates from the bottom, invert them here
            state.ModelviewMatrix.Scale(1, -1);
            state.ModelviewMatrix.Translate(0, Height);
            

            if (mask != null) painter.DrawMask(mask);

            if (filter != null) filter.Render(painter);
            else obj.Render(painter);

            if (mask != null) painter.EraseMask(mask);

            painter.PopState();
        }

        private void RenderBundled(Action<DisplayObject, Matrix, float> renderBlock, DisplayObject obj = null,
                                          Matrix matrix = null, float alpha = 1.0f,
                                          int antiAliasing = 0)
        {
            Painter painter = SparrowSharp.Painter;
            RenderState state = painter.State;

            painter.PushState();

            Texture rootTexture = _activeTexture.Root;
            state.SetProjectionMatrix(0, 0, rootTexture.Width, rootTexture.Height, Width, Height);

            // limit drawing to relevant area
            Rectangle sClipRect = Rectangle.Create(0, 0, _activeTexture.Width, _activeTexture.Height);

            state.ClipRect = sClipRect;
            state.SetRenderTarget(_activeTexture, true, antiAliasing);
            painter.PrepareToDraw();

            if (!IsPersistent || !_bufferReady)
            {
                painter.Clear();
            }

            _bufferReady = true;
            
            _drawing = true;
            renderBlock(obj, matrix, alpha);
            _drawing = false;

            painter.PopState();
        }

        /** Clears the render texture with a certain color and alpha value. Call without any
         *  arguments to restore full transparency. */
        public void Clear(uint color = 0, float alpha = 0.0f)
        {
            _activeTexture.Root.Clear(color, alpha);
            _bufferReady = true;
        }

        // properties

        /** Indicates if the texture is persistent over multiple draw calls. */
        public bool IsPersistent { get { return _isPersistent; } }

        /** @inheritDoc */
        public override int Base { get { return _activeTexture.Base; } }
        
        /** @inheritDoc */
        public override ConcreteTexture Root { get { return _activeTexture.Root; } }

    /** @inheritDoc */
    /*
    private bool _framebufferIsActive;
    private readonly RenderSupport _renderSupport;

    public RenderTexture(float width, float height, uint argbFillColor = 0x0, float scale = 1.0f)
    {
        int legalWidth = MathUtil.NextPowerOfTwo(width * scale);
        int legalHeight = MathUtil.NextPowerOfTwo(height * scale);

        TextureProperties properties = new TextureProperties
        {
            TextureFormat = Sparrow.Textures.TextureFormat.Rgba8888,
            Scale = scale,
            Width = legalWidth,
            Height = legalHeight,
            NumMipmaps = 0,
            PremultipliedAlpha = true
        };

        Rectangle region = new Rectangle(0, 0, width, height);
        GLTexture glTexture = new GLTexture(IntPtr.Zero, properties);

        Init(glTexture, region);
        _renderSupport = new RenderSupport();
        Clear(argbFillColor, ColorUtil.GetA(argbFillColor) / 255.0f);
    }

    /// <summary>
    /// Draws an object onto the texture, adhering its properties for position, scale, rotation and alpha.
    /// </summary>
    public void DrawObject(DisplayObject obj)
    {
        DrawBundled(delegate
        {
            _renderSupport.PushState(obj.TransformationMatrix, obj.Alpha, obj.BlendMode);
            obj.Render(_renderSupport);
            _renderSupport.PopState();
        });
    }

    /// <summary>
    /// Clears the texture with a certain color and alpha value.
    /// </summary>
    public void Clear(uint color, float alpha)
    {
        DrawBundled(delegate
        {
            _renderSupport.Clear(color, alpha);
        });
    }

    /// <summary>
    /// Bundles several calls to 'drawObject' together in a block. This avoids framebuffer switches.
    /// </summary>
    public void DrawBundled(Action block)
    {
        if (block == null)
        {
            return;
        }

        // the block may call a draw-method again, so we're making sure that the frame buffer switching
        // happens only in the outermost block.
        bool isDrawing = _framebufferIsActive;
        Texture previousTarget = null;

        if (!isDrawing)
        {
            _framebufferIsActive = true;

            // remember standard frame buffer
            previousTarget = _renderSupport.RenderTarget;

            GLTexture rootTexture = Root;
            float width = rootTexture.Width;
            float height = rootTexture.Height;

            // switch to the texture's framebuffer for rendering
            _renderSupport.RenderTarget = rootTexture;

            // prepare clipping and OpenGL matrices
            _renderSupport.PushClipRect(new Rectangle(0, 0, width, height));
            // NOTE: this modifies the render matrix, changes must be reverted!
            _renderSupport.SetupOrthographicProjection(0, width, height, 0);
        }

        block();

        if (!isDrawing)
        {
            _framebufferIsActive = false;

            _renderSupport.FinishQuadBatch();
            _renderSupport.NextFrame();

            // return to standard frame buffer
            _renderSupport.RenderTarget = previousTarget;
        }
    }
    */
}
}

