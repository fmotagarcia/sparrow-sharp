using System;
using System.Drawing;
using System.Drawing.Imaging;
using Sparrow.Textures;
using System.IO;
using Sparrow.Core;
using System.Runtime.InteropServices;

namespace Sparrow.ResourceLoading
{
    public class TextureLoader // TODO makes this an interface
    {
        protected bool _isLoaded;
        protected Texture _glTexture;

        public bool IsLoaded { get { return _isLoaded; } }

        public Texture Texture { get { return _glTexture; } }

        public event EventHandler<Texture> ResourceLoaded;

        public TextureLoader LoadRemoteImage(string remoteUrl)
        {
            _isLoaded = false;
            return this; 
        }

        public Texture LoadLocalImage(string pathToFile)
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

        public Texture LoadFromStream(Stream stream)
        {
            _isLoaded = false;
            Bitmap bitmap = new Bitmap(stream);
            GenerateTexture(bitmap);
            return _glTexture;
        }

        private async void LoadLocalBitmapAsync(string path)
        {
            throw new NotImplementedException();
        }

        private void GenerateTexture(Bitmap bitmap)
        {
            _isLoaded = false;


            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );
            bitmap.UnlockBits(bitmapData);
            IntPtr rawData = bitmapData.Scan0;
            

            int length = bitmap.Width * bitmap.Height * 4;
            byte[] data = new byte[length];
            Marshal.Copy(rawData, data, 0, length);

            for (int i = 0; i < length; i += 4)
            {
                float alpha = (float)data[i + 3] / 255;
                byte r = data[i + 2];
                byte g = data[i + 1];
                byte b = data[i + 0];
                data[i + 0] = (byte)(r * alpha);
                data[i + 1] = (byte)(g * alpha);
                data[i + 2] = (byte)(b * alpha);
                data[i + 3] = (byte)(alpha * 255);
            }
            TextureOptions opts = new TextureOptions(TextureFormat.Rgba8888);

            _glTexture = Texture.FromData(data, opts, bitmapData.Width, bitmapData.Height);

            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler<Texture> handler = ResourceLoaded;
            handler?.Invoke(this, _glTexture);
        }

    }
}

