using System;
using System.Collections.Generic;
using Sparrow.Display;
using Sparrow;
using OpenTK.Graphics.ES20;
using Sparrow.Textures;

namespace Sparrow.Samples.Android
{
    public class SampleGame : DisplayObjectContainer
    {
        private static int NUMROWS = 3;
        private static int NUMCOLS = 10;
        private Quad[] _childQuads = new Quad[NUMCOLS * NUMROWS];

        public SampleGame()
        {
            SparrowSharpApp.Stage.Color = 0xFF0000;

            int cnt = 0;
            for (int i = 0; i < NUMCOLS; i++)
            {
                for (int j = 0; j < NUMROWS; j++)
                {
                    Quad childQuad = new Quad(15, 15);
                    childQuad.Color = 0x1212FF;
                    childQuad.X = 10 + i * 30;
                    childQuad.Y = 10 + j * 30;
                    //childQuad.SkewX = 0.3f;
                    //childQuad.SkewY = 0.4f;
                    //childQuad.Rotation = i / 10.0f;
                    AddChild(childQuad);
                    _childQuads[cnt] = childQuad;
                    cnt++;
                }
            }
            Random r = new Random();
            List<Image> images = new List<Image>();

			Texture tex = TextureFactory.CreateTexture ((uint) BenchmarkResources.Sparrow);

            for (int i = 0; i < 100; i++)
            {
                Image img = new Image(tex);
                AddChild(img);
                //img.Color = 0xFFFFFE;
                //img.Alpha = 0.98f;
                img.X = r.Next(0, 800);
                img.Y = r.Next(0, 800);
                img.PivotX = img.Width / 2f;
                img.PivotY = img.Height / 2f;

                images.Add(img);
            }
			
//            Added += (DisplayObject target, DisplayObject currentTarget) =>
//            {
//                target.Alpha = 0.5f;
//            };
//
//            AddedToStage += (DisplayObject target, DisplayObject currentTarget) =>
//            {
//                Stage stage = target as Stage;
//                if (stage != null)
//                {	
//                    stage.Color = 0x00FFCC;
//                }
//            };	
			
            EnterFrame += (target, currentTarget, passedTime) =>
            {
                foreach (Image image in images)
                {
                    image.Rotation += 0.05f;
                }
            };
        }

        void EnterFrameHandler(DisplayObject target, DisplayObject currentTarget, float passedTime)
        {
            /*for (int i = 0; i < _childQuads.Length; i++) {
				if (_childQuads [i] != null) {
					_childQuads [i].X = _childQuads [i].X + 1.0f;
					if (_childQuads [i].X > 700)
						_childQuads [i].X = 0;

				}
			}*/
        }
    }
}