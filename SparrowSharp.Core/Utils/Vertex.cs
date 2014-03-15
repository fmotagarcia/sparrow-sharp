using OpenTK;
using System.Runtime.InteropServices;

namespace Sparrow.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
		public const int SIZE = 20;
		public const int POSITION_OFFSET = 0;
		public const int TEXTURE_OFFSET = 8;
		public const int COLOR_OFFSET = 20;

        public Vector2 Position;
        public Vector2 TexCoords;
        public VertexColor Color;
    }
}