using Android.App;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;
using GLNativeES20;
using System;

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
		//ViewController sparrowView;
		AndroidSurfaceView sparrowView;
		public static global::Android.Content.Context ContextRef;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			ContextRef = this.BaseContext;
			//sparrowView = new ViewController(this.ApplicationContext);
			//SetContentView(sparrowView);
			//SP.Start(typeof(Benchmark));

			sparrowView = new AndroidSurfaceView (this);
			SetContentView (sparrowView);
			SP.Start(typeof(Benchmark));
        }

        protected override void OnPause()
        {
            base.OnPause();

			sparrowView.OnPause ();
        }

        protected override void OnResume()
        {
            base.OnResume();

			sparrowView.OnResume();
        }
    }
}