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
        }
    }
}