﻿using System;
using Sparrow.Core;
#if __WINDOWS__
using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;
#elif __ANDROID__
using Android.Opengl;
#endif

namespace SparrowSharp.Core.Desktop.Rendering
{

#if __WINDOWS__
    class OpenGLDebugCallback
    {

        private static OpenGLDebugCallback instance;
        private DebugProc PCCallbackInstance = PCCallbackHandler; // The callback delegate must be stored to avoid GC

        public static void Init()
        {
            instance = new OpenGLDebugCallback();
        }

        private OpenGLDebugCallback()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GL.DebugMessageCallback(PCCallbackInstance, IntPtr.Zero);
        }

        static void PCCallbackHandler(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == DebugSeverity.DebugSeverityHigh || severity == DebugSeverity.DebugSeverityMedium)
            {
                string msg = Marshal.PtrToStringAnsi(message);
                Console.WriteLine("[GL] {0}; {1}; {2}; {3}; {4}", source, type, id, severity, msg);
            }
        }

#elif __ANDROID__
    // NOTE: this is only supported with OpenGL 3.1. And it only works with Android N or newer
    class OpenGLDebugCallback : Java.Lang.Object, GLES31Ext.IDebugProcKHR // TODO this is an Android 5.0 dependency, make it optional 
    { 
            
        private static OpenGLDebugCallback instance;
        
        public static void Init()
        {
            instance = new OpenGLDebugCallback();
        }

        public OpenGLDebugCallback()
        {
            if (Context.DeviceSupportsOpenGLExtension("GL_KHR_debug"))
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
