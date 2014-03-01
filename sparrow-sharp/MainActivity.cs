using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;

namespace sparrowsharp
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity (Label = "sparrow-sharp",
				ConfigurationChanges=ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
				MainLauncher = true)]
	public class MainActivity : Activity
	{
		//GLView1 view;
        ViewController sparrowView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create our OpenGL view, and display it
			//view = new GLView1 (this);
			//SetContentView (view);

            // NEW SPARROW CODE
            sparrowView = new ViewController(this.ApplicationContext);

            SetContentView(sparrowView);
            sparrowView.Start(typeof(SampleGame));
		}

		protected override void OnPause ()
		{
			// never forget to do this!
			base.OnPause ();
			//view.Pause ();
            sparrowView.Pause();
		}

		protected override void OnResume ()
		{
			// never forget to do this!
			base.OnResume ();
			//view.Resume ();
            sparrowView.Resume();
		}
	}
}


