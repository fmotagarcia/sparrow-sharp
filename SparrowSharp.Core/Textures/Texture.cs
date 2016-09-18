
using System;
using Sparrow.Geom;
using Sparrow.Utils;
using Sparrow.Core;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Textures
{
    public abstract class Texture
    {

        public static Texture FromData(IntPtr imgData, TextureOptions properties, 
                                       int width, int height)
        {
            if (imgData == IntPtr.Zero)
            {
                throw new ArgumentException("imgData cannot be a null pointer!");
            }
            Texture tex = Empty(width, height, properties.PremultipliedAlpha, 
                                properties.NumMipMaps, properties.OptimizeForRenderToTexture,
                                properties.Scale, properties.Format);

            GL.TexSubImage2D(TextureTarget.Texture2D,
                        0, // level
                        0, // xOffset
                        0, // yOffset
                        width,
                        height,
                        properties.Format.PixelFormat,
                        properties.Format.PixelType,
                        imgData);
            if (properties.NumMipMaps > 0)
            {
#if __WINDOWS__
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
#elif __ANDROID__
                GL.GenerateMipmap(TextureTarget.Texture2D);
#endif
            }
            return tex;
        }

        // TODO this is not tested!
        public static Texture FromCompressedData(IntPtr imgData, TextureOptions properties,
                                       int width, int height)
        {
            if (imgData == IntPtr.Zero)
            {
                throw new ArgumentException("imgData cannot be a null pointer!");
            }
            Texture tex = Empty(width, height, properties.PremultipliedAlpha,
                                properties.NumMipMaps, properties.OptimizeForRenderToTexture,
                                properties.Scale, properties.Format);

            int size = Math.Max(32, width * height * properties.Format.BitsPerPixel / 8);
            GL.CompressedTexSubImage2D(TextureTarget.Texture2D,
                0, // level
                0, // xOffset
                0, // yOffset
                width,
                height,
                properties.Format.PixelFormat,
                size,
                imgData);

            if (properties.NumMipMaps > 0)
            {
#if __WINDOWS__
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
#elif __ANDROID__
                GL.GenerateMipmap(TextureTarget.Texture2D);
#endif
            }
            return tex;
        }

        /** Creates an empty texture of a certain size.
         *  Beware that the texture can only be used after you either upload some color data
         *  ("texture.root.upload...") or clear the texture ("texture.root.clear()").
         *
         *  @param width   in points; number of pixels depends on scale parameter
         *  @param height  in points; number of pixels depends on scale parameter
         *  @param premultipliedAlpha  the PMA format you will use the texture with. If you will
         *                 use the texture for bitmap data, use "true"; for ATF data, use "false".
         *  @param mipMapping  indicates if mipmaps should be used for this texture. When you upload
         *                 bitmap data, this decides if mipmaps will be created; when you upload ATF
         *                 data, this decides if mipmaps inside the ATF file will be displayed.
         *  @param optimizeForRenderToTexture  indicates if this texture will be used as render target
         *  @param scale   if you omit this parameter, 'Starling.contentScaleFactor' will be used.
         *  @param format  the context3D texture format to use. Pass one of the packed or
         *                 compressed formats to save memory (at the price of reduced image quality).
         */
        public static Texture Empty(float width, float height, bool premultipliedAlpha = true,
                                    int numMipMaps = 0, bool optimizeForRenderToTexture = false,
                                    float scale = -1, TextureFormat format = null)
        {
            if (format == null) format = TextureFormat.Rgba4444;
            if (scale <= 0.0f) scale = SparrowSharpApp.ContentScaleFactor;

            int actualWidth;
            int actualHeight;

            float origWidth  = width * scale;
            float origHeight = height * scale;
            
            actualWidth  = (int)Math.Ceiling(origWidth  - 0.000000001d); // avoid floating point errors
            actualHeight = (int)Math.Ceiling(origHeight - 0.000000001d);
            
            bool compressed = format.Compressed;
            int glTexName = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, glTexName);

#if __WINDOWS__
            GL.TexStorage2D(TextureTarget2d.Texture2D,
#elif __ANDROID__
            GL.TexStorage2D(TextureTarget2D.Texture2D,
#endif
                numMipMaps + 1, // mipmap level, min 1
                format.InternalFormat,
                actualWidth,
                actualHeight);
            
            ConcreteTexture concreteTexture = new ConcreteTexture(
                    glTexName, format, actualWidth, actualHeight, numMipMaps,
                    premultipliedAlpha, optimizeForRenderToTexture, scale);
            

            if (actualWidth - origWidth < 0.001f && actualHeight - origHeight < 0.001f)
            {
                return concreteTexture;
            }
            else
            {
                throw new NotImplementedException();
//                return new SubTexture(concreteTexture, new Rectangle(0.0f, 0.0f, width, height), true);
            }
        }


        /** Disposes the underlying texture data. Note that not all textures need to be disposed:
         *  SubTextures (created with 'Texture.fromTexture') just reference other textures and
         *  and do not take up resources themselves; this is also true for textures from an
         *  atlas. */
        public abstract void Dispose();
        

        /** Creates a texture that contains a region (in pixels) of another texture. The new
         *  texture will reference the base texture; no data is duplicated.
         *
         *  @param texture  The texture you want to create a SubTexture from.
         *  @param region   The region of the parent texture that the SubTexture will show
         *                  (in points).
         *  @param frame    If the texture was trimmed, the frame rectangle can be used to restore
         *                  the trimmed area.
         *  @param rotated  If true, the SubTexture will show the parent region rotated by
         *                  90 degrees (CCW).
         *  @param scaleModifier  The scale factor of the new texture will be calculated by
         *                  multiplying the parent texture's scale factor with this value.
         */
        public static Texture FromTexture(Texture texture, Rectangle region = null,
                                          Rectangle frame = null, bool rotated = false,
                                          float scaleModifier = 1.0f)
        {
            throw new NotImplementedException();
//            return new SubTexture(texture, region, false, frame, rotated, scaleModifier);
        }

        /** Sets up a VertexData instance with the correct positions for 4 vertices so that
        *  the texture can be mapped onto it unscaled. If the texture has a <code>frame</code>,
        *  the vertices will be offset accordingly.
        *
        *  @param vertexData  the VertexData instance to which the positions will be written.
        *  @param vertexID    the start position within the VertexData instance.
        *  @param bounds      useful only for textures with a frame. This will position the
        *                     vertices at the correct position within the given bounds,
        *                     distorted appropriately.
        */
        public void SetupVertexPositions(VertexData vertexData, int vertexID = 0,
                                         Rectangle bounds = null)
        {
            Rectangle frame = Frame;
            float width     = Width;
            float height    = Height;

            Rectangle sRectangle = Rectangle.Create();
            if (frame != null)
                sRectangle.SetTo(-frame.X, -frame.Y, width, height);
            else
                sRectangle.SetTo(0, 0, width, height);
            
            vertexData.SetPoint(vertexID,     sRectangle.Left,  sRectangle.Top);
            vertexData.SetPoint(vertexID + 1, sRectangle.Right, sRectangle.Top);
            vertexData.SetPoint(vertexID + 2, sRectangle.Left,  sRectangle.Bottom);
            vertexData.SetPoint(vertexID + 3, sRectangle.Right, sRectangle.Bottom);

            if (bounds != null)
            {
                float scaleX = bounds.Width  / FrameWidth;
                float scaleY = bounds.Height / FrameHeight;

                if (scaleX != 1.0 || scaleY != 1.0 || bounds.X != 0 || bounds.Y != 0)
                {
                    Matrix sMatrix = Matrix.Create();
                    sMatrix.Identity();
                    sMatrix.Scale(scaleX, scaleY);
                    sMatrix.Translate(bounds.X, bounds.Y);
                    vertexData.TransformVertices(sMatrix, vertexID, 4);
                }
            }
        }

        /** Sets up a VertexData instance with the correct texture coordinates for
         *  4 vertices so that the texture is mapped to the complete quad.
         *
         *  @param vertexData  the vertex data to which the texture coordinates will be written.
         *  @param vertexID    the start position within the VertexData instance.
         *  @param attrName    the attribute name referencing the vertex positions.
         */
        public void SetupTextureCoordinates(VertexData vertexData, int vertexID = 0)
        {
            SetTexCoords(vertexData, vertexID    , 0.0f, 0.0f);
            SetTexCoords(vertexData, vertexID + 1, 1.0f, 0.0f);
            SetTexCoords(vertexData, vertexID + 2, 0.0f, 1.0f);
            SetTexCoords(vertexData, vertexID + 3, 1.0f, 1.0f);
        }

        /** Transforms the given texture coordinates from the local coordinate system
         *  into the root texture's coordinate system. */
        public Point LocalToGlobal(float u, float v)
        {
            Point outP = Point.Create();
            if (this == Root)
            {
                outP.X = u;
                outP.Y = v;
            } 
            else outP = TransformationMatrixToRoot.TransformPoint(u, v);
            return outP;
        }

        /** Transforms the given texture coordinates from the root texture's coordinate system
         *  to the local coordinate system. */
        public Point GlobalToLocal(float u, float v)
        {
            Point outP = Point.Create(u, v);
            if (this == Root)
            {
                outP.X = u;
                outP.Y = v;
            }
            else
            {
                Matrix sMatrix = Matrix.Create();
                sMatrix.CopyFromMatrix(TransformationMatrixToRoot);
                sMatrix.Invert();
                outP = sMatrix.TransformPoint(u, v);
            }
            return outP;
        }

        /** Writes the given texture coordinates to a VertexData instance after transforming
         *  them into the root texture's coordinate system. That way, the texture coordinates
         *  can be used directly to sample the texture in the fragment shader. */
        public void SetTexCoords(VertexData vertexData, int vertexID, float u, float v)
        {
            Point sPoint = LocalToGlobal(u, v);
            vertexData.SetTexCoords(vertexID, sPoint);
        }

        /** Reads a pair of texture coordinates from the given VertexData instance and transforms
         *  them into the current texture's coordinate system. (Remember, the VertexData instance
         *  will always contain the coordinates in the root texture's coordinate system!) */
        public Point GetTexCoords(VertexData vertexData, int vertexID)
        {
            Point outP = Point.Create();
            outP = vertexData.TexCoordsAt(vertexID);
            return GlobalToLocal(outP.X, outP.Y);
        }

        // properties

        /** The texture frame if it has one (see class description), otherwise <code>null</code>.
         *  <p>CAUTION: not a copy, but the actual object! Do not modify!</p> */
        virtual public Rectangle Frame { get { return null; } }

        /** The height of the texture in points, taking into account the frame rectangle
         *  (if there is one). */
        virtual public float FrameWidth { get { return Frame != null ? Frame.Width : Width; } }

        /** The width of the texture in points, taking into account the frame rectangle
         *  (if there is one). */
        virtual public float FrameHeight { get { return Frame != null ? Frame.Height : Height; } }

        /** The width of the texture in points. */
        virtual public float Width { get { return 0; } }

        /** The height of the texture in points. */
        virtual public float Height { get { return 0; } }

        /** The width of the texture in pixels (without scale adjustment). */
        virtual public float NativeWidth { get { return 0; } }

        /** The height of the texture in pixels (without scale adjustment). */
        virtual public float NativeHeight { get { return 0f; } }

        /** The scale factor, which influences width and height properties. */
        virtual public float Scale { get { return 1.0f; } }

        /** The Stage3D texture object the texture is based on. */
        virtual public int Base { get { return 0; } }

        /** The concrete texture the texture is based on. */
        virtual public ConcreteTexture Root { get { return null; } }

        /** The <code>Context3DTextureFormat</code> of the underlying texture data. */
        virtual public TextureFormat Format { get { return TextureFormat.Rgba4444; } }

        /** Indicates if the texture contains mip maps. */
        virtual public int NumMipMaps { get { return 0; } }

        /** Indicates if the alpha values are premultiplied into the RGB values. */
        virtual public bool PremultipliedAlpha { get { return false; } }

        /** The matrix that is used to transform the texture coordinates into the coordinate
         *  space of the parent texture, if there is one. @default null
         *
         *  <p>CAUTION: not a copy, but the actual object! Never modify this matrix!</p> */
        virtual public Matrix TransformationMatrix { get { return null; } }

        /** The matrix that is used to transform the texture coordinates into the coordinate
         *  space of the root texture, if this instance is not the root. @default null
         *
         *  <p>CAUTION: not a copy, but the actual object! Never modify this matrix!</p> */
        virtual public Matrix TransformationMatrixToRoot { get { return null; } }

        /** Returns the maximum size constraint (for both width and height) for textures in the
         *  current Context3D profile. */
        public const int MaxSize = 4096;


    /*
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
    */
}
}

