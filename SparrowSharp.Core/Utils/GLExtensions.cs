using System;
using OpenTK.Graphics.ES20;

namespace Sparrow.Utils
{
	public static class GLExtensions
	{
		public readonly static bool MapBufferSupported = false;

		static GLExtensions ()
		{
			string extensions = GL.GetString (All.Extensions);

			MapBufferSupported = extensions.IndexOf ("GL_OES_mapbuffer") >= 0;
		}
	}
}