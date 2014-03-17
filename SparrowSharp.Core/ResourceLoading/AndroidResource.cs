using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Textures;

namespace Sparrow.ResourceLoading
{
	public class AndroidResource : Resource
	{

		public static global::Android.Content.Context _context;

		public Resource LoadAndroidResource(int AndroidResourceId, ResourceType resType) 
		{
			_resourceType = resType;
			if (_resourceType == ResourceType.IMAGE) { // TODO Can the resources folder store anything else?
				Bitmap bitmap = BitmapFactory.DecodeResource (_context.Resources, AndroidResourceId);
				GenerateTexture (bitmap);
			}
			return this;
		}

		override protected void DecodeImage(byte[] data) 
		{
			Bitmap bitmap = BitmapFactory.DecodeByteArray (data, 0, data.Length);
			GenerateTexture (bitmap);
		}

		private void GenerateTexture(Bitmap bitmap) {
			uint name = (uint)GL.GenTexture ();
			GL.BindTexture (All.Texture2D, name);
			GL.TexParameter (All.Texture2D, All.TextureMaxAnisotropyExt, 1);
			GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int) All.NearestMipmapNearest);
			GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int) All.Linear);
			GL.TexParameter (All.Texture2D, All.GenerateMipmapHint, (int) All.False);

			// see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
			// for how MonoGame does it
			GLTexture texture = new GLTexture (name, bitmap.Width, bitmap.Height, false, 1.0f, false);
			_resource = texture;
			_isLoaded = true;

			InvokeComplete ();
		}

	}
}

