namespace Sparrow.Utils
{
    public static class VertexColorHelper
    {
        
        public static VertexColor CreateVertexColor(byte r, byte g, byte b, byte a)
        {
            VertexColor vertexColor = new VertexColor();

            vertexColor.R = r;
            vertexColor.G = g;
            vertexColor.B = b;
            vertexColor.A = a;

            return vertexColor;
        }

        public static VertexColor CreateVertexColor(uint color, float alpha)
        {
            VertexColor vertexColor = new VertexColor();

            vertexColor.R = ColorUtil.GetR(color);
            vertexColor.G = ColorUtil.GetG(color);
            vertexColor.B = ColorUtil.GetB(color);

            vertexColor.A = (byte)(alpha * 255.0f + 0.5f);

            return vertexColor;
        }

        public static VertexColor PremultiplyAlpha(VertexColor color)
        {
            float alpha = color.A / 255.0f;

            if (alpha == 1.0f)
            {
                return color;
            }
            return CreateVertexColor(
                (byte)(color.R * alpha + 0.5f),
                (byte)(color.G * alpha + 0.5f),
                (byte)(color.B * alpha + 0.5f),
                color.A);
        }

        public static VertexColor UnmultiplyAlpha(VertexColor color)
        {
            float alpha = color.A / 255.0f;

            if (alpha == 0.0f || alpha == 1.0f)
            {
                return color;
            }
            // 0.5f is added to prevent rounding issues, see 
            // http://stackoverflow.com/questions/25703304/why-does-a-division-result-differ-based-on-the-cast-type
            return VertexColorHelper.CreateVertexColor(
                (byte)(color.R / alpha + 0.5f),
                (byte)(color.G / alpha + 0.5f),
                (byte)(color.B / alpha + 0.5f),
                color.A);
        }

        public static bool IsOpaqueWhite(VertexColor color)
        {
            return color.A == 255 && color.R == 255 && color.G == 255 && color.B == 255;
        }
    }
}