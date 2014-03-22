using System;
using Sparrow.Textures;
using Sparrow.Geom;
using Sparrow;
using Sparrow.Core;
using OpenTK.Graphics.ES20;
using OpenTK;

namespace SparrowSharp.Filters
{
	public class EmptyFilter : FragmentFilter
	{
		Program _program;

		override public void CreatePrograms()
		{
			if (_program == null)
			{
				string programName = "sparrow_emptyProgram";
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

		override public void ActivateWithPass (int pass, Texture texture, Matrix matrix)
		{
			GL.UseProgram(_program.Name);
			Matrix4 mvp = matrix.ConvertToMatrix4();
			GL.UniformMatrix4(_program.Uniforms["uMvpMatrix"], false, ref mvp);
		}

	}
}

