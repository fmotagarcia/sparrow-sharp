using System;
using Sparrow.Textures;

namespace Sparrow.ResourceLoading
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

		GLTexture GetResource ();

		bool IsLoaded { get; }

		event EventHandler<GLTexture> ResourceLoaded;

		// these are not needed for the interface, just serve as a safety that the 
		// implementations have the same base API on each platform.
		void LoadRemoteImage (string remoteURL);

		GLTexture LoadLocalImage (string pathToFile);

		void LoadLocalImageAsync (string pathToFile);


	}
}

