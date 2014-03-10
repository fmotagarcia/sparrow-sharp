using Android.App;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;

namespace Sparrow.Samples.Android
{
    [Activity(Label = "sparrow-sharp",
		#if __ANDROID_11__
		HardwareAccelerated=false,
		#endif
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
        MainLauncher = true)]
    public class MainActivity : Activity
    {
        ViewController sparrowView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            sparrowView = new ViewController(this.ApplicationContext);

            SetContentView(sparrowView);
			sparrowView.Start(typeof(Benchmark));
        }

        protected override void OnPause()
        {
            base.OnPause();

            sparrowView.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            sparrowView.Resume();
        }
    }
}