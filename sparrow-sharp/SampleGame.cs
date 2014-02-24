using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK.Platform.Android;
using Sparrow.Display;

namespace sparrowsharp
{
    class SampleGame : Sprite
    {
         
        public void SampleGame()
		{
            Quad quad = new Quad(100, 100);
            quad.Color = 0xff0000;
            quad.X = 50;
            quad.Y = 50;
            AddChild(quad);
		}
	
    }
}