using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;
using componenttest;

namespace sparrowsharp
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity (Label = "sparrow-sharp",
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
		MainLauncher = true)]
	public class MainActivity : Activity
	{
		const bool TEST = true;
		GLView view;
		ViewController sparrowView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			if (TEST) {
				view = new GLView (this);
				SetContentView (view);
			} else {
				sparrowView = new ViewController (this.ApplicationContext);

				SetContentView (sparrowView);
				sparrowView.Start (typeof(SampleGame));
			}
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			if (TEST) {
				view.Pause ();
			} else {
				sparrowView.Pause ();
			}
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			if (TEST) {
				view.Resume ();
			} else {
				sparrowView.Resume ();
			}
		}
	}
}


