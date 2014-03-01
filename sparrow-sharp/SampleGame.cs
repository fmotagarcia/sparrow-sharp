using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sparrow.Display;
using Sparrow;

namespace sparrowsharp
{
    class SampleGame : DisplayObjectContainer
    {
        public SampleGame()
        {
			SP.Stage.Color = 0xFF0000;

            Quad quad = new Quad(100, 100);
			quad.Color = 0x00FF00;
            quad.X = 50;
            quad.Y = 50;
            AddChild(quad);
        }
    }
}