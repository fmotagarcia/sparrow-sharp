using System;
using OpenTK.Graphics.ES20;
using System.Drawing;
using System.Drawing.Imaging;
using Sparrow.Textures;
using System.IO;

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
            return this; 
        }

        public GLTexture LoadLocalImage(string pathToFile)
        {
            _isLoaded = false;
            GenerateTexture(new Bitmap(pathToFile));
            return _glTexture;
        }

        public TextureLoader LoadLocalImageAsync(string pathToFile)
        {
            _isLoaded = false;
            LoadLocalBitmapAsync(pathToFile);
            // TODO check wether the async call can be executed instantly, 
            // because in that case it will be impossible to catch the event
            return this; 
        }

        public GLTexture LoadFromStream(Stream stream)
        {
            _isLoaded = false;
            Bitmap bitmap = new Bitmap(stream);
            GenerateTexture(bitmap);
            return _glTexture;
        }

        private async void LoadLocalBitmapAsync(string path)
        {

        }

        private void GenerateTexture(Bitmap bitmap)
        {
            _isLoaded = false;
            uint name = (uint)GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, name);

            BitmapData bitmapData = bitmap.LockBits(
                               new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                               ImageLockMode.ReadOnly, 
                               System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(
                TextureTarget2d.Texture2D, 
                0,
                TextureComponentCount.Rgba, 
                bitmapData.Width, 
                bitmapData.Height, 
                0,
                OpenTK.Graphics.ES20.PixelFormat.Rgba, 
                PixelType.UnsignedByte, 
                bitmapData.Scan0);
            // was brga
            bitmap.UnlockBits(bitmapData);

            _glTexture = new GLTexture(name, bitmap.Width, bitmap.Height, false, 1.0f, false);

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
    }
}

