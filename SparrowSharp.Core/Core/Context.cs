using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics;
using Sparrow.Geom;
using Sparrow.Textures;

namespace Sparrow.Core
{
    public class Context
    {
        private const string CurrentContextKey = "SPCurrentContext";
        private static Dictionary<uint, uint> FramebufferCache = new Dictionary<uint, uint>();
        private static HashSet<string> Extensions;
        private Texture _renderTarget;

        static Context()
        {
            Extensions = new HashSet<string>();
			#if __ANDROID__
			string extensionsString = GL.GetString(All.Extensions);
			#elif __IOS__ || __WINDOWS__
			string extensionsString = GL.GetString(StringName.Extensions);
			#endif
            
            if (!string.IsNullOrEmpty(extensionsString))
            {
                string[] extensions = extensionsString.Split(' ');
                for (int i = 0; i < extensions.Length; i++)
                {
                    Extensions.Add(extensions[i]);
                }
            }
        }

        public Rectangle Viewport
        {
            get
            {
                int[] viewport = new int[4];
				#if __ANDROID__
				GL.GetInteger(All.Viewport, viewport);
				#elif __IOS__ || __WINDOWS__
				GL.GetInteger(GetPName.Viewport, viewport);
				#endif
                
                return new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);
            }

            set
            {
				if (value != null) {
					GL.Viewport ((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);
				} else {
					GL.Viewport(0, 0, SparrowSharpApp.DrawableWidth, SparrowSharpApp.DrawableHeight);
				}
            }
        }

        public Rectangle ScissorBox
        {
            get
            {
                int[] scissorBox = new int[4];
				#if __ANDROID__
				GL.GetInteger(All.ScissorBox, scissorBox);
				#elif __IOS__ || __WINDOWS__
				GL.GetInteger(GetPName.ScissorBox, scissorBox);
				#endif
                
                return new Rectangle(scissorBox[0], scissorBox[1], scissorBox[2], scissorBox[3]);
            }

            set
            {
				#if __ANDROID__
				if (value != null) {
				GL.Enable (All.ScissorTest);
				GL.Scissor ((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);                
				} else {
				GL.Disable (All.ScissorTest);
				}
				#elif __IOS__ || __WINDOWS__
				if (value != null) {
					GL.Enable (EnableCap.ScissorTest);
					GL.Scissor ((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);                
				} else {
					GL.Disable (EnableCap.ScissorTest);
				}
				#endif
            }
        }

        public Texture RenderTarget
        {
            set
            {
				_renderTarget = value;
				if (value != null)
				{
	                uint framebuffer;
	                if (!FramebufferCache.TryGetValue(value.Name, out framebuffer))
	                {
	                    framebuffer = CreateFramebufferForTexture(value);
	                    FramebufferCache.Add(value.Name, framebuffer);
	                }
					#if __ANDROID__
					GL.BindFramebuffer(All.Framebuffer, framebuffer);
					#elif __IOS__ || __WINDOWS__
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
					#endif
					GL.Viewport (0, 0, (int)value.NativeWidth, (int)value.NativeHeight);
	            }
				else
				{
					#if __ANDROID__
					GL.BindFramebuffer(All.Framebuffer, 0); //its 1 in sparrow-s, this might need to be 0
					#elif __IOS__ || __WINDOWS__
					GL.BindFramebuffer(FramebufferTarget.Framebuffer, 1); //its 1 in sparrow-s, this might need to be 0
					#endif

					GL.Viewport(0, 0, SparrowSharpApp.DrawableWidth, SparrowSharpApp.DrawableHeight);
				}
			}
			get { return _renderTarget;}
        }

        public static bool DeviceSupportsOpenGLExtension(string extensionName)
        {
            return Extensions.Contains(extensionName);
        }

        private uint CreateFramebufferForTexture(Texture texture)
        {
            uint framebuffer;
            GL.GenFramebuffers(1, out framebuffer);
			#if __ANDROID__
			GL.BindFramebuffer(All.Framebuffer, framebuffer);

			GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0, All.Texture2D, texture.Name, 0);
			#elif __IOS__ || __WINDOWS__
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, texture.Name, 0);
			#endif
            
			#if __WINDOWS__
			if (GL.CheckFramebufferStatus(All.Framebuffer) != FramebufferErrorCode.FramebufferComplete);
			#elif __ANDROID__ 
			if (GL.CheckFramebufferStatus(All.Framebuffer) != All.FramebufferComplete)
			#else
			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
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

