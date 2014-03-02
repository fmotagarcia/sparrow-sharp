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
        public SampleGame()
        {
			SP.Stage.Color = 0xFF0000;

			Quad quad = new Quad(640, 384);
			quad.Color = 0x00FF00;
            AddChild(quad);
        }
    }
}