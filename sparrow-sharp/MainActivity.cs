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
	[Activity (Label = "sparrow-sharp",
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
		MainLauncher = true)]
	public class MainActivity : Activity
	{
		ViewController sparrowView;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			sparrowView = new ViewController (this.ApplicationContext);

			SetContentView (sparrowView);
			sparrowView.Start (typeof(SampleGame));
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			sparrowView.Pause ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			sparrowView.Resume ();
		}
	}
}