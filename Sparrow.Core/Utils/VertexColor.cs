using System;
using System.Runtime.InteropServices;

namespace Sparrow.Utils
{
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexColor
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;
	}
}