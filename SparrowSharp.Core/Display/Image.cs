using System;
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;

namespace Sparrow.Display
{
    /// <summary>
    /// An Image displays a quad with a texture mapped onto it.
 
    /// Sparrow uses the Texture class to represent textures. To display a texture, you have to map
    /// it on a quad - and that's what Image is for.
 
    /// As Image inherits from Quad, you can give it a color. For each pixel, the resulting color will
    /// be the result of the multiplication of the color of the texture with the color of the quad. That 
    /// way, you can easily tint textures with a certain color. 
 
    /// Furthermore, Image allows the manipulation of texture coordinates. That way, you can move a 
    /// texture inside an image without changing any vertex coordinates of the quad. You can also use 
    /// this feature as a very efficient way to create a rectangular mask.
    /// </summary>
    public class Image : Quad
    {
        private readonly VertexData _vertexDataCache;
        private bool _vertexDataCacheInvalid;
        private Texture _texture;

        /// <summary>
        /// Initialize a quad with a texture mapped onto it
        /// </summary>
        /// <param name="texture">The texture to use. Use for example the TextureLoader class to load one.</param>
        public Image(Texture texture)
        {
            if (texture == null)
            {
                throw new Exception("texture cannot be null!");
            }

            Rectangle frame = texture.Frame;    
            float width = (frame != null) ? frame.Width : texture.Width;
            float height = (frame != null) ? frame.Height : texture.Height;
            bool pma = texture.PremultipliedAlpha;

            Init(width, height, 0xFFFFFF, pma);

            _vertexData.Vertices[1].TexCoords.X = 1.0f;
            _vertexData.Vertices[2].TexCoords.Y = 1.0f;
            _vertexData.Vertices[3].TexCoords.X = 1.0f;
            _vertexData.Vertices[3].TexCoords.Y = 1.0f;

            _texture = texture;
            _vertexDataCache = new VertexData(4, pma);
            _vertexDataCacheInvalid = true;
        }

        /// <summary>
        /// Sets the texture coordinates of a vertex. Coordinates are in the range [0, 1].
        /// </summary>
        public void SetTexCoords(Point coords, int vertexID)
        {
            _vertexData.SetTexCoords(coords, vertexID);
            VertexDataDidChange();
        }

        /// <summary>
        /// Sets the texture coordinates of a vertex. Coordinates are in the range [0, 1].
        /// </summary>
        public void SetTexCoords(float x, float y, int vertexID)
        {
            _vertexData.SetTexCoords(x, y, vertexID);
            VertexDataDidChange();
        }

        /// <summary>
        /// Gets the texture coordinates of a vertex.
        /// </summary>
        public Point TexCoordsOfVertex(int vertexID)
        {
            return _vertexData.TexCoordsAt(vertexID);
        }

        /// <summary>
        /// Readjusts the dimensions of the image according to its current texture. Call this method 
        /// to synchronize image and texture size after assigning a texture with a different size.
        /// </summary>
        public void ReadjustSize()
        {
            Rectangle frame = _texture.Frame;    
            float width = (frame != null) ? frame.Width : Texture.Width;
            float height = (frame != null) ? frame.Height : Texture.Height;

            _vertexData.Vertices[1].Position.X = width;
            _vertexData.Vertices[2].Position.Y = height;
            _vertexData.Vertices[3].Position.X = width;
            _vertexData.Vertices[3].Position.Y = height;
            VertexDataDidChange();
        }

        protected override void VertexDataDidChange()
        {
            _vertexDataCacheInvalid = true;
        }

        override internal void CopyVertexDataTo(VertexData targetData, int atIndex, bool copyColor)
        {
            copyColor = copyColor || Tinted;

            if (_vertexDataCacheInvalid)
            {
                _vertexDataCacheInvalid = false;
                _vertexData.CopyToVertexData(_vertexDataCache, copyColor);
                _texture.AdjustVertexData(_vertexDataCache, 0, 4);
            }
            _vertexDataCache.CopyToVertexData(targetData, copyColor, atIndex, 4);
        }

        /// <summary>
        /// The texture that is displayed on the quad.
        /// </summary>
        override public Texture Texture
        { 
            set
            {
                if (value == null)
                {
                    throw new Exception("texture cannot be null!");
                }
                else if (value != _texture)
                {
                    _texture = value;
                    _vertexData.SetPremultipliedAlpha(_texture.PremultipliedAlpha, true);
                    _vertexDataCache.SetPremultipliedAlpha(_texture.PremultipliedAlpha, false);
                    VertexDataDidChange();
                }
            }
            get { return _texture; }
        }

        override public bool Tinted 
        { 
            get { 
                return _alpha != 1.0f || _color != 0xFFFFFF;
            } 
        }
    }
}