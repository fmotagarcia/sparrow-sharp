using System;
using Sparrow.Geom;
using OpenTK;
using System.Runtime.InteropServices;

namespace Sparrow.Utils
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex
	{
		public Vector2 Position;
		public Vector2 TexCoords;
		public VertexColor Color;
	}
}