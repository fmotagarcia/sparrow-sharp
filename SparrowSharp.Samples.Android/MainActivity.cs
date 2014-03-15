using System.Diagnostics;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Sparrow.Core;
using System;
using Sparrow.Utils;
using SparrowSharp.Samples.Android;

namespace Sparrow.Samples.Android
{
	[Activity(Label = "sparrow-sharp",Name="awesome.demo.activity",
		#if __ANDROID_11__
		HardwareAccelerated=false,
		#endif
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        MainLauncher = true)]
    public class MainActivity : Activity
    {
		ViewController sparrowView;
		//AndroidSurfaceView sparrowView;
		public static global::Android.Content.Context ContextRef;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			ContextRef = this.BaseContext;

			sparrowView = new ViewController(this.ApplicationContext);
			//sparrowView = new AndroidSurfaceView (this);
			SetContentView(sparrowView);
			SP.Start(typeof(Benchmark));

			//TrigonometryTest test = new TrigonometryTest ();
			//string result = test.TestLUTAccuracy ();
			//Console.WriteLine (result);
		}

        protected override void OnPause()
        {
            base.OnPause();
			//sparrowView.OnPause ();
        }

        protected override void OnResume()
        {
			base.OnResume();
			//sparrowView.OnResume();
        }
    }
}