using System.Diagnostics;
using Android.App;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Sparrow.Core;
using System;
using Sparrow.Utils;
using SparrowSharp.Samples.Android;
using Sparrow.Textures;

namespace Sparrow.Samples.Android
{
    [Activity(Label = "sparrow-sharp", Name = "awesome.demo.activity",
		#if __ANDROID_11__
		HardwareAccelerated=false,
		#endif
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        MainLauncher = true)]
    public class MainActivity : Activity
    {
        AndroidViewController sparrowView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

			sparrowView = new AndroidViewController(this.BaseContext);
            SetContentView(sparrowView);
            SP.Start(typeof(Benchmark));
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