using System;
using System.Collections.Generic;
using Sparrow.Display;
using Sparrow;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Textures;
using Android.Opengl;
using Android.App;
using Android.Widget;
using SparrowSharp.Display;
using Sparrow.ResourceLoading;
using Sparrow.Core;
using SparrowSharp.Filters;

namespace Sparrow.Samples.Android
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
            GLTexture star = SimpleTextureLoader.LoadAndroidResource(SparrowSharp.Samples.Android.Resource.Drawable.star);
            GLTexture bird = SimpleTextureLoader.LoadAndroidResource(SparrowSharp.Samples.Android.Resource.Drawable.benchmark_object);
            GLTexture bigstar = SimpleTextureLoader.LoadAndroidResource(SparrowSharp.Samples.Android.Resource.Drawable.bigstar);
            textures = new Texture[] { bird, bigstar, star };


            // the container will hold all test objects
            _container = new Sprite();
            AddChild(_container);

            EnterFrame += EnterFrameHandler;
            AddedToStage += AddedToStageHandler;
        }

        private void AddTestObjects(int numObjects)
        {
            int border = 15;

            Random r = new Random();
            for (int i = 0; i < numObjects; ++i)
            {   
                Image egg = new Image(textures[1]);
                //MovieClip egg = new MovieClip (textures, 3);
                //SparrowSharpApp.DefaultJuggler.Add (egg);
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

            Toast.MakeText(AndroidViewController.AndroidContext, "number of objects: " + _container.NumChildren, ToastLength.Long).Show();

            _started = false;
            _container.RemoveAllChildren();
        }

        private void AddedToStageHandler(DisplayObject target, DisplayObject currentTarget)
        {
            /*
			RenderTexture tex = new RenderTexture (400, 300, 0xf1ff00ff);
			tex.DrawBundled(delegate
				{
					for (int i=0; i<12; ++i)
					{
						Image img = new Image(textures[i%3]);
						//img.Rotation = (2 * (float)Math.PI / 12) * i;
						img.X = i * 30;
						tex.DrawObject(img);            
					}             
				});
			AddChild (new Image(tex));*/
            _started = true;
            _waitFrames = 3;
            AddTestObjects(1);
        }

        private void EnterFrameHandler(DisplayObject target, float passedTime)
        {
            if (!_started)
                return;

            _elapsed += passedTime / 1000;
            ++_frameCount;

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

            for (int i = 0; i < _container.NumChildren; i++)
            {
                DisplayObject child = _container.GetChild(i);
                child.Rotation += 0.05f;    
            } 
        }
    }
}