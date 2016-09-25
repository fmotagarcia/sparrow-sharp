
using Sparrow.Core;
using Sparrow.Textures;

namespace Sparrow.Fonts
{
    
    public class TextOptions
    {
        // + add here HTML text support

        /** Indicates if the text should be wrapped at word boundaries if it does not fit into
        *  the TextField otherwise. @default true */
        public bool WordWrap { get; set; }
        /** Indicates whether the font size is automatically reduced if the complete text does
         *  not fit into the TextField. @default false */
        public bool AutoScale { get; set; }
        /** Specifies the type of auto-sizing set on the TextField.Custom text compositors may
         *  take this into account, though the basic implementation (done by the TextField itself)
         *  is often sufficient: it passes a very big size to the <code>fillMeshBatch</code>
         *  method and then trims the result to the actually used area. @default none */
        public TextFieldAutoSize AutoSize { get; set; }
        /** The scale factor of any textures that are created during text composition.
         *  @default Starling.contentScaleFactor */
        public float TextureScale { get; set; }
        /** The Context3DTextureFormat of any textures that are created during text composition.
         *  @default Context3DTextureFormat.BGRA_PACKED */
        public TextureFormat TextureFormat { get; set; }

        /** Creates a new TextOptions instance with the given properties. */
        public TextOptions(bool wordWrap = true, bool autoScale = false)
        {
            WordWrap = wordWrap;
            AutoScale = autoScale;
            AutoSize = TextFieldAutoSize.NONE;
            TextureScale = SparrowSharpApp.ContentScaleFactor;
            TextureFormat = TextureFormat.RGB565; // likely wrong
        }

        /** Copies all properties from another TextOptions instance. */
        public void CopyFrom(TextOptions options)
        {
            WordWrap = options.WordWrap;
            AutoScale = options.AutoScale;
            AutoSize = options.AutoSize;
            TextureScale = options.TextureScale;
            TextureFormat = options.TextureFormat;
        }

        /** Creates a clone of this instance. */
        public TextOptions Clone()
        {
            var clone = new TextOptions();
            clone.CopyFrom(this);
            return clone;
        }

}
}
