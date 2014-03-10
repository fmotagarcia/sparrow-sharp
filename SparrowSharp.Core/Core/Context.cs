using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics;
using Sparrow.Geom;

namespace Sparrow.Core
{
    public class Context
    {
        private const string CurrentContextKey = "SPCurrentContext";
        private static Dictionary<uint, uint> FramebufferCache = new Dictionary<uint, uint>();
        private static HashSet<string> Extensions;
        private IGraphicsContext _nativeContext;
        private Texture _renderTarget;

        static Context()
        {
            Extensions = new HashSet<string>();
            string extensionsString = GL.GetString(All.Extensions);
            if (!string.IsNullOrEmpty(extensionsString))
            {
                string[] extensions = extensionsString.Split(' ');
                for (int i = 0; i < extensions.Length; i++)
                {
                    Extensions.Add(extensions[i]);
                }
            }
        }

        public Context(IGraphicsContext graphicsContext)
        {
            _nativeContext = graphicsContext;
        }

        public Rectangle Viewport
        {
            get
            {
                int[] viewport = new int[4];
                GL.GetInteger(All.Viewport, viewport);
                return new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);
            }

            set
            {
                GL.Viewport((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
            }
        }

        public void ResetViewport()
        {
            GL.Viewport(0, 0, (int)SP.CurrentController.DrawableWidth, (int)SP.CurrentController.DrawableWidth);
        }

        public Rectangle ScissorBox
        {
            get
            {
                int[] scissorBox = new int[4];
                GL.GetInteger(All.ScissorBox, scissorBox);
                return new Rectangle(scissorBox[0], scissorBox[1], scissorBox[2], scissorBox[3]);
            }

            set
            {
                if (value != null)
                {
                    GL.Enable(All.ScissorTest);
                    GL.Scissor((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);                
                }
            }
        }

        public void ResetScissorBox()
        {
            GL.Disable(All.ScissorTest);
        }

        public Texture RenderTarget
        {
            set
            {
                uint framebuffer;
                if (!FramebufferCache.TryGetValue(value.Name, out framebuffer))
                {
                    framebuffer = CreateFramebufferForTexture(value);
                    FramebufferCache.Add(value.Name, framebuffer);
                }

                GL.BindFramebuffer(All.Framebuffer, framebuffer);
                GL.Viewport(0, 0, (int)value.NativeWidth, (int)value.NativeHeight);
                _renderTarget = value;
            }
        }

        public void ResetRenderTarget()
        {
            GL.BindFramebuffer(All.Framebuffer, 1);
            GL.Viewport(0, 0, SP.CurrentController.DrawableWidth, SP.CurrentController.DrawableHeight);
            _renderTarget = null;
        }

        private void RenderToBackBuffer()
        {
            _renderTarget = null;
        }

        public static bool DeviceSupportsOpenGLExtension(string extensionName)
        {
            return Extensions.Contains(extensionName);
        }

        private uint CreateFramebufferForTexture(Texture texture)
        {
            uint framebuffer;
            GL.GenFramebuffers(1, out framebuffer);
            GL.BindFramebuffer(All.Framebuffer, framebuffer);

            GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0, All.Texture2D, texture.Name, 0);
			#if __WINDOWS__
			if (GL.CheckFramebufferStatus(All.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			#else
			if (GL.CheckFramebufferStatus(All.Framebuffer) != All.FramebufferComplete)
			#endif
			{
                Debug.WriteLine("Failed to create framebuffer for render texture");
            }


            return framebuffer;
        }

        private void DestroyFramebufferForTexture(Texture texture)
        {
            uint framebuffer;
            if (FramebufferCache.TryGetValue(texture.Name, out framebuffer))
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                FramebufferCache.Remove(texture.Name);
            }
        }
    }
}

