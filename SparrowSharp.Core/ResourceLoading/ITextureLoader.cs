using System;
using Sparrow.Textures;

namespace SparrowSharp.Core.Android
{
	public interface ITextureLoader
	{
		// Raw loading is not neeeded for 1.0 IMO, there are way too many possibilities how
		// a byteArray can represent image data (file formats, raw pixel arrays, 2D arrays,...)
		// And if someone really needs it they can upload whatever the want to the GPU
		// and make a GLTexture manually.

		// Since each platform provides some easy method to download and decode images 
		// syncronously and asynronously (e.g. BitmapFacotry in Android)
		// we should use these. Plus I think they are faster than manually downloading 
		// and passing the bytearray to some decoder.

		void LoadRemoteResource (string remoteURL);

		GLTexture LoadLocalResource (string pathToFile);

		void LoadLocalResourceAsync (string pathToFile);

		GLTexture GetResource ();

		bool IsLoaded { get; }

		event EventHandler<GLTexture> ResourceLoaded;
	}
}

