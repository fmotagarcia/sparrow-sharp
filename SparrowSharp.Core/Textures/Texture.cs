using Sparrow.Geom;
using Sparrow.Utils;

namespace Sparrow.Textures
{
    public abstract class Texture
    {
        abstract public uint Name { get; }

        abstract public float NativeWidth { get; }

        abstract public float NativeHeight { get; }

        abstract public float Height { get; }

        abstract public float Width { get; }

        virtual public float Scale { get { return 1.0f; } }

        virtual public GLTexture Root { get { return null; } }

        virtual public Rectangle Frame { get { return null; } }

        virtual public bool PremultipliedAlpha { get { return false; } }

        virtual public bool MipMaps { get { return false; } }

        virtual public TextureFormat Format { get { return TextureFormat.RGBA; } }

        abstract public bool Repeat
        { 
            get;
            set;
        }

        abstract public TextureSmoothing Smoothing
        {
            get;
            set;
        }

        public enum TextureFormat
        {
            RGBA,
            Alpha,
            PvrtcRGB2,
            PvrtcRGBA2,
            PvrtcRGB4,
            PvrtcRGBA4,
            BGR565,
            BGR888,
            BGR5551,
            BGR4444,
            AI88,
            I8
        }

        /// <summary>
        /// Converts texture coordinates and vertex positions of raw vertex data into the format
        /// required for rendering.
        /// </summary>
        virtual public void AdjustVertexData(VertexData vertexData, uint startIndex, uint count)
        {
        }

        /// <summary>
        /// Converts texture coordinates stored at the given memory region into the format required for
        /// rendering. While the texture coordinates of an image always use the range [0, 1], the actual
        /// coordinates could be different: you might be working with a SubTexture. This method adjusts
        /// the coordinates accordingly.
        /// </summary>
        virtual public void AdjustTexCoords(VertexData vertexData, uint startIndex, uint count)
        {
        }

        /// <summary>
        /// Moves the position coordinates stored at the given memory region into the format required for
        /// rendering. This happens for SubTextures that contain a 'frame'.
        /// </summary>
        virtual public void AdjustPositions(VertexData vertexData, uint startIndex, uint count)
        {
        }
    }
}

