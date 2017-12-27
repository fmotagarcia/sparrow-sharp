using Sparrow.Textures;
using System.IO;

namespace Sparrow.ResourceLoading
{
    public static class SimpleTextureLoader
    {
        public static TextureLoader LoadRemoteImage(string remoteURL)
        {
            return new TextureLoader().LoadRemoteImage(remoteURL);
        }

        public static Texture LoadLocalImage(string pathToFile)
        {
            return new TextureLoader().LoadLocalImage(pathToFile);
        }

        public static TextureLoader LoadLocalImageAsync(string pathToFile)
        {
            return new TextureLoader().LoadLocalImageAsync(pathToFile);
        }

        public static Texture LoadImageFromStream(Stream stream)
        {
            return new TextureLoader().LoadFromStream(stream);
        }
    }
}

