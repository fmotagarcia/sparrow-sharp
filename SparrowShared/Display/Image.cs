
using Sparrow.Geom;
using Sparrow.Textures;
using Sparrow.Utils;
using Sparrow.Rendering;

namespace Sparrow.Display
{
    /// <summary>
    /// An Image is a quad with a texture mapped onto it.
    ///
    /// <para>Sparrow uses textures to represent the pixels of an image. To display a
    /// texture, you have to map it onto a quad - and that's what the Image class is for.</para>
    ///
    /// <para>While the base class <code>Quad</code> already supports textures, the <code>Image</code>
    /// class adds some additional functionality.</para>
    ///
    /// <para>First of all, it provides a convenient constructor that will automatically synchronize
    /// the size of the image with the displayed texture.</para>
    ///
    /// <para>Furthermore, it adds support for a "Scale9" grid. This splits up the image into
    /// nine regions, the corners of which will always maintain their original aspect ratio.
    /// The center region stretches in both directions to fill the remaining space; the side
    /// regions will stretch accordingly in either horizontal or vertical direction.</para>
    ///
    /// <para>Finally, you can repeat a texture horizontally and vertically within the image's region,
    /// just like the tiles of a wallpaper. Use the <code>TileGrid</code> property to do that.</para>
    /// </summary>
    /// <see cref="Texture"/>
    /// <see cref="Quad"/>
    public class Image : Quad
    {
        private Rectangle _scale9Grid;
        private Rectangle _tileGrid;
        private delegate void SetupScale9GridDelegate(VertexData vertexData, Texture texture,
                                                   int vertexID, float x, float y);
        // helpers
        private static Padding sPadding = new Padding();

        /// <summary>
        ///  Creates an image with a texture mapped onto it.
        /// </summary>
        public Image(Texture texture) : base(100, 100)
        {
            Texture = texture;
            ReadjustSize();
        }
        
        /// <summary>
        /// <para>The current scaling grid that is in effect. If set to null, the image is scaled just
        /// like any other display object; assigning a rectangle will divide the image into a grid
        /// of nine regions, based on the center rectangle. The four corners of this grid will
        /// always maintain their original aspect ratio; the other regions will stretch accordingly
        /// (horizontally, vertically, or both) to fill the complete area.</para>
        ///
        /// Notes:
        /// <list type="bullet">
        ///   <item><description>Assigning a Scale9 rectangle will change the number of vertices to a maximum of 16
        ///   (less if possible) and all vertices will be colored like vertex 0 (the top left vertex).</description>  
        ///   </item>
        ///   <item><description>An image can have either a <code>Scale9Grid</code> or a <code>TileGrid</code>, but
        ///   not both. Assigning one will delete the other.</description>  
        ///   </item>
        ///   <item><description>Changes will only be applied on assignment. To force an update, simply call
        ///   <code>image.Scale9Grid = image.Scale9Grid</code></description>  
        ///   </item>
        ///   <item><description>Assignment causes an implicit call to <code>ReadjustSize()</code>,
        ///   and the same will happen when the texture is changed afterwards.</description>  
        ///   </item> 
        /// </list>
        ///
        /// Default value is <code>null</code>
        /// </summary>
        public Rectangle Scale9Grid {
            get => _scale9Grid;
            set {
                if (value != null)
                {
                    if (_scale9Grid == null) _scale9Grid = value.Clone();
                    else _scale9Grid.CopyFrom(value);

                    ReadjustSize();
                    _tileGrid = null;
                }
                else _scale9Grid = null;

                SetupVertices();
            }
        }
        
        /// <summary>
        /// The current tiling grid that (repeat) is in effect. If set to null, the image is scaled 
        /// just like any other display object; assigning a rectangle will divide the image into a grid
        /// displaying the current texture in each and every cell. The assigned rectangle points
        /// to the bounds of one cell; all other elements will be calculated accordingly. A zero
        /// or negative value for the rectangle's width or height will be replaced with the actual
        /// texture size. Thus, you can make a 2x2 grid simply like this:
        /// 
        /// <code>
        /// var image = new Image(texture);
        /// image.TileGrid = Rectangle.Create(5, 5, 10, 10);
        /// image.Scale = 2;
        /// </code>
        /// Notes:
        /// <list type="bullet">
        ///   <item><description>Assigning a tile rectangle will change the number of vertices to whatever is
        ///   required by the grid. New vertices will be colored just like vertex 0 (the top left
        ///   vertex).</description>  
        ///   </item>
        ///   <item><description>An image can have either a <code>Scale9Grid</code> or a <code>TileGrid</code>, but
        ///   not both. Assigning one will delete the other.</description>  
        ///   </item>
        ///   <item><description>Changes will only be applied on assignment. To force an update, simply call
        ///   <code>image.TileGrid = image.TileGrid</code></description>  
        ///   </item>
        /// </list>
        /// 
        /// Default value is <code>null</code>
        /// </summary>
        public Rectangle TileGrid { 
            get => _tileGrid;
            set
            {
                if (value != null)
                {
                    if (_tileGrid == null) _tileGrid = value.Clone();
                    else _tileGrid.CopyFrom(value);

                    _scale9Grid = null;
                }
                else _tileGrid = null;

                SetupVertices();
            }
        }
        
        protected override void SetupVertices()
        {
            if (Texture != null && _scale9Grid != null) SetupScale9Grid();
            else if (Texture != null && _tileGrid != null) SetupTileGrid();
            else base.SetupVertices();
        }

        public override float ScaleX
        {
            set
            {
                base.ScaleX = value;
                if (Texture != null && (_scale9Grid != null || _tileGrid != null))
                {
                    SetupVertices();
                }
            }
        }

        public override float ScaleY
        {
            set
            {
                base.ScaleY = value;
                if (Texture != null && (_scale9Grid != null || _tileGrid != null))
                {
                    SetupVertices();
                }
            }
        }
        
        public override Texture Texture
        {
            set
            {
                if (value != Texture)
                {
                    base.Texture = value;
                    if (_scale9Grid != null && value != null) ReadjustSize();
                }
            }
        }

        // vertex setup

        private void SetupScale9Grid()
        {
            Texture texture = Texture;
            Rectangle frame = texture.Frame;
            float absScaleX = ScaleX > 0 ? ScaleX : -ScaleX;
            float absScaleY = ScaleY > 0 ? ScaleY : -ScaleY;

            // If top and bottom row / left and right column are empty, this is actually
            // a scale3 grid. In that case, we want the 'caps' to maintain their aspect ratio.

            if (MathUtil.IsEquivalent(_scale9Grid.Width, texture.FrameWidth))
            {
                absScaleY /= absScaleX;
            }
            else if (MathUtil.IsEquivalent(_scale9Grid.Height, texture.FrameHeight))
            {
                absScaleX /= absScaleY;
            }
            
            float invScaleX = 1.0f / absScaleX;
            float invScaleY = 1.0f / absScaleY;
            VertexData vertexData = VertexData;
            IndexData indexData = IndexData;
            int prevNumVertices = vertexData.NumVertices;
            float correction;
            
            // The following rectangles are used to figure everything out.
            // The meaning of each is depicted in this sketch: http://i.imgur.com/KUcv71O.jpg
            
            Rectangle gridCenter = Rectangle.Create();
            Rectangle textureBounds = Rectangle.Create();
            Rectangle pixelBounds = Rectangle.Create();
            
            gridCenter.CopyFrom(_scale9Grid);
            textureBounds.SetTo(0, 0, texture.FrameWidth, texture.FrameHeight);

            if (frame != null) pixelBounds.SetTo(-frame.X, -frame.Y, texture.Width, texture.Height);
            else pixelBounds.CopyFrom(textureBounds);

            // calculate 3x3 grid according to texture and scale9 properties,
            // taking special care about the texture frame (headache included)

            var intersection = gridCenter.Intersection(pixelBounds);

            float[] sBasCols = new float[3];
            float[] sBasRows = new float[3];
            sBasCols[1] = intersection.Width;
            sBasRows[1] = intersection.Height;

            if (pixelBounds.X < gridCenter.X)
                sBasCols[0] = gridCenter.X - pixelBounds.X;

            if (pixelBounds.Y < gridCenter.Y)
                sBasRows[0] = gridCenter.Y - pixelBounds.Y;

            if (pixelBounds.Right > gridCenter.Right)
                sBasCols[2] = pixelBounds.Right - gridCenter.Right;

            if (pixelBounds.Bottom > gridCenter.Bottom)
                sBasRows[2] = pixelBounds.Bottom - gridCenter.Bottom;

            // set vertex positions
            
            if (pixelBounds.X < gridCenter.X)
                sPadding.Left = pixelBounds.X * invScaleX;
            else
                sPadding.Left = gridCenter.X * invScaleX + pixelBounds.X - gridCenter.X;

            if (pixelBounds.Right > gridCenter.Right)
                sPadding.Right = (textureBounds.Width - pixelBounds.Right) * invScaleX;
            else
                sPadding.Right = (textureBounds.Width - gridCenter.Right) * invScaleX + gridCenter.Right - pixelBounds.Right;

            if (pixelBounds.Y < gridCenter.Y)
                sPadding.Top = pixelBounds.Y * invScaleY;
            else
                sPadding.Top = gridCenter.Y * invScaleY + pixelBounds.Y - gridCenter.Y;

            if (pixelBounds.Bottom > gridCenter.Bottom)
                sPadding.Bottom = (textureBounds.Height - pixelBounds.Bottom) * invScaleY;
            else
                sPadding.Bottom = (textureBounds.Height - gridCenter.Bottom) * invScaleY + gridCenter.Bottom - pixelBounds.Bottom;

            float[] sPosCols = new float[3];
            float[] sPosRows = new float[3];
            sPosCols[0] = sBasCols[0] * invScaleX;
            sPosCols[2] = sBasCols[2] * invScaleX;
            sPosCols[1] = textureBounds.Width - sPadding.Left - sPadding.Right - sPosCols[0] - sPosCols[2];

            sPosRows[0] = sBasRows[0] * invScaleY;
            sPosRows[2] = sBasRows[2] * invScaleY;
            sPosRows[1] = textureBounds.Height - sPadding.Top - sPadding.Bottom - sPosRows[0] - sPosRows[2];

            // if the total width / height becomes smaller than the outer columns / rows,
            // we hide the center column / row and scale the rest normally.

            if (sPosCols[1] <= 0f)
            {
                correction = textureBounds.Width / (textureBounds.Width - gridCenter.Width) * absScaleX;
                sPadding.Left *= correction;
                sPosCols[0] *= correction;
                sPosCols[1] = 0f;
                sPosCols[2] *= correction;
            }

            if (sPosRows[1] <= 0f)
            {
                correction = textureBounds.Height / (textureBounds.Height - gridCenter.Height) * absScaleY;
                sPadding.Top *= correction;
                sPosRows[0] *= correction;
                sPosRows[1] = 0f;
                sPosRows[2] *= correction;
            }

            // now set the texture coordinates
            float[] sTexCols = new float[3];
            float[] sTexRows = new float[3];
            sTexCols[0] = sBasCols[0] / pixelBounds.Width;
            sTexCols[2] = sBasCols[2] / pixelBounds.Width;
            sTexCols[1] = 1.0f - sTexCols[0] - sTexCols[2];

            sTexRows[0] = sBasRows[0] / pixelBounds.Height;
            sTexRows[2] = sBasRows[2] / pixelBounds.Height;
            sTexRows[1] = 1.0f - sTexRows[0] - sTexRows[2];

            var numVertices = SetupScale9GridAttributes(
                sPadding.Left, sPadding.Top, sPosCols, sPosRows, sTexCols, sTexRows);

            // update indices

            var numQuads = numVertices / 4;
            vertexData.NumVertices = numVertices;
            indexData.NumIndices = 0;

            for (int i = 0; i < numQuads; ++i)
                indexData.AddQuad((short)(i * 4), (short)(i * 4 + 1), (short)(i * 4 + 2), (short)(i * 4 + 3));

            // if we just switched from a normal to a scale9 image,
            // we need to colorize all vertices just like the first one.

            if (numVertices != prevNumVertices)
            {
                uint color  = prevNumVertices != 0 ? vertexData.GetColor(0) : 0xffffff;
                float alpha = prevNumVertices != 0 ? vertexData.GetAlpha(0) : 1.0f;
                vertexData.Colorize(color, alpha);
            }

            SetRequiresRedraw();
        }

        private int SetupScale9GridAttributes(float startX, float startY,
                                              float[] posCols,
                                              float[] posRows,
                                              float[] texCols,
                                              float[] texRows)
        {
            VertexData vertexData = VertexData;
            Texture texture = Texture;
            float currentX = startX;
            float currentY = startY;
            float currentU = 0f;
            float currentV = 0f;
            int vertexId = 0;

            for (int row = 0; row < 3; ++row)
            {
                var rowHeightPos = posRows[row];
                var rowHeightTex = texRows[row];

                if (rowHeightPos > 0)
                {
                    for (int col = 0; col < 3; ++col)
                    {
                        var colWidthPos = posCols[col];
                        var colWidthTex = texCols[col];

                        if (colWidthPos > 0)
                        {
                            vertexData.SetPoint(vertexId, currentX, currentY);
                            texture.SetTexCoords(vertexData, vertexId, currentU, currentV);
                            vertexId++;

                            vertexData.SetPoint(vertexId, currentX + colWidthPos, currentY);
                            texture.SetTexCoords(vertexData, vertexId, currentU + colWidthTex, currentV);
                            vertexId++;

                            vertexData.SetPoint(vertexId, currentX, currentY + rowHeightPos);
                            texture.SetTexCoords(vertexData, vertexId, currentU, currentV + rowHeightTex);
                            vertexId++;

                            vertexData.SetPoint(vertexId, currentX + colWidthPos, currentY + rowHeightPos);
                            texture.SetTexCoords(vertexData, vertexId, currentU + colWidthTex, currentV + rowHeightTex);
                            vertexId++;

                            currentX += colWidthPos;
                        }

                        currentU += colWidthTex;
                    }

                    currentY += rowHeightPos;
                }

                currentX = startX;
                currentU = 0f;
                currentV += rowHeightTex;
            }

            return vertexId;
        }

        private void SetupTileGrid()
        {
            // calculate the grid of vertices simulating a repeating / tiled texture.
            // again, texture frames make this somewhat more complicated than one would think.

            Texture texture = Texture;
            Rectangle frame = texture.Frame;
            VertexData vertexData = VertexData;
            IndexData indexData = IndexData;
            Rectangle bounds = GetBounds(this);
            int prevNumVertices = vertexData.NumVertices;
            uint color   = prevNumVertices != 0 ? vertexData.GetColor(0) : 0xffffff;
            float alpha = prevNumVertices != 0 ? vertexData.GetAlpha(0) : 1.0f;
            float invScaleX = ScaleX > 0 ? 1.0f / ScaleX : -1.0f / ScaleX;
            float invScaleY = ScaleY > 0 ? 1.0f / ScaleY : -1.0f / ScaleY;
            float frameWidth = _tileGrid.Width > 0 ? _tileGrid.Width : texture.FrameWidth;
            float frameHeight = _tileGrid.Height > 0 ? _tileGrid.Height : texture.FrameHeight;

            frameWidth *= invScaleX;
            frameHeight *= invScaleY;

            float tileX = frame != null ? -frame.X * (frameWidth / frame.Width) : 0;
            float tileY = frame != null  ? -frame.Y * (frameHeight / frame.Height) : 0;
            float tileWidth = texture.Width * (frameWidth / texture.FrameWidth);
            float tileHeight = texture.Height * (frameHeight / texture.FrameHeight);
            float modX = (_tileGrid.X * invScaleX) % frameWidth;
            float modY = (_tileGrid.Y * invScaleY) % frameHeight;

            if (modX < 0) modX += frameWidth;
            if (modY < 0) modY += frameHeight;

            float startX = modX + tileX;
            float startY = modY + tileY;

            if (startX > (frameWidth - tileWidth)) startX -= frameWidth;
            if (startY > (frameHeight - tileHeight)) startY -= frameHeight;

            float posLeft, posRight, posTop, posBottom;
            float texLeft, texRight, texTop, texBottom;
            float currentX;
            float currentY = startY;
            int vertexId = 0;

            indexData.NumIndices = 0;

            while (currentY < bounds.Height)
            {
                currentX = startX;

                while (currentX < bounds.Width)
                {
                    indexData.AddQuad((short)vertexId, (short)(vertexId + 1), (short)(vertexId + 2), (short)(vertexId + 3));

                    posLeft = currentX < 0 ? 0 : currentX;
                    posTop = currentY < 0 ? 0 : currentY;
                    posRight = currentX + tileWidth > bounds.Width ? bounds.Width : currentX + tileWidth;
                    posBottom = currentY + tileHeight > bounds.Height ? bounds.Height : currentY + tileHeight;

                    vertexData.SetPoint(vertexId, posLeft, posTop);
                    vertexData.SetPoint(vertexId + 1, posRight, posTop);
                    vertexData.SetPoint(vertexId + 2, posLeft, posBottom);
                    vertexData.SetPoint(vertexId + 3, posRight, posBottom);

                    texLeft = (posLeft - currentX) / tileWidth;
                    texTop = (posTop - currentY) / tileHeight;
                    texRight = (posRight - currentX) / tileWidth;
                    texBottom = (posBottom - currentY) / tileHeight;

                    texture.SetTexCoords(vertexData, vertexId, texLeft, texTop);
                    texture.SetTexCoords(vertexData, vertexId + 1, texRight, texTop);
                    texture.SetTexCoords(vertexData, vertexId + 2, texLeft, texBottom);
                    texture.SetTexCoords(vertexData, vertexId + 3, texRight, texBottom);

                    currentX += frameWidth;
                    vertexId += 4;
                }

                currentY += frameHeight;
            }

            // trim to actual size
            vertexData.NumVertices = vertexId;

            for (int i = prevNumVertices; i < vertexId; ++i)
            {
                vertexData.SetColor(i, color);
                vertexData.SetAlpha(i, alpha);
            }

            SetRequiresRedraw();
        }

}
}