using OpenTK.Graphics.ES20;
using Sparrow.Textures;
using System;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;
using MonoTouch.Foundation;

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

        public GLTexture LoadFromStream(Stream stream)
        {
            _isLoaded = false;
            NSData data = NSData.FromStream(stream);
            UIImage img = UIImage.LoadFromData (data);
            GenerateTexture(img);
            return _glTexture;
        }

		private void GenerateTexture(UIImage image)
		{
			uint name = (uint)GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, name);
//			GL.TexParameter(TextureTarget.Texture2D, All.TextureMaxAnisotropyExt, 1);
//			GL.TexParameter(TextureTarget.Texture2D, All.GenerateMipmapHint, (int)All.False);
			LoadTexture(image);

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

		protected void LoadTexture(UIImage inImage)
		{
			var pixelsWide = inImage.CGImage.Width;
			var pixelsHigh = inImage.CGImage.Height;
			var bitmapBytesPerRow = pixelsWide * 4;
			var bitmapByteCount = bitmapBytesPerRow * pixelsHigh;

            var bitmapData = Marshal.AllocHGlobal(bitmapByteCount);
			if (bitmapData == IntPtr.Zero)
			{
				throw new Exception("Memory not allocated.");
			}

			var context = new CGBitmapContext(bitmapData, pixelsWide, pixelsHigh, 8, bitmapBytesPerRow, 
                inImage.CGImage.ColorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);

			if (context == null)
			{
				throw new Exception("Context not created");
			}

			var imageSize = inImage.Size;
			var rect = new RectangleF(0.0f, 0.0f, imageSize.Width, imageSize.Height);
            context.ClearRect(rect);
			context.DrawImage(rect, inImage.CGImage);
			var data = context.Data;

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)imageSize.Width,
                (int)imageSize.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);

			Marshal.FreeHGlobal(data);
		}
	}
}

