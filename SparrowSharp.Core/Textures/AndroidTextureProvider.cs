using System;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Android.Opengl;
using System.Collections.Generic;
using Android;

namespace Sparrow.Textures
{
	public class AndroidTextureProvider : ITextureProvider
	{
		private readonly global::Android.Content.Context _context;
		private Dictionary<uint, int> _resources = new Dictionary<uint, int> ();

		public AndroidTextureProvider (global::Android.Content.Context context)
		{
			_context = context;
		}

		public Texture CreateTexture (uint id)
		{
			int resourceID;

			if (_resources.TryGetValue (id, out resourceID)) {
				uint name = (uint)GL.GenTexture ();
				GL.BindTexture (All.Texture2D, name);

				Bitmap bitmap = BitmapFactory.DecodeResource (_context.Resources, resourceID);
				GLUtils.TexImage2D (GLES20.GlTexture2d, 0, bitmap, 0);

				// see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
				// for how MonoGame does it
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