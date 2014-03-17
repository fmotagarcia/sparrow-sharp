using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Sparrow.Textures;
using Sparrow;
using Sparrow.Samples.iOS;
using SparrowSharp.Core.iOS;
using SparrowSharp.Samples.iOS;

namespace SparrowSharp.Sample.iOS
{
    [Register("OpenGLViewController")]
    public partial class OpenGLViewController : UIViewController
    {
        public OpenGLViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
            RegisterResources();
            SP.Start(new Benchmark());
        }

        private void RegisterResources()
        {
            iOSTextureProvider provider = new iOSTextureProvider();
            provider.RegisterResource((uint)BenchmarkResources.Sparrow, 0);

            TextureFactory.Provider = provider;
        }

        new EAGLView View { get { return (EAGLView)base.View; } }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillResignActiveNotification, a =>
            {
                if (IsViewLoaded && View.Window != null)
                    View.StopAnimating();
            }, this);
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, a =>
            {
                if (IsViewLoaded && View.Window != null)
                    View.StartAnimating();
            }, this);
            NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.WillTerminateNotification, a =>
            {
                if (IsViewLoaded && View.Window != null)
                    View.StopAnimating();
            }, this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
        }

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();

            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.StartAnimating();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            View.StopAnimating();
        }
    }
}
