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

		public Quad childQuad;

        public SampleGame()
        {
			SP.Stage.Color = 0xFF0000;

			childQuad = new Quad(640, 384);
			childQuad.Color = 0x00FF00;
			childQuad.X = 100;
			childQuad.Y = 100;
			//childQuad.SkewX = 0.3f;
			//childQuad.SkewY = 0.4f;
			childQuad.Rotation = 0.75f;
			AddChild(childQuad);

			Quad quad2 = new Quad(123, 234);
			quad2.Color = 0x1400FF;
			quad2.X = 100;
			quad2.Y = 100;
			quad2.Alpha = 0.5f;
			quad2.Rotation = 0;
			AddChild(quad2);
        }
    }
}