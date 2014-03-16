using OpenTK;
using System.Runtime.InteropServices;

namespace Sparrow.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
		public const int SIZE = 16;
		public const int POSITION_OFFSET = 0;
		public const int TEXTURE_OFFSET = 8;

        public Vector2 Position;
        public Vector2 TexCoords;
    }
}