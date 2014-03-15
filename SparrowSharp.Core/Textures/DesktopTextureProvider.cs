using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.ES20;

namespace Sparrow.Textures
{
	public class DesktopTextureProvider : ITextureProvider
	{
		private readonly Dictionary<uint, string> _resources = new Dictionary<uint, string> ();

		public Texture CreateTexture (uint id)
		{
			string resourceID;

			if (_resources.TryGetValue (id, out resourceID)) {
				uint name = (uint)GL.GenTexture ();
				GL.BindTexture (All.Texture2D, name);

                Bitmap bitmap = new Bitmap(resourceID);
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

				GLTexture texture = new GLTexture (name, bitmap.Width, bitmap.Height, false, 1.0f, false);
				return texture;
			}

			return null;
		}

		public void RegisterResource (uint id, string resource)
		{
			_resources [id] = resource;
		}
	}
}