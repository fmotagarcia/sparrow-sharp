using System;
using OpenGL;

namespace Sparrow.Rendering
{
    public class GPUInfo
    {
        public static void PrintGPUInfo()
        {
            string versionOpenGL = Gl.GetString(StringName.Version);
            Console.Out.WriteLine("GL version:" + versionOpenGL);

            int[] workGrpCnt = new int[3];
            int[] workGrpSize = new int[3];
            int maxComputeWorkGroupInvocations;

            int version;
      
            Gl.Get(Gl.MAJOR_VERSION, out version);

            int minVersion;
#if __WINDOWS__
            minVersion = 4;
#elif __ANDROID__
            minVersion = 3; // GL ES 3
#endif
            if (version < minVersion)
            {
                throw new NotSupportedException("You need at least OpenGL " + minVersion + " to run Sparrow!");
            }
            
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_COUNT, 0, out workGrpCnt[0]);
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_COUNT, 1, out workGrpCnt[1]);
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_COUNT, 2, out workGrpCnt[2]);
            
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_SIZE, 0, out workGrpSize[0]);
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_SIZE, 1, out workGrpSize[1]);
            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_SIZE, 2, out workGrpSize[2]);

            Gl.Get(Gl.MAX_COMPUTE_WORK_GROUP_INVOCATIONS, out maxComputeWorkGroupInvocations);

            // According to Quallcomm docs, on GL ES:
            // 
            // GL_MAX_COMPUTE_WORK_GROUP_COUNT is guaranteed that the limit will not be less than 65535 in any of the
            //    three dimensions.
            //
            // GL_MAX_COMPUTE_WORK_GROUP_SIZE The maximum size is guaranteed to be at least 128 in the case of the x and y
            //    dimensions, and 64 in the case of z           
            //
            // GL_MAX_COMPUTE_WORK_GROUP_INVOCATIONS is guaranteed to be no lower than 128.
            Console.Out.WriteLine("max global (total) work group size " + string.Join(",", workGrpCnt));
            Console.Out.WriteLine("max local (in one shader) work group sizes " + string.Join(",", workGrpSize));
            Console.Out.WriteLine("max total local workgroup elements " + maxComputeWorkGroupInvocations);
        }

        /// <summary>
        /// Checks for an OpenGL error. If there is one, it is logged an the error code is returned.
        /// </summary>
        public static uint CheckForOpenGLError()
        {
            ErrorCode err = Gl.GetError();
            string errstr = "";
            while (err != ErrorCode.NoError)
            {
                errstr += "There was an OpenGL error: " + err;
                err = Gl.GetError();
                if (errstr.Length > 500) break; // to prevent some weird infine error loops
            }
            if (errstr != "")
            {
                Console.WriteLine(errstr);
            }
            return (uint)err;
        }
    }
}
