using Sparrow.Geom;

namespace Sparrow.Core
{
    internal class RenderState
    {
        public readonly Matrix ModelViewMatrix;
        public float Alpha;
        public uint BlendMode;

        /// <summary>
        /// Helper class used by RenderSupport
        /// </summary>
        public RenderState()
        {
			ModelViewMatrix = Matrix.Create();
            Alpha = 1.0f;
            BlendMode = Sparrow.Display.BlendMode.NORMAL;
        }

        public void Setup(RenderState state, Matrix modelViewMatrix, float alpha, uint blendMode)
        {
            Alpha = alpha * state.Alpha;
            BlendMode = blendMode == Sparrow.Display.BlendMode.AUTO ? state.BlendMode : blendMode;

            ModelViewMatrix.CopyFromMatrix(state.ModelViewMatrix);
            ModelViewMatrix.PrependMatrix(modelViewMatrix);
        }
    }
}

