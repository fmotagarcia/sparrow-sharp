using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sparrow.Display;
using Sparrow;

namespace sparrowsharp
{
	public class SampleGame : DisplayObjectContainer
	{
		private static int NUMROWS = 3;
		private static int NUMCOLS = 10;
		private Quad[] _childQuads = new Quad[NUMCOLS * NUMROWS];

		public SampleGame ()
		{
			SP.Stage.Color = 0xFF0000;

			int cnt = 0;
			for (int i = 0; i < NUMCOLS; i++) {
				for (int j = 0; j < NUMROWS; j++) {
					Quad childQuad = new Quad (15, 15);
					childQuad.Color = 0x1212FF;
					childQuad.X = 10 + i * 30;
					childQuad.Y = 10 + j * 30;
					//childQuad.SkewX = 0.3f;
					//childQuad.SkewY = 0.4f;
					//childQuad.Rotation = i / 10.0f;
					AddChild (childQuad);
					_childQuads [cnt] = childQuad;
					cnt++;
				}
			}

			Quad bigQuad = new Quad (36, 36, 0x23FF00);
				
			Sprite sp = new Sprite ();
			sp.X = 10;
			sp.Y = 100;
			//sp.Alpha = 0.9f;
			//sp.Rotation = 0.001f;
			sp.AddChild (bigQuad); // TODO not working
			AddChild (sp);

			EnterFrame += EnterFrameHandler;
		}

		void EnterFrameHandler (DisplayObject target, DisplayObject currentTarget, float passedTime)
		{
			for (int i = 0; i < _childQuads.Length; i++) {
				if (_childQuads [i] != null) {
					_childQuads [i].X = _childQuads [i].X + 1.0f;
					if (_childQuads [i].X > 700)
						_childQuads [i].X = 0;

				}
			}
		}
	}
}