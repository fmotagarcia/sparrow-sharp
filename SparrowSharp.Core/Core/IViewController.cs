using System;
using Sparrow.Core;
using Sparrow.Display;

namespace SparrowSharp.Core
{
    public interface IViewController
    {
        int DrawableWidth { get; set; }

        int DrawableHeight { get; set; }

        Context SPContext { get; set; }

        DisplayObject Root { get; set; }

        Stage Stage { get; set; }

        float ContentScaleFactor { get; set; }

        RenderSupport RenderSupport { get; set; }
        //Juggler Juggler { get; set; }
    }
}

