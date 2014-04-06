using Android.App;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;

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
        private AndroidViewController sparrowView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            sparrowView = new AndroidViewController(BaseContext, (width, height) => SparrowSharpApp.Start(width, height, new Benchmark()));
            SetContentView(sparrowView);
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