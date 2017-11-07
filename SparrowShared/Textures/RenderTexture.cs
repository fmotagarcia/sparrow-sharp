using System;
using Sparrow.Display;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Rendering;
using Sparrow.Filters;

namespace Sparrow.Textures
{
    /// <summary>
    /// A RenderTexture is a dynamic texture on which you can draw any display object.
    ///
    /// After creating a render texture, just call the 'DrawObject' method to render an object directly 
    /// onto the texture. The object will be drawn onto the texture at its current position, adhering 
    /// its current rotation, scale and alpha properties. 
    ///
    /// Drawing is done very efficiently, as it is happening directly in graphics memory. After you have 
    /// drawn objects on the texture, the performance will be just like that of a normal texture - no
    /// matter how many objects you have drawn.
    ///
    /// If you draw lots of objects at once, it is recommended to bundle the drawing calls in a block
    /// via the 'DrawBundled' method, like shown below. That will speed it up immensely, allowing
    /// you to draw hundreds of objects very quickly.
    ///
    /// <code>
    /// renderTexture.DrawBundled(delegate
    /// {
    ///     for (int i=0; i &lt; numDrawings; ++i)
    ///	    {
    ///         image.Rotation = (2 * (float)Math.PI / numDrawings) * i;
    ///	        renderTexture.Draw(image);            
    ///     }             
    /// });
    /// </code>
    /// </summary>
    public class RenderTexture : SubTexture
    {
        private Texture _activeTexture;
        private bool _drawing;
        private bool _bufferReady;
        private readonly bool _isPersistent;


        /// <summary>
        /// Creates a new RenderTexture with a certain size(in points). If the texture is
        /// persistent, its contents remains intact after each draw call, allowing you to use the
        /// texture just like a canvas.If it is not, it will be cleared before each draw call.
        ///
        /// <para>Non-persistent textures can be used more efficiently on older devices; on modern
        /// hardware, it does not make a difference.For more information, have a look at the
        /// documentation of the <code>useDoubleBuffering</code> property.</para>
        /// </summary>
        public RenderTexture(int width, int height, bool persistent = true,
                             float scale = -1, TextureFormat format = null)
        {
            if (format == null)
            {
                format = TextureFormat.Rgba4444;
            }

            _isPersistent = persistent;
            _activeTexture = Empty(width, height, true, 0, true, scale, format);

            SetTo(_activeTexture, Rectangle.Create(0, 0, width, height), true);
        }

        public override void Dispose()
        {
            _activeTexture.Dispose();
            _activeTexture = null;
            base.Dispose();
        }
        
        /// <summary>
        /// Draws an object into the texture. Note that any filters on the object will currently
        /// be ignored.
        /// </summary>
        /// <param name="obj">The object to draw.</param>
        /// <param name="matrix">If 'matrix' is null, the object will be drawn adhering its 
        ///                      properties for position, scale, and rotation. If it is not null,
        ///                       the object will be drawn in the orientation depicted by the matrix.</param>
        /// <param name="alpha">The object's alpha value will be multiplied with this value.</param>
        /// <param name="antiAliasing">Values range from 0 (no antialiasing) to 4 (best quality).</param>
        public void Draw(DisplayObject obj, Matrix2D matrix = null, float alpha = 1.0f,
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

        private void Render(DisplayObject obj, Matrix2D matrix = null, float alpha = 1.0f)
        {
            Painter painter = SparrowSharp.Painter;
            RenderState state = painter.State;
            bool wasCacheEnabled = painter.CacheEnabled;
            FragmentFilter filter = obj.Filter;
            DisplayObject mask = obj.Mask;

            painter.CacheEnabled = false;
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
            painter.CacheEnabled = wasCacheEnabled;
        }

        private void RenderBundled(Action<DisplayObject, Matrix2D, float> renderBlock, DisplayObject obj = null,
                                   Matrix2D matrix = null, float alpha = 1.0f, int antiAliasing = 0)
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

        /// <summary>
        /// Clears the render texture with a certain color and alpha value. Call without any
        /// arguments to restore full transparency.
        /// </summary>
        public void Clear(uint color = 0, float alpha = 0.0f)
        {
            _activeTexture.Root.Clear(color, alpha);
            _bufferReady = true;
        }

        // properties

        /// <summary>
        /// Indicates if the texture is persistent over multiple draw calls.
        /// </summary>
        public bool IsPersistent { get { return _isPersistent; } }

        
        /// <inheritdoc cref="SubTexture.Base"/>
        public override uint Base { get { return _activeTexture.Base; } }
        
        /// <inheritdoc cref="SubTexture.Root"/>
        public override ConcreteTexture Root { get { return _activeTexture.Root; } }

}
}

