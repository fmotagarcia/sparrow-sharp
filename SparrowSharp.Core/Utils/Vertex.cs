using OpenTK;
using System.Runtime.InteropServices;

namespace Sparrow.Utils
{
	[StructLayout (LayoutKind.Sequential)]
	public struct Vertex
	{
		public const int SIZE = 16;
		public const int POSITION_OFFSET = 0;
		public const int TEXTURE_OFFSET = 8;
		public Vector2 Position;
		public Vector2 TexCoords;

		public static unsafe void Copy (Vertex[] source, int sourceOffset, Vertex[] target, int targetOffset, int count)
		{
			// If either array is not instantiated, you cannot complete the copy. 
			if ((source == null) || (target == null)) {
				throw new System.ArgumentException ();
			}

			// If either offset, or the number of bytes to copy, is negative, you 
			// cannot complete the copy. 
			if ((sourceOffset < 0) || (targetOffset < 0) || (count < 0)) {
				throw new System.ArgumentException ();
			}

			// If the number of bytes from the offset to the end of the array is  
			// less than the number of bytes you want to copy, you cannot complete 
			// the copy.  
			if ((source.Length - sourceOffset < count) ||
			    (target.Length - targetOffset < count)) {
				throw new System.ArgumentException ();
			}

			// The following fixed statement pins the location of the source and 
			// target objects in memory so that they will not be moved by garbage 
			// collection. 
			fixed (Vertex* pSource = source, pTarget = target) {
				// Set the starting points in source and target for the copying. 
				Vertex* ps = pSource + sourceOffset;
				Vertex* pt = pTarget + targetOffset;

				// Copy the specified number of bytes from source to target. 
				for (int i = 0; i < count; i++) {
					*pt = *ps;
					pt++;
					ps++;
				}
			}
		}
	}
}