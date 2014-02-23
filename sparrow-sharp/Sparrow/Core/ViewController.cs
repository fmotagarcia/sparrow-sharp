using Sparrow.Display;
using System.Collections.Generic;

namespace Sparrow.Core
{
    public class ViewController
    {
        public Dictionary<string, Program> Programs { get; private set; }

        public int DrawableWidth { get; set; }

        public int DrawableHeight { get; set; }

        public Context Context { get; set; }

        public DisplayObject Root { get; set; }

        public Stage Stage { get; set; }

        public float ContentScaleFactor { get; set; }

        public void RegisterProgram(string name, Program program)
        {
            Programs.Add(name, program);
        }

        public void UnregisterProgram(string name)
        {
            Programs.Remove(name);
        }

        public void Setup()
        {
            Programs = new Dictionary<string, Program>();
        }
    }
}

