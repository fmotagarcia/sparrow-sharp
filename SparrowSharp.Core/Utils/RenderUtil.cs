using Sparrow.Textures;

namespace SparrowSharp.Core.Utils
{
    public class RenderUtil
    {

        /** Returns a bit field uniquely describing texture format and premultiplied alpha,
         *  so that each required AGAL variant will get its unique ID. This method is most
         *  useful when overriding the <code>programVariantName</code> method of custom
         *  effects.
         *
         *  @return a bit field using the 3 least significant bits.
         */
        public static uint GetTextureVariantBits(Texture texture)
        {
            if (texture == null) return 0;

            uint bitField = 0;
            uint formatBits = 0;

            /* TODO?
            switch (texture.Format)
            {
                case Context3DTextureFormat.COMPRESSED_ALPHA:
                    formatBits = 3; break;
                case Context3DTextureFormat.COMPRESSED:
                    formatBits = 2; break;
                default:
                    formatBits = 1;
            }
            */
            formatBits = 1;

            bitField |= formatBits;

            if (!texture.PremultipliedAlpha)
                bitField |= 1 << 2;

            return bitField;
        }
    }
}
