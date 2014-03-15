using System;
using System.Collections.Generic;

namespace Sparrow.Textures
{
	public class DesktopTextureProvider : ITextureProvider
	{
		private Dictionary<uint, string> _resources = new Dictionary<uint, string> ();

		public Texture CreateTexture (uint id)
		{
			string resourceID;

			if (_resources.TryGetValue (id, out resourceID)) {
				uint name = (uint)GL.GenTexture ();
				GL.BindTexture (All.Texture2D, name);

				Bitmap bitmap = new Bitmap("benchmark_object.png");
				BitmapData bitmapData = bitmap.LockBits(
					new Rectangle(0, 0, bmp.Width, bmp.Height), 
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

				GLTexture texture = new GLTexture (name, bitmap.Width, bitmap.Height, false, 1.0f, false);
				return texture;
			}

			return null;
		}

		public void RegisterResource (uint id, int resource)
		{
			_resources [id] = resource;
		}
	}
}