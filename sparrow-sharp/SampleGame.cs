using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sparrow.Display;

namespace sparrowsharp
{
    class SampleGame : DisplayObjectContainer
    {
        public SampleGame()
        {
            Quad quad = new Quad(100, 100);
            quad.Color = 0xff0000;
            quad.X = 50;
            quad.Y = 50;
            AddChild(quad);
        }
    }
}