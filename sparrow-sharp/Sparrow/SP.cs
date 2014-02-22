using System;
using Sparrow.Core;
using Sparrow.Display;

namespace Sparrow
{
	public static class SP
	{
		public static ViewController CurrentController { get; set; }

		public static Sparrow.Core.Context Context {
			get {
				return CurrentController.Context;
			}
		}
		//		public static Juggler Juggler {
		//			get {
		//				return CurrentController.Juggler;
		//			}
		//		}
		public static Sparrow.Display.Stage Stage {
			get {
				return CurrentController.Juggler;
			}
		}

		public static DisplayObject Root {
			get {
				return CurrentController.Root;
			}
		}

		public static double ContentScaleFactor {
			get {
				return CurrentController != null ? CurrentController.CurrentScaleFactor : 1.0;
			}
		}
	}
}