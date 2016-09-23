using OpenTK;
using Sparrow;
using Sparrow.Core;
using Sparrow.Geom;
using Sparrow.Textures;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES30;
#endif

namespace Sparrow.Filters
{
    /// <summary>
    /// Empty filter that does nothing. Used for testing; you can use its code to make custom filters.
    /// </summary>
    public class EmptyFilter : FragmentFilter
    {
       /* Program _program;

        override protected void CreatePrograms()
        {
            if (_program == null)
            {
                string programName = "_test_emptyFilterProgram";
                _program = SparrowSharpApp.GetProgram(programName);
                if (_program == null)
                {
                    _program = new Program(FragmentFilter.StandardVertexShader(), FragmentFilter.StandardFragmentShader());
                    SparrowSharpApp.RegisterProgram(programName, _program);
                }
            }
            VertexPosID = _program.Attributes["aPosition"];
            TexCoordsID = _program.Attributes["aTexCoords"];
        }

        override protected void ActivateWithPass(int pass, Texture texture, Matrix mvpMatrix)
        {
            GL.UseProgram(_program.Name);
            Matrix4 mvp = mvpMatrix.ConvertToMatrix4();
            GL.UniformMatrix4(_program.Uniforms["uMvpMatrix"], false, ref mvp);
        }*/
    }
}

