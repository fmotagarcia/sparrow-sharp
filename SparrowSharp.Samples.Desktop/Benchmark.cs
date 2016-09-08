using System;
using Sparrow.Display;
using Sparrow.Textures;
using Sparrow.ResourceLoading;
using Sparrow.Core;
using SparrowSharp.Filters;
using SparrowSharp.Core.Rendering;

namespace Sparrow.Samples
{
    public class Benchmark : Sprite
    {
        private Sprite _container;
        private int _frameCount = 0;
        private float _elapsed = 0;
        private bool _started = false;
        private int _failCount = 0;
        private int _waitFrames = 0;
        private Texture[] textures;

        public Benchmark()
        {
            /*GLTexture star = SimpleTextureLoader.LoadLocalImage("../../bigstar.png");
            GLTexture bird = SimpleTextureLoader.LoadLocalImage("../../benchmark_object.png");
            textures = new Texture[] { bird, star };

            // the container will hold all test objects
            _container = new Sprite();
            AddChild(_container);

            EnterFrame += EnterFrameHandler;
            AddedToStage += AddedToStageHandler;*/

            Quad q = new Quad(76, 127, 0x3faf2f);
            AddChild(q);
        }

        private void AddedToStageHandler(DisplayObject target, DisplayObject currentTarget)
        {
            _started = true;
            _waitFrames = 3;
            AddTestObjects(40);

            SparrowSharpApp.ShowStats = true;
        }

        private void AddTestObjects(int numObjects)
        {
            int border = 15;

            /*Random r = new Random();
            for (int i = 0; i < numObjects; ++i)
            {   
                Image egg = new Image(textures[0]);
                if (i < 5)
                {
                    ColorMatrix cm = new ColorMatrix();
                    cm.AdjustSaturation(-0.8f);
                    ColorMatrixFilter fi = new ColorMatrixFilter (cm);
                    //EmptyFilter fi = new EmptyFilter();
                    //BlurFilter fi = new BlurFilter(4, 1.1f);
                    egg.Filter = fi;
                    //egg.Filter.Cache();
                }
                //MovieClip egg = new MovieClip (textures, 3);
                //SP.DefaultJuggler.Add (egg);
                egg.X = r.Next(border, (int)Stage.Width - border);
                egg.Y = r.Next(border, (int)Stage.Height - border);
                egg.Rotation = (float)(r.Next(0, 100) / 100.0f * Math.PI);
                _container.AddChild(egg);
            }

            Sprite sp = new Sprite();
            sp.X = sp.Y = 250;
            _container.AddChild(sp);

            Image test = new Image(textures[1]);
            test.PivotX = test.PivotY = test.Width / 2;
            sp.AddChild(test);

            Image test1 = new Image(textures[1]);
            sp.AddChild(test1);
            test1.X = test1.Y = 60;*/
        }

        private void BenchmarkComplete()
        {
            Console.WriteLine("benchmark complete!");
            Console.WriteLine("number of objects: " + _container.NumChildren);

            _started = false;
            _container.RemoveAllChildren();
        }

        private void EnterFrameHandler(DisplayObject target, float passedTime)
        {
            if (!_started)
                return;
            
            _elapsed += passedTime / 1000;
            ++_frameCount;
            /*
            if (_frameCount % _waitFrames == 0)
            {
                float targetFPS = 60;
                float realFPS = _waitFrames / _elapsed;
                //Console.WriteLine ("FPS: " + realFPS);
                if (realFPS >= targetFPS)
                {
                    int numObjects = _failCount != 0 ? 5 : 25;
                    AddTestObjects(numObjects);
                    _failCount = 0;
                }
                else
                {
                    ++_failCount;

                    if (_failCount > 15)
                        _waitFrames = 5; // slow down creation process to be more exact
                    if (_failCount > 20)
                        _waitFrames = 10;
                    if (_failCount == 25)
                        BenchmarkComplete(); // target fps not reached for a while
                }

                _elapsed = _frameCount = 0;
            }
*/
            for (int i = 0; i < _container.NumChildren; i++)
            {
                DisplayObject child = _container.GetChild(i);
                child.Rotation += 0.05f;    
            } 
        }
    }
}