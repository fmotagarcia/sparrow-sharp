using OpenTK.Graphics.ES20;
using Sparrow.Textures;
using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Sparrow.ResourceLoading
{
    public class iOSTextureLoader : LoaderBase
    {
        public delegate void EventHandler(GLTexture resource,LoaderBase resourceLoader);

        public event EventHandler ResourceLoaded;

        protected GLTexture _resource;

        /// <summary>
        /// Loads an image based on resource ID, only avaiable on Andoid. 
        /// The resource will be scaled based on screen DPI, put it into the drawable-nodpi folder if you dont want this.
        /// </summary>
        public LoaderBase LoadAndroidResource(int AndroidResourceId)
        {
            _isLoaded = false;
            UIImage image = UIImage.FromBundle("star.png");
            GenerateTexture(image);
            return this;
        }

        #region implemented abstract members of LoaderBase

        protected override void DecodeRawResult(byte[] data)
        {
            throw new NotImplementedException();
        }

        #endregion

        //        override protected void DecodeRawResult(byte[] data)
        //        {
        //            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
        //            GenerateTexture(bitmap);
        //        }
        private void GenerateTexture(UIImage image)
        {
            uint name = (uint)GL.GenTexture();
            GL.BindTexture(All.Texture2D, name);
            GL.TexParameter(All.Texture2D, All.TextureMaxAnisotropyExt, 1);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.NearestMipmapNearest);
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.GenerateMipmapHint, (int)All.False);


            LoadTexture(All.Texture2D, image);

            // see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
            // for how MonoGame does it
            GLTexture texture = new GLTexture(name, image.CGImage.Width, image.CGImage.Height, false, 1.0f, false);
            _resource = texture;
            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = ResourceLoaded;
            if (handler != null)
            {
                handler(_resource, this);
            }
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

        public GLTexture GetResource()
        {
            return _resource;
        }
    }
}

