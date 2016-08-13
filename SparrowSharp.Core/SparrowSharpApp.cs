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

        public static Stage Stage { get; private set; }

        public static Context Context { get; private set; }

        public static readonly Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static int DrawableWidth { get; private set; }

        public static int DrawableHeight { get; private set; }

        public static Object NativeWindow;

        private static RenderSupport renderSupport;

        public static DisplayObject Root { get; private set; }

        public static Juggler DefaultJuggler { get; private set; }

        private static readonly Stopwatch watch = new Stopwatch();

        private static StatsDisplay stats;

        public static void Step(double time)
        {
            long elapsed = watch.ElapsedMilliseconds;
            watch.Restart();

            renderSupport.NextFrame();
            Stage.Render(renderSupport);
            renderSupport.FinishQuadBatch();

            if (stats != null)
            {
                stats.NumDrawCalls = renderSupport.NumDrawCalls - 2; // stats display requires 2 itself
            }
                
            #if DEBUG
            RenderSupport.CheckForOpenGLError();
            #endif

            Stage.AdvanceTime(elapsed);
            DefaultJuggler.AdvanceTime(elapsed / 1000.0f);
        }

        public static void Start(float width, float height, Type rootType)
        {
            if (rootType == null)
            {
                throw new InvalidOperationException("Root cannot be null!");
            }

            if (Root != null)
            {
                throw new InvalidOperationException("App already initialized!");
            }
            RenderSupport.HasOpenGLError = false;
            DrawableWidth = (int)width;
            DrawableHeight = (int)height;

            Stage = new Stage(width, height);
            ReadjustStageSize(width, height); 
            DefaultJuggler = new Juggler();
            Context = new Context();
            renderSupport = new RenderSupport();

            Root = (DisplayObject)Activator.CreateInstance(rootType);
            Stage.AddChild(Root);
        }

        public static void ReadjustStageSize(float width, float height)
        {
            // TODO check if Width/Height are not 0 here
            DrawableWidth = (int)width;
            DrawableHeight = (int)height;
            Stage.Width = width * ViewScaleFactor / ContentScaleFactor;
            Stage.Height = height * ViewScaleFactor / ContentScaleFactor;
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
            Stage.RemoveAllChildren();
            Stage = null;
            Root = null;
            Programs.Clear();
        }
    }
}