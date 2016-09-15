using System;

namespace Sparrow.Filters
{
    /// <summary>
    /// Helper class that allows quick creation Sparrow's built in filters.
    /// </summary>
    public static class FilterFactory
    {
        /*
        /// <summary>
        /// Creates a blur filter on the object with the specified parameters
        /// </summary>
        public static BlurFilter Blur(float blur = 1.0f, float resolution = 1.0f)
        {
            return new SparrowSharp.Filters.BlurFilter(blur, resolution);
        }

        /// <summary>
        /// Creates a drop shadow with a specified distance, angle, color, alpha, blur and resolution.
        /// </summary>
        public static BlurFilter DropShadow(
            float distance = 4.0f, 
            float angle = 0.785f,
            uint color = 0x000000, 
            float alpha = 0.5f, 
            float blur = 1.0f,
            float resolution = 0.5f)
        {
            BlurFilter dropShadow = new SparrowSharp.Filters.BlurFilter(blur, resolution);
            dropShadow.OffsetX = (float)Math.Cos(angle) * distance;
            dropShadow.OffsetY = (float)Math.Sign(angle) * distance;
            dropShadow.Mode = FragmentFilterMode.Below;
            dropShadow.SetUniformColor(true, color, alpha);
            return dropShadow;
        }

        /// <summary>
        /// Creates a glow with a specified color, alpha, blur and resolution.
        /// </summary>
        public static BlurFilter Glow(
            uint color = 0xFF00FF,
            float alpha = 0.5f,
            float blur = 1.0f,
            float resolution = 0.5f)
        {
            BlurFilter glow = new BlurFilter(blur, resolution);
            glow.Mode = FragmentFilterMode.Below;
            glow.SetUniformColor(true, color, alpha);
            return glow;
        }

        /// <summary>
        /// Creates a color matrix filter wit the specified color matrix.
        /// If the matrix is not specified it will use an identity matrix 
        /// (that does nothing)
        /// </summary>
        public static ColorMatrixFilter ColorMatrix(ColorMatrix colorMatrix = null)
        {
            return new ColorMatrixFilter(colorMatrix);
        }*/
    }
}

