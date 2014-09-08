using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sparrow.Core;
using Sparrow.Display;
using SparrowSharp.Utils;

namespace Sparrow
{
    public static class SparrowSharpApp
    {
        private static Stage _stage;

        public static Stage Stage { get { return _stage; } }

        public static Context Context { get; set; }

        public static Dictionary<string, Program> Programs = new Dictionary<string, Program>();
        private static int _drawableWidth;

        public static int DrawableWidth { get { return _drawableWidth; } }

        private static int _drawableHeight;

        public static int DrawableHeight { get { return _drawableHeight; } }

        private static RenderSupport RenderSupport { get; set; }

        public static DisplayObject Root { get; set; }

        public static Juggler DefaultJuggler { get; set; }

        private static readonly Stopwatch watch = new Stopwatch();

        public static void Step(double time)
        {
            long elapsed = watch.ElapsedMilliseconds;
            watch.Restart();

            RenderSupport.NextFrame();
            Stage.Render(RenderSupport);
            RenderSupport.FinishQuadBatch();

            if (stats != null)
            {
                stats.NumDrawCalls = RenderSupport.NumDrawCalls - 2; // stats display requires 2 itself
            }
                
            #if DEBUG
            RenderSupport.CheckForOpenGLError();
            #endif

            Stage.AdvanceTime(elapsed);
            DefaultJuggler.AdvanceTime(elapsed / 1000.0f);
        }

        public static void Start(float width, float height, Sprite root)
        {
            if (root == null)
            {
                throw new InvalidOperationException("Root cannot be null!");
            }

            if (Root != null)
            {
                throw new InvalidOperationException("App already initialized!");
            }
            _drawableWidth = (int)width;
            _drawableHeight = (int)height;

            _stage = new Stage(width, height);
            ReadjustStageSize(width, height); 
            DefaultJuggler = new Juggler();
            Context = new Context();
            RenderSupport = new RenderSupport();

            Root = root;
            _stage.AddChild(Root);
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

        public static Program GetProgram(string name)
        {
            if (Programs.ContainsKey(name))
            {
                return Programs[name];
            }
            return null;
        }

        public static float ContentScaleFactor
        {
            get { return 1.0f; }
        }

        private static float ViewScaleFactor
        {
            get { return 1.0f; }
        }

        private static StatsDisplay stats;

        public static bool ShowStats
        {
            set {
                if (stats == null) {
                    stats = new StatsDisplay();
                    Stage.AddChild(stats);
                }
                stats.Visible = value;
            }
        }

        public static void Destroy()
        {
            stats = null;
            _stage.RemoveAllChildren();
            _stage = null;
            Programs.Clear();
        }
    }
}