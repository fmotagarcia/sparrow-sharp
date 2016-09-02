using System;
using Sparrow.Core;

#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
#elif __ANDROID__
using OpenTK.Graphics.ES20;
#endif

namespace SparrowSharp.Core.Desktop.Rendering
{
    class GPUInfo
    {
        public static void PrintGPUInfo()
        {
            
            string versionOpenGL = GL.GetString(StringName.Version);
            Console.Out.WriteLine("GL version:" + versionOpenGL);

            int[] work_grp_cnt = new int[3];
            int[] work_grp_size = new int[3];
            int maxComputeWorkGroupInvocations;
#if __WINDOWS__
            int version;
            GL.GetInteger(GetPName.MajorVersion, out version);
            if (version < 4)
            {
                throw new NotSupportedException("You need at least OpenGL 4 to run on Desktop!");
            }
            GetIndexedPName maxWorkGroupCount = (GetIndexedPName)All.MaxComputeWorkGroupCount;
            GL.GetInteger(maxWorkGroupCount, 0, out work_grp_cnt[0]);
            GL.GetInteger(maxWorkGroupCount, 1, out work_grp_cnt[1]);
            GL.GetInteger(maxWorkGroupCount, 2, out work_grp_cnt[2]);

            GetIndexedPName maxComputeGroupSize = (GetIndexedPName)All.MaxComputeWorkGroupSize;
            GL.GetInteger(maxComputeGroupSize, 0, out work_grp_size[0]);
            GL.GetInteger(maxComputeGroupSize, 1, out work_grp_size[1]);
            GL.GetInteger(maxComputeGroupSize, 2, out work_grp_size[2]);

            GL.GetInteger((GetPName)All.MaxComputeWorkGroupInvocations, out maxComputeWorkGroupInvocations);
#else

            /* According to Quallcomm docs:
             
             GL_MAX_COMPUTE_WORK_GROUP_COUNT is guaranteed that the limit will not be less than 65535 in any of the
                three dimensions.

             GL_MAX_COMPUTE_WORK_GROUP_SIZE The maximum size is guaranteed to be at least 128 in the case of the x and y
                dimensions, and 64 in the case of z           

             GL_MAX_COMPUTE_WORK_GROUP_INVOCATIONS is guaranteed to be no lower than 128.
            */
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupCount, 0, out work_grp_cnt[0]);
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupCount, 1, out work_grp_cnt[1]);
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupCount, 2, out work_grp_cnt[2]);

            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupSize, 0, out work_grp_size[0]);
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupSize, 1, out work_grp_size[1]);
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupSize, 2, out work_grp_size[2]);
            
            OpenTK.Graphics.ES31.GL.GetInteger(OpenTK.Graphics.ES31.All.MaxComputeWorkGroupInvocations, out maxComputeWorkGroupInvocations);
#endif
            Console.Out.WriteLine("max global (total) work group size " + string.Join(",", work_grp_cnt));
            Console.Out.WriteLine("max local (in one shader) work group sizes " + string.Join(",", work_grp_size));
            Console.Out.WriteLine("max total local workgroup elements " + maxComputeWorkGroupInvocations);
        }

        public static void checkShaderCompileError(int shaderId)
        {
            int rvalue;
            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out rvalue);
            if (rvalue != 1)
            {
                string info = GL.GetShaderInfoLog(shaderId);
                Console.Out.WriteLine("Shader compile error: " + info + " " + rvalue);
            }
        }

        public static void checkShaderLinkError(int shaderProgramId)
        {
            int rvalue;
#if __WINDOWS__
            GL.GetProgram(shaderProgramId, GetProgramParameterName.LinkStatus, out rvalue);
#else
            GL.GetProgram(shaderProgramId, ProgramParameter.LinkStatus, out rvalue);
#endif
            if (rvalue != 1)
            {
                string info = GL.GetProgramInfoLog(shaderProgramId);
                Console.Out.WriteLine("Shader linker error: " + info + " " + rvalue);
            }
        }

        public static bool HasOpenGLError { get; internal set; }

        /// <summary>
        /// Checks for an OpenGL error. If there is one, it is logged an the error code is returned.
        /// </summary>
        public static uint CheckForOpenGLError()
        {
#if __WINDOWS__
            ErrorCode err = GL.GetError();
#else
            ErrorCode err = GL.GetErrorCode();
#endif
            string errstr = "";
            while (err != ErrorCode.NoError)
            {
                HasOpenGLError = true;
                errstr += "There was an OpenGL error: " + err;
#if __WINDOWS__
                err = GL.GetError();
#else
                err = GL.GetErrorCode();
#endif
            }
            if (errstr != "")
            {
                Console.WriteLine(errstr);
            }
            return (uint)err;
        }
    }
}
