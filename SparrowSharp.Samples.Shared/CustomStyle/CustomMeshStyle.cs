using Sparrow.Rendering;
using Sparrow.Styles;

namespace Sparrow.Samples.CustomStyle
{
    public class CustomMeshStyle : MeshStyle
    {

        public override MeshEffect CreateEffect()
        {
            return new CustomMeshEffect();
        }

        public override void UpdateEffect(MeshEffect effect, RenderState state)
        {
            base.UpdateEffect(effect, state);
        }
        
        public override void CopyFrom(MeshStyle meshStyle)
        {
            base.CopyFrom(meshStyle);
        }

        public override bool CanBatchWith(MeshStyle meshStyle)
        {
            return base.CanBatchWith(meshStyle);
        }
    }
}