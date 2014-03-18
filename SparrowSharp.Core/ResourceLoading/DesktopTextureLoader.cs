using System;
using OpenTK.Graphics.ES20;
using System.Drawing;
using System.Drawing.Imaging;
using Sparrow.Textures;

namespace Sparrow.ResourceLoading
{
	public class TextureLoader : LoaderBase
	{

		public delegate void EventHandler (GLTexture resource, LoaderBase resourceLoader);
		public event EventHandler ResourceLoaded;
		protected GLTexture _resource;

		override public LoaderBase LoadLocalResource (string pathToFile) {
			_isLoaded = false;
			uint name = (uint)GL.GenTexture ();
			GL.BindTexture (TextureTarget.Texture2D, name);
			Bitmap bitmap = new Bitmap(pathToFile);
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

			_resource = new GLTexture (name, bitmap.Width, bitmap.Height, false, 1.0f, false);

			_isLoaded = true;
			// Make a temporary copy of the event to avoid possibility of 
			// a race condition if the last subscriber unsubscribes 
			// immediately after the null check and before the event is raised.
			EventHandler handler = ResourceLoaded;
			if (handler != null) {
				handler (_resource, this);
			}
			return this;
		}

		override protected void DecodeRawResult(byte[] data) 
		{
			throw new Exception ("Not implemented!");
		}

		public GLTexture GetResource () {return _resource;}
	}
}

