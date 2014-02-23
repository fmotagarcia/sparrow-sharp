using System;

namespace Sparrow.Display
{
    public class DisplayObjectContainer : DisplayObject
    {
        public int NumChildren { get; set; }

        public DisplayObjectContainer()
        {
        }

        public DisplayObject GetChildAt(int i)
        {
            throw new NotImplementedException();
        }
    }
}

