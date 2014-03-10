using Sparrow.Core;
using Sparrow.Display;
using System.Collections.Generic;
using SparrowSharp.Core;

namespace Sparrow
{
    public static class SP
    {
        public static IViewController CurrentController { get; set; }

        public static Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static void RegisterProgram(string name, Program program)
        {
            Programs.Add(name, program);
        }

        public static void UnregisterProgram(string name)
        {
            Programs.Remove(name);
        }

        public static Context Context
        {
            get
            {
                return CurrentController.SPContext;
            }

            set
            {
                CurrentController.SPContext = value;
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

        public static float ContentScaleFactor
        {
            get
            {
                return CurrentController != null ? CurrentController.ContentScaleFactor : 1.0f;
            }
        }
    }
}