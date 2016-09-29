#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Rendering
{

    public class BufferUsageType
    {
#if __WINDOWS__
        public static readonly BufferUsageType DynamicDraw = new BufferUsageType(BufferUsageHint.DynamicDraw);
        public static readonly BufferUsageType StaticDraw = new BufferUsageType(BufferUsageHint.StaticDraw);
        public static readonly BufferUsageType StreamDraw = new BufferUsageType(BufferUsageHint.StreamDraw);

        public BufferUsageType(BufferUsageHint drawType)
        {
            Usage = drawType;
        }

        public BufferUsageHint Usage { private set; get; }

#elif __ANDROID__
        public static readonly BufferUsageType DynamicDraw = new BufferUsageType(BufferUsage.DynamicDraw);
        public static readonly BufferUsageType StaticDraw = new BufferUsageType(BufferUsage.StaticDraw);
        public static readonly BufferUsageType StreamDraw = new BufferUsageType(BufferUsage.StreamDraw);

        public BufferUsageType(BufferUsage drawType)
        {
            Usage = drawType;
        }

        public BufferUsage Usage { private set; get; }
#endif

    }
}
