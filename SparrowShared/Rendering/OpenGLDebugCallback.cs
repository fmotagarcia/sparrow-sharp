using System;
using Sparrow.Core;
using Sparrow.Utils;
using OpenGL;
using System.Runtime.InteropServices;
#if __WINDOWS__
#elif __ANDROID__
using Android.Opengl;
#endif

namespace Sparrow.Core
{

#if __WINDOWS__
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
            Gl.Enable((EnableCap)Gl.DEBUG_OUTPUT_SYNCHRONOUS);
            Gl.DebugMessageCallback(PCCallbackInstance, IntPtr.Zero);
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

#elif __ANDROID__
    // NOTE: this is only supported with OpenGL ES 3.1. And it only works with Android N or newer
    class OpenGLDebugCallback : Java.Lang.Object, GLES31Ext.IDebugProcKHR // TODO this is an Android 5.0 dependency, make it optional 
    { 
            
        private static OpenGLDebugCallback instance;
        
        internal static void Init()
        {
            if (instance == null)
            {
                instance = new OpenGLDebugCallback();
            }
        }

        public OpenGLDebugCallback()
        {
            if (GLExtensions.DeviceSupportsOpenGLExtension("GL_KHR_debug"))
            {
                try
                {
                    GLES31Ext.GlDebugMessageCallbackKHR(this);
                }
                catch (Exception ex)
                {
                    // Google was lazy to implement this in Android 5 and 6, even though the drivers support it.
                    Console.Out.WriteLine("WARNING: No support for OpenGL debug callback, likely its not implemented");
                }
            }
            else
            {
                Console.Out.WriteLine("WARNING: No support for OpenGL debug callback");
            }
        }

        public void OnMessage(int source, int type, int id, int severity, string message)
        {
            Console.Out.WriteLine("OpenGL msg: " + source + " " + message);
        }
#endif
    }

}
