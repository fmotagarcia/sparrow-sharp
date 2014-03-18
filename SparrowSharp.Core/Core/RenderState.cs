using System;
using Sparrow.Display;
using Sparrow.Geom;

namespace Sparrow.Core
{
    public class RenderState
    {
        public Matrix ModelViewMatrix;
        public float Alpha;
        public uint BlendMode;

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

