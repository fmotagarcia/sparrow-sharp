using System;
using System.Collections.Generic;
using Sparrow.Display;
using Sparrow;
using OpenTK.Graphics.ES20;
using Sparrow.Core;

namespace Sparrow.Samples.Desktop
{
    public class SampleGame : DisplayObjectContainer
    {
		private Quad[] _childQuads = new Quad[20000];

        public SampleGame()
        {
            SP.Stage.Color = 0xFF0000;
			Random rnd = new Random();
            int cnt = 0;
			for (int i = 0; i < _childQuads.Length; i++) {
				Quad childQuad = new Quad(35, 35);
                childQuad.Color = 0x1212FF;
				childQuad.X = rnd.Next(600);
				childQuad.Y = rnd.Next(500);
				childQuad.Alpha = rnd.Next (10) / 100.0f;
                AddChild(childQuad);
                _childQuads[cnt] = childQuad;
                cnt++;
            }
            List<Image> images = new List<Image>();
			
			EnterFrame += EnterFrameHandler;
        }

        void EnterFrameHandler(DisplayObject target, DisplayObject currentTarget, float passedTime)
        {
            for (int i = 0; i < _childQuads.Length; i++) {
				if (_childQuads [i] != null) {
					_childQuads [i].Rotation = _childQuads [i].Rotation + 0.01f;
				}
			}
        }
    }
}