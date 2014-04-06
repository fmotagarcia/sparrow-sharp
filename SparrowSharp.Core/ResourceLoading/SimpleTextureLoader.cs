using Sparrow.Textures;

namespace Sparrow.ResourceLoading
{
	public static class SimpleTextureLoader
	{
		public static TextureLoader LoadRemoteImage (string remoteURL)
		{
			return new TextureLoader ().LoadRemoteImage (remoteURL);
		}

		public static GLTexture LoadLocalImage (string pathToFile)
		{
			return new TextureLoader ().LoadLocalImage (pathToFile);
		}

		public static TextureLoader LoadLocalImageAsync (string pathToFile)
		{
			return new TextureLoader ().LoadLocalImageAsync (pathToFile);
		}
        #if __ANDROID__
		public static GLTexture LoadAndroidResource(int AndroidResourceId)
		{
			return new TextureLoader ().LoadAndroidResource (AndroidResourceId);
		}
        #endif

	}
}

