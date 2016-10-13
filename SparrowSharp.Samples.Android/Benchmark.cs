using System;
using Sparrow.Display;
using Sparrow.Textures;
using Sparrow.ResourceLoading;
using Sparrow.Core;
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

        public Benchmark()
        {
            SparrowSharp.EnableErrorChecking();
            textures = new Texture[] { SimpleTextureLoader.LoadAndroidResource(Resource.Drawable.bigstar),
                                       SimpleTextureLoader.LoadAndroidResource(Resource.Drawable.benchmark_object) };

            // the container will hold all test objects
            _container = new Sprite();
            AddChild(_container);

            EnterFrame += EnterFrameHandler;
            AddedToStage += AddedToStageHandler;
            //SparrowSharp.SkipUnchangedFrames = true;
            SparrowSharp.Stage.Color = 0x432323;
        }
        private void AddedToStageHandler(DisplayObject target, DisplayObject currentTarget)
        {

            _started = true;
            _waitFrames = 3;

            SparrowSharp.ShowStats(HAlign.Right, VAlign.Bottom, 2f);


            AddTestObjects(16);

            TestRenderTexture();

            TestFilters();

            TestQuad();

            TestMovieClip();

            TestTextField();

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

        int cnt = 0;
        private void EnterFrameHandler(DisplayObject target, float passedTime)
        {
            cnt++;

            //Console.WriteLine("COUNT: " + cnt);
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
                child.Rotation += 0.01f;
            }
        }

        private void TestMovieClip()
        {
            List<Texture> li = new List<Texture>();
            li.Add(textures[0]);
            li.Add(textures[1]);
            MovieClip mc = new MovieClip(li, 3);
            AddChild(mc);
            mc.X = mc.Y = 444;
            SparrowSharp.DefaultJuggler.Add(mc);
        }

        private void TestTextField()
        {
            TextField tf = new TextField(160, 100, "This is a TextField");
            tf.Format.Size = 32;
            tf.Border = true;
            AddChild(tf);
            tf.Y = 85;
            tf.X = 45;
        }

        private void TestQuad()
        {
            for (int i = 0; i < 10; i++)
            {
                Quad q1 = new Quad(40, 36, 0xff0000);
                _container.AddChild(q1);
                q1.X = i * 30;
                q1.Y = 5;
                q1.Alpha = i / 10f;
            }

        }

        private void TestRenderTexture()
        {
            RenderTexture renderTexture = new RenderTexture(300, 200);
            renderTexture.Draw(new Quad(155, 155, 0xFF00FF));
            Image toDraw = new Image(textures[1]);
            toDraw.X = 35;
            toDraw.Y = 15;
            toDraw.Rotation = 1;
            renderTexture.Draw(toDraw);

            Image rtImage = new Image(renderTexture);
            AddChild(rtImage);

            rtImage.X = 123;
            rtImage.Y = 198;
            rtImage.Rotation = 1;
        }

        private void TestFilters()
        {
            Image im = new Image(textures[0]);
            ColorMatrixFilter fi = new ColorMatrixFilter();
            fi.AdjustSaturation(-0.99f);
            im.Filter = fi;
            im.X = 225;
            im.Y = 110;
            im.Rotation = 0.2f;
            _container.AddChild(im);

            Image im2 = new Image(textures[0]);
            BlurFilter fi2 = new BlurFilter(5, 1);
            fi2.AlwaysDrawToBackBuffer = true;
            im2.Filter = fi2;
            im2.X = 525;
            im2.Y = 210;
            im2.Rotation = 0.2f;
            _container.AddChild(im2);
        }
    }
}