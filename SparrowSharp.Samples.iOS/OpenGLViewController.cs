using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SparrowSharp.Core.iOS;
using Sparrow;
using Sparrow.Samples.iOS;

namespace SparrowSharp.Sample.iOS
{
    [Register("OpenGLViewController")]
    public partial class OpenGLViewController : UIViewController
    {
        public delegate void OnLoadedAction(int viewWidth,int viewHeight);

        private OnLoadedAction _onLoadedAction;

        public OpenGLViewController(string nibName, NSBundle bundle) : base(nibName, bundle)
        {
            _onLoadedAction = (width, height) => SparrowSharpApp.Start(width, height, typeof(Benchmark));
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
                {
                    View.StartAnimating();
                }
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
            _onLoadedAction((int)View.Bounds.Width, (int)View.Bounds.Height);
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            View.StopAnimating();
        }
    }
}
