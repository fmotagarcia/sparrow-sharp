using Sparrow.Core;
using Sparrow.Display;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using SparrowSharp.Utils;

namespace Sparrow
{
    public static class SP
    {
        private static Stage _stage;

        public static Stage Stage { get { return _stage; } }

        public static Context Context { get; set; }

        public static Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static int DrawableWidth { get { return 300; } }
        // not used yet
        public static int DrawableHeight { get { return 300; } }
        // not used yet
        private static RenderSupport RenderSupport { get; set; }

        private static DisplayObject Root { get; set; }

        public static Juggler DefaultJuggler { get; set; }

        private static DisplayObjectContainer _root;
        private static Stopwatch watch = new Stopwatch();
        public static int cnt = 0;

        public static void Step(double time)
        {
            long elapsed = watch.ElapsedMilliseconds;
            watch.Restart();
            cnt++;

            RenderSupport.NextFrame();
            Stage.Render(RenderSupport);
            RenderSupport.FinishQuadBatch();

            #if DEBUG
            RenderSupport.CheckForOpenGLError();
            #endif

            Stage.AdvanceTime(elapsed);
            DefaultJuggler.AdvanceTime(elapsed / 1000.0f);
        }

        public static void Start(DisplayObjectContainer root)
        {
            if (_root != null)
            {
                throw new Exception("Sparrow has already been started");
            }
            _root = root;
        }

        public static void InitApp(float width, float height)
        {
            if (_stage == null)
            {
                _stage = new Stage(width, height);
                ReadjustStageSize(width, height); 
                DefaultJuggler = new Juggler();
                Context = new Context();
                RenderSupport = new RenderSupport();

                _stage.AddChild(_root);
            }
        }

        public static void ReadjustStageSize(float width, float height)
        {
            // TODO check if Width/Height are not 0 here
            _stage.Width = width * ViewScaleFactor / ContentScaleFactor;
            _stage.Height = height * ViewScaleFactor / ContentScaleFactor;
        }

        public static void RegisterProgram(string name, Program program)
        {
            Programs.Add(name, program);
        }

        public static void UnregisterProgram(string name)
        {
            Programs.Remove(name);
        }
        //		public static Juggler Juggler {
        //			get {
        //				return CurrentController.Juggler;
        //			}
        //		}
        private static float ContentScaleFactor
        {
            get { return 1.0f; }
        }

        private static float ViewScaleFactor
        {
            get { return 1.0f; }
        }
    }
}