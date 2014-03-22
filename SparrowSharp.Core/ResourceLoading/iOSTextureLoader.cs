using OpenTK.Graphics.ES20;
using Sparrow.Textures;
using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Sparrow.ResourceLoading
{
    public class TextureLoader
    {
        protected bool _isLoaded = false;
        protected GLTexture _glTexture;
        public bool IsLoaded { get { return _isLoaded; } }
        public GLTexture Texture { get { return _glTexture; } }
        public event EventHandler<GLTexture> ResourceLoaded;

        public TextureLoader LoadRemoteImage(string remoteURL)
        {
            _isLoaded = false;
            LoadRemoteImageAsync(remoteURL);
            return this;
        }

        private async void LoadRemoteImageAsync(string url)
        { 
        }

        public GLTexture LoadLocalImage(string pathToFile)
        {
            _isLoaded = false;
            UIImage image = UIImage.FromBundle(pathToFile);
            GenerateTexture(image);
            return _glTexture;
        }

        public TextureLoader LoadLocalImageAsync(string pathToFile)
        {
            _isLoaded = false;
            LoadLocalBitmapAsync(pathToFile);
            // TODO check wether the async call can be executed instantly, 
            // because in that case it will be impossible to cacth the event
            return this;
        }

        private async void LoadLocalBitmapAsync(string path)
        {
        }

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
            _glTexture = new GLTexture(name, image.CGImage.Width, image.CGImage.Height, false, 1.0f, false);
            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler<GLTexture> handler = ResourceLoaded;
            if (handler != null)
            {
                handler(this, _glTexture);
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

    }
}

