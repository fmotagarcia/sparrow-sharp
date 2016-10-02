using System;
using Sparrow.Display;
using Sparrow.Textures;
using Sparrow.ResourceLoading;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Text;
using Sparrow.Utils;
using System.Collections.Generic;
using Sparrow.Filters;

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

        public TextField eee;

        public Benchmark()
        {
            SparrowSharp.EnableErrorChecking();
            Texture star = SimpleTextureLoader.LoadLocalImage("bigstar.png");
            Texture bird = SimpleTextureLoader.LoadLocalImage("benchmark_object.png");
            textures = new Texture[] { star, bird };
            
            // the container will hold all test objects
            _container = new Sprite();
            AddChild(_container);

            EnterFrame += EnterFrameHandler;
            AddedToStage += AddedToStageHandler;
            //SparrowSharpApp.SkipUnchangedFrames = true;
            SparrowSharp.Stage.Color = 0x432323;
        }

        private void AddedToStageHandler(DisplayObject target, DisplayObject currentTarget)
        {
            SparrowSharp.ShowStats(HAlign.Right, VAlign.Bottom, 2f);
            _started = true;
            _waitFrames = 3;

            AddTestObjects(16);

            TextField tf = new TextField(100, 100, "abcdefg");
            tf.Format.Size = 32;
            tf.Border = true;
            AddChild(tf);
            tf.Y = 85;
            tf.X = 45;
           
            Image im = new Image(textures[0]);
            //im.Scale9Grid = Rectangle.Create(68, 0, 55, 128);
            //im.Width = 256;
            im.Filter = new BlurFilter();
            im.X = 523;
            im.Y = 23;
            _container.AddChild(im);
            
            for (int i = 0; i < 10; i++)
            {
                Quad q1 = new Quad(40, 36, 0xff0000);
                _container.AddChild(q1);
                q1.X = i * 30;
                q1.Y = 5;
                q1.Alpha = i / 10f;
            }

            List<Texture> li = new List<Texture>();
            li.Add(textures[0]);
            li.Add(textures[1]);
            MovieClip mc = new MovieClip(li, 3);
            AddChild(mc);
            mc.X = mc.Y = 444;
            SparrowSharp.DefaultJuggler.Add(mc);
        }

        private void AddTestObjects(int numObjects)
        {
            
            int border = 40;

            Random r = new Random();
            for (int i = 0; i < numObjects; ++i)
            {   
                Quad egg = new Quad();
                egg.Texture = textures[1];
                egg.Width = textures[1].Width;
                egg.Height = textures[1].Height;
                egg.AlignPivot(HAlign.Center, VAlign.Center);
                 if (i < 5)
                 {
                     //ColorMatrix cm = new ColorMatrix();
                     //cm.AdjustSaturation(-0.8f);
                     //ColorMatrixFilter fi = new ColorMatrixFilter (cm);
                     //EmptyFilter fi = new EmptyFilter();
                     //BlurFilter fi = new BlurFilter(4, 1.1f);
                     //egg.Filter = fi;
                     //egg.Filter.Cache();
                 }
                //MovieClip egg = new MovieClip (textures, 3);
                //SP.DefaultJuggler.Add (egg);
                egg.X = r.Next(border, (int)Stage.Width - border);
                egg.Y = r.Next(border, (int)Stage.Height - border);
                egg.Rotation = (float)(r.Next(0, 100) / 100.0f * Math.PI);
                _container.AddChild(egg);
            }
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