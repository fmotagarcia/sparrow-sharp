using System;
using System.Drawing;
using System.Drawing.Imaging;
using Sparrow.Textures;
using System.IO;

namespace Sparrow.ResourceLoading
{
    public class TextureLoader
    {
        protected bool _isLoaded = false;
        protected Texture _glTexture;

        public bool IsLoaded { get { return _isLoaded; } }

        public Texture Texture { get { return _glTexture; } }

        public event EventHandler<Texture> ResourceLoaded;

        public TextureLoader LoadRemoteImage(string remoteURL)
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
           
            // Fix up the Image to match the expected format
            bitmap = RGBToBGR(bitmap);

            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height), 
                ImageLockMode.ReadOnly, 
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            
            TextureOptions opts = new TextureOptions(TextureFormat.Rgba8888, 
                                         SparrowSharpApp.ContentScaleFactor, 0, false);
            bitmap.UnlockBits(bitmapData);
            _glTexture = Texture.FromData(bitmapData.Scan0, opts, bitmapData.Width, bitmapData.Height);
            
            /*
            uint name = (uint)GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, name);

            OpenTK.Graphics.OpenGL4.GL.TexStorage2D(OpenTK.Graphics.OpenGL4.TextureTarget2d.Texture2D,
               1, // mipmap level
               OpenTK.Graphics.OpenGL4.SizedInternalFormat.Rgba8,
               bitmapData.Width,
               bitmapData.Height);

            OpenTK.Graphics.OpenGL4.GL.TexSubImage2D(OpenTK.Graphics.OpenGL4.TextureTarget.Texture2D,
                0, // level
                0, // xOffset
                0, // yOffset
                bitmapData.Width, 
                bitmapData.Height,
                OpenTK.Graphics.OpenGL4.PixelFormat.Rgba,
                OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte,
                bitmapData.Scan0);
           
            bitmap.UnlockBits(bitmapData);
            _glTexture = new GLTexture(name, bitmap.Width, bitmap.Height, false, 1.0f, false);
            */

            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler<Texture> handler = ResourceLoaded;
            if (handler != null)
            {
                handler(this, _glTexture);
            }
        }

        // from https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/ImageEx.cs
        // RGB to BGR convert Matrix
        private static float[][] rgbtobgr = new float[][]
        {
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
        };

        private static Bitmap RGBToBGR(Bitmap bmp)
        {
            Bitmap newBmp;
            if ((bmp.PixelFormat & System.Drawing.Imaging.PixelFormat.Indexed) != 0)
            {
                newBmp = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }
            else
            {
                newBmp = bmp;
            }

            try
            {
                ImageAttributes ia = new ImageAttributes();
                ColorMatrix cm = new ColorMatrix(rgbtobgr);

                ia.SetColorMatrix(cm);
                using (Graphics g = Graphics.FromImage(newBmp))
                {
                    g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, ia);
                }
            }
            finally
            {
                if (newBmp != bmp)
                {
                    bmp.Dispose();
                }
            }

            return newBmp;
        }

    }
}

