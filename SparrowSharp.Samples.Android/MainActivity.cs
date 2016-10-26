using Android.App;
using Android.OS;
using Android.Content.PM;
using Sparrow.Core;
using Android.Views;

namespace Sparrow.Samples
{
    [Activity(Label = "sparrow-sharp benchmark", Name = "awesome.demo.activity",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden,
        MainLauncher = true)]
    public class MainActivity : Activity
    {
        private static AndroidViewController sparrowView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            RequestWindowFeature(WindowFeatures.NoTitle);

            if (sparrowView == null)
            {
                sparrowView = new AndroidViewController(BaseContext, typeof(Benchmark));  
            }
            if (sparrowView.Parent != this)
            {
                if (sparrowView.Parent != null)
                {
                    // this is executed when the view is recreated, e.g. when the user returns to this app after 
                    // pressing the back button
                    ViewGroup oldParent = (ViewGroup)sparrowView.Parent;
                    oldParent.RemoveView(sparrowView);
                }
                SetContentView(sparrowView);
            }  
        }

    }
}