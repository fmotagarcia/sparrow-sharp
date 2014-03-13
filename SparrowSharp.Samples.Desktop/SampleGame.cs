using System;
using System.Collections.Generic;
using Sparrow.Display;
using Sparrow;
using OpenTK.Graphics.ES20;
using Sparrow.Core;
using System.Drawing;
using System.Drawing.Imaging;

namespace Sparrow.Samples.Desktop
{
	public class SampleGame : DisplayObjectContainer
	{
		Texture _texture;
		Sprite _container;
		int _frameCount = 0;
		float _elapsed = 0;
		bool _started = false;
		int _failCount = 0;
		int _waitFrames = 0;

		public SampleGame ()
		{
			_texture = createTexture ();

			// the container will hold all test objects
			_container = new Sprite ();
			AddChild (_container);

			EnterFrame += EnterFrameHandler;
			AddedToStage += AddedToStageHandler;
		}

		Texture createTexture ()
		{
			uint name = (uint)GL.GenTexture ();
			GL.BindTexture (All.Texture2D, name);

			Bitmap bmp = new Bitmap("benchmark_object.png");
			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), 
						ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget2d.Texture2D, 0,TextureComponentCount.Rgba, bmp_data.Width, bmp_data.Height, 0,
				OpenTK.Graphics.ES20.PixelFormat.Rgba, PixelType.UnsignedByte, bmp_data.Scan0);
			// was brga
			bmp.UnlockBits(bmp_data);

			GLTexture tex = new GLTexture (name, bmp.Width, bmp.Height, false, 1.0f, false);

			return tex;
		}

		void AddTestObjects (int numObjects)
		{
			int border = 15;

			Random r = new Random ();
			for (int i = 0; i < numObjects; ++i) {   
				Sparrow.Display.Image egg = new Sparrow.Display.Image (_texture);
				egg.X = r.Next (border, (int)Stage.Width - border);
				egg.Y = r.Next (border, (int)Stage.Height - border);
				egg.Rotation = (float)(r.Next (0, 100) / 100 * Math.PI);
				_container.AddChild (egg);
			}
		}

		void BenchmarkComplete ()
		{
			_started = false;

			Console.WriteLine ("benchmark complete!");
			Console.WriteLine ("number of objects: " + _container.NumChildren);
		}

		void AddedToStageHandler (DisplayObject target, DisplayObject currentTarget)
		{
			_started = true;
			_waitFrames = 30;
			AddTestObjects (15000);
		}

		void EnterFrameHandler (DisplayObject target, DisplayObject currentTarget, float passedTime)
		{
			if (!_started)
				return;

			_elapsed += passedTime / 1000;
			++_frameCount;

			if (_frameCount % _waitFrames == 0) {
				float targetFPS = 60;
				float realFPS = _waitFrames / _elapsed;
                Console.WriteLine("FPS:" + realFPS);
				/*if (realFPS >= targetFPS) {
					int numObjects = _failCount != 0 ? 5 : 25;
					AddTestObjects (numObjects);
					_failCount = 0;
				} else {
					++_failCount;

					if (_failCount > 15)
						_waitFrames = 5; // slow down creation process to be more exact
					if (_failCount > 20)
						_waitFrames = 10;
					if (_failCount == 25)
						BenchmarkComplete (); // target fps not reached for a while
				}*/

				_elapsed = _frameCount = 0;
			}

			for (int i = 0; i < _container.NumChildren; i++) {
				DisplayObject child = _container.GetChild (i);
				child.Rotation += 0.05f;    
			}
		}
	}
}