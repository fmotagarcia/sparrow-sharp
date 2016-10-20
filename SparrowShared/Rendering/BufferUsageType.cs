using OpenGL;

namespace Sparrow.Rendering
{

    public class BufferUsageType // TODO remove
    {

        public static readonly BufferUsageType DynamicDraw = new BufferUsageType(BufferUsageARB.DynamicDraw);
        public static readonly BufferUsageType StaticDraw = new BufferUsageType(BufferUsageARB.StaticDraw);
        public static readonly BufferUsageType StreamDraw = new BufferUsageType(BufferUsageARB.StreamDraw);

        public BufferUsageType(BufferUsageARB drawType)
        {
            Usage = drawType;
        }

        public BufferUsageARB Usage { private set; get; }

    }
}
