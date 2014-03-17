using System;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using System.Runtime.InteropServices;
using MonoTouch.GLKit;
using MonoTouch.Foundation;
using OpenTK;

namespace Sparrow.Textures
{
    public class iOSTextureProvider : ITextureProvider
    {
        private Dictionary<uint, int> _resources = new Dictionary<uint, int>();

        public iOSTextureProvider()
        {
        }

        public Texture CreateTexture(uint id)
        {
            int resourceID;

//            if (_resources.TryGetValue(id, out resourceID))
//            {
            uint name = (uint)GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, name);


            UIImage image = UIImage.FromBundle("star.png");
            LoadTexture(All.Texture2D, image);

            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (Int32)All.LinearMipmapLinear);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (Int32)All.Linear);
            GL.GenerateMipmap(All.Texture2D);

            return new GLTexture(name, image.CGImage.Width, image.CGImage.Height, false, 1.0f, false);


//            using (UIImage image = UIImage.FromBundle("benchmark_object.png"))
//            {
//                var cgImage = image.CGImage;
//                var width = cgImage.Width;
//                var height = cgImage.Height;
//                var data = new byte[width * height * 4];
//
//                var colorSpace = CGColorSpace.CreateDeviceRGB();
//                var bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace, CGBitmapFlags.PremultipliedLast);
//                bitmapContext.DrawImage(new RectangleF(0, 0, width, height), cgImage);
//                bitmapContext.Dispose();
//                colorSpace.Dispose();
//
//                var elementSizeInByte = Marshal.SizeOf(typeof(byte));
//                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
//
//                var startBytes = elementSizeInByte;
//                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
//
////                IntPtr dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
//                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, dataPtr);
//
//                GLTexture texture = new GLTexture(name, width, height, false, 1.0f, false);
//                return texture;
//            }
//            }
//
//            return null;
        }

        private void LoadTexture(All usage, UIImage image)
        {
            GL.TexImage2D(usage, 0, (Int32)All.Rgba, (Int32)image.Size.Width,
                (Int32)image.Size.Height, 0, All.Rgba, All.UnsignedByte, RequestImagePixelData(image));
        }

        protected CGBitmapContext CreateARGBBitmapContext(CGImage inImage)
        {
            var pixelsWide = inImage.Width;
            var pixelsHigh = inImage.Height;
            var bitmapBytesPerRow = pixelsWide * 4;
            var bitmapByteCount = bitmapBytesPerRow * pixelsHigh;
            //Note implicit colorSpace.Dispose() 
            using (var colorSpace = CGColorSpace.CreateDeviceRGB())
            {
                //Allocate the bitmap and create context
                var bitmapData = Marshal.AllocHGlobal(bitmapByteCount);
                if (bitmapData == IntPtr.Zero)
                {
                    throw new Exception("Memory not allocated.");
                }

                var context = new CGBitmapContext(bitmapData, pixelsWide, pixelsHigh, 8,
                                  bitmapBytesPerRow, colorSpace, CGImageAlphaInfo.PremultipliedFirst);
                if (context == null)
                {
                    throw new Exception("Context not created");
                }
                return context;
            }
        }
        //Store pixel data as an ARGB Bitmap
        protected IntPtr RequestImagePixelData(UIImage inImage)
        {
            var imageSize = inImage.Size;
            CGBitmapContext ctxt = CreateARGBBitmapContext(inImage.CGImage);
            var rect = new RectangleF(0.0f, 0.0f, imageSize.Width, imageSize.Height);
            ctxt.DrawImage(rect, inImage.CGImage);
            var data = ctxt.Data;
            return data;
        }

        public void RegisterResource(uint id, int resource)
        {
            _resources[id] = resource;
        }
    }
}