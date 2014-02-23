using Sparrow.Core;
using Sparrow.Display;

namespace Sparrow
{
    public static class SP
    {
        public static ViewController CurrentController { get; set; }

        public static Context Context
        {
            get
            {
                return CurrentController.Context;
            }
        }
        //		public static Juggler Juggler {
        //			get {
        //				return CurrentController.Juggler;
        //			}
        //		}
        public static Stage Stage
        {
            get
            {
                return CurrentController.Stage;
            }
        }

        public static DisplayObject Root
        {
            get
            {
                return CurrentController.Root;
            }
        }

        public static double ContentScaleFactor
        {
            get
            {
                return CurrentController != null ? CurrentController.ContentScaleFactor : 1.0;
            }
        }
    }
}