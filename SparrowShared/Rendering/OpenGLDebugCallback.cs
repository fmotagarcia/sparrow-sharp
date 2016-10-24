using System;
using OpenGL;
using System.Runtime.InteropServices;

namespace Sparrow.Core
{
    
    class OpenGLDebugCallback
    {

        private static OpenGLDebugCallback instance;
        private Gl.DebugProc PCCallbackInstance = PCCallbackHandler; // The callback delegate must be stored to avoid GC

        internal static void Init()
        {
            if (instance == null)
            {
                instance = new OpenGLDebugCallback();
            }
        }

        private OpenGLDebugCallback()
        {
            try
            {
                Gl.Enable((EnableCap)Gl.DEBUG_OUTPUT_SYNCHRONOUS);
                Gl.DebugMessageCallback(PCCallbackInstance, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                // Google was lazy to implement this in Android 5 and 6, even though the drivers support it.
                Console.Out.WriteLine("WARNING: No support for OpenGL debug callback, likely its not implemented");
            }
        }

        static void PCCallbackHandler(Gl.DebugSource source, Gl.DebugType type, uint id, Gl.DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            string msg = Marshal.PtrToStringAnsi(message);
            if (severity == Gl.DebugSeverity.High || severity == Gl.DebugSeverity.Medium)
            {
                Console.WriteLine("[GL] {0}; {1}; {2}; {3}; {4}", source, type, id, severity, msg);
            }
            else if (severity == Gl.DebugSeverity.Low)
            {
                Console.WriteLine("[GL] {0}; {1}; {2}; {3}; {4}", source, type, id, severity, msg);
            }
        }
    }

}
