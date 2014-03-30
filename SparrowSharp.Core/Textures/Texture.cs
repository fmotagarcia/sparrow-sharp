using System;
using Sparrow.Geom;
using Sparrow.Utils;
using Sparrow.Display;

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

		abstract public bool Repeat { 
			get;
			set;
        }

		abstract public TextureSmoothing Smoothing {
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

        public Texture()
        {
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
			// override in subclasses if needed
        }

        virtual public void AdjustTexCoords(VertexData vertexData, uint startIndex, uint count)
        {
			// override in subclasses if needed
        }

        virtual public void AdjustPositions(VertexData vertexData, uint startIndex, uint count)
        {
			// override in subclasses if needed
        }
    }
}

