using System;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Geom;
using Sparrow.Utils;
using Android.Content.Res;
using System.Net;
using Sparrow.Display;

namespace Sparrow.Core
{
    public class Texture
    {
        virtual public uint Name { get { throw new Exception("Override 'Name' in subclasses."); } }

        virtual public float NativeWidth { get { throw new Exception("Override 'NativeWidth' in subclasses."); } }

        virtual public float NativeHeight { get { throw new Exception("Override 'NativeHeight' in subclasses."); } }

        virtual public float Height { get { throw new Exception("Override 'Height' in subclasses."); } }

        virtual public float Width { get { throw new Exception("Override 'Width' in subclasses."); } }

        virtual public float Scale { get { return 1.0f; } }
        // TODO virtual public GLTexture Root { get {null;} }
        virtual public Rectangle Frame { get { return null; } }

        virtual public bool PremultipliedAlpha { get { return false; } }
        // TODO virtual public bool Format { get { return TextureFormat.RGBA; } }
        virtual public bool MipMaps { get { return false; } }

        virtual public bool Repeat
        { 
            get { throw new Exception("Override 'Repeat' in subclasses."); }
            set { throw new Exception("Override 'Repeat' in subclasses."); }
        }

        public Texture.TextureSmoothing Smoothing { get; set; }

        public enum TextureSmoothing
        {
            None,
            Bilinear,
            Trilinear
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
        //TODO virtual public TextureSmoothing Smoothing {
        //	get {throw new Exception ("Override 'TextureSmoothing' in subclasses.");}
        //	set {throw new Exception ("Override 'TextureSmoothing' in subclasses.");}
        //}
        public Texture()
        {
            int name = GL.GenTexture();
            GL.BindTexture(All.Texture2D, name);

            // setup texture parameters
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			
            Bitmap b = BitmapFactory.DecodeResource(ViewController.ResourcesRef, sparrowsharp.Resource.Drawable.exampleImageJPG);

            // this uses Android to set up things, it might not be safe to use Android calls mixed with OpenTK calls
            Android.Opengl.GLUtils.TexImage2D(Android.Opengl.GLES20.GlTexture2d, 0, b, 0);
            // see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
            // for how MonoGame does it
        }

        public Texture(string path) : this(path, false)
        {

        }

        public Texture(string path, bool generateMipmaps)
        {
            // TODO
//            if (cachingEnabled)
//            {
//                SPTexture *cachedTexture = [textureCache textureForKey:path];
//                if (cachedTexture)
//                {
//                    [self release]; // return the cached texture
//                    return [cachedTexture retain];
//                }
//            }
//
//            NSString *fullPath = [SPUtils absolutePathToFile:path];
//            if (!fullPath)
//                [NSException raise:SPExceptionFileNotFound format:@"File '%@' not found", path];
//
//            if ([SPTexture isPVRFile:fullPath])
//            {
//                BOOL isCompressed = [SPTexture isCompressedFile:fullPath];
//                float scale = [fullPath contentScaleFactor];
//
//                NSData *rawData = [[NSData alloc] initWithContentsOfFile:fullPath];
//                SPPVRData *pvrData = [[SPPVRData alloc] initWithData:rawData compressed:isCompressed];
//
//                [self release]; // we'll return a subclass!
//                self = [[SPGLTexture alloc] initWithPVRData:pvrData scale:scale];
//
//                [rawData release];
//                [pvrData release];
//            }
//            else
//            {
//                // load image via this crazy workaround to be sure that path is not extended with scale
//                NSData *data = [[NSData alloc] initWithContentsOfFile:fullPath];
//                UIImage *image1 = [[UIImage alloc] initWithData:data];
//                UIImage *image2 = [[UIImage alloc] initWithCGImage:image1.CGImage
//                    scale:[fullPath contentScaleFactor] orientation:UIImageOrientationUp];
//
//                self = [self initWithContentsOfImage:image2 generateMipmaps:mipmaps];
//
//                [image2 release];
//                [image1 release];
//                [data release];
//            }
//
//            if (cachingEnabled)
//                [textureCache setTexture:self forKey:path];
//
//            return self;
        }

        virtual public void AdjustVertexData(VertexData vertexData, uint startIndex, uint count)
        {
            // override in subclasses
        }

        virtual public void AdjustTexCoords(VertexData vertexData, uint startIndex, uint count)
        {
            // override in subclasses
        }

        virtual public void AdjustPositions(VertexData vertexData, uint startIndex, uint count)
        {
            // override in subclasses
        }
    }
}

