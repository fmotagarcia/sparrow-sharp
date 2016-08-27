using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sparrow.Core;
using Sparrow.Display;
using SparrowSharp.Utils;
using SparrowSharp.Core.Desktop.Rendering;

namespace Sparrow
{
    public static class SparrowSharpApp
    {

        public static Stage Stage { get; private set; }

        public static Context Context { get; private set; }

        public static readonly Dictionary<string, Program> Programs = new Dictionary<string, Program>();

        public static Object NativeWindow;

        private static RenderSupport renderSupport;

        public static DisplayObject Root { get; private set; }

        public static Juggler DefaultJuggler { get; private set; }

        private static readonly Stopwatch watch = new Stopwatch();

        private static StatsDisplay stats;

        public static void Start(uint width, uint height, Type rootType)
        {
            GPUInfo.PrintGPUInfo();
#if DEBUG
            OpenGLDebugCallback.Init();
#endif
            if (rootType == null)
            {
                throw new InvalidOperationException("Root cannot be null!");
            }

            if (Root != null)
            {
                throw new InvalidOperationException("App already initialized!");
            }
            RenderSupport.HasOpenGLError = false;

            Stage = new Stage(width, height);
            DefaultJuggler = new Juggler();
            Context = new Context();
            renderSupport = new RenderSupport();

            Root = (DisplayObject)Activator.CreateInstance(rootType);
            Stage.AddChild(Root);
        }

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