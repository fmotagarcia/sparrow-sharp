using Sparrow.Display;
using Sparrow.Geom;
using Sparrow.Textures;

namespace Sparrow.Filters
{
    public interface IFilterHelper
    {

        /** Gets a pass texture from the pool, or creates a new one (cleared and ready to be used
         *  as render target). Its size is dictated by the bounds of the target display object
         *  plus padding.
         *
         *  <p>Beware: each call of this method counts as one render pass. For performance reasons,
         *  the filter may be configured to render the last pass directly to the back buffer. In
         *  that case, this method will return <code>null</code> for the last pass! That's the
         *  sign for the <code>process</code> method to draw to the back buffer. If you receive
         *  <code>null</code> too soon, the filter class probably didn't correctly override
         *  <code>numPasses</code>.</p>
         */
        Texture GetTexture(float resolution = 1.0f);

        /** Puts a texture back into the pool to be reused later (or to be disposed
         *  with the pool). */
        void PutTexture(Texture texture);

        /** The bounds of the target object (plus padding) in stage coordinates. */
        Rectangle TargetBounds { get; }

        /** The display object the filter is currently attached to. */
        DisplayObject Target { get;  }
    }
}
