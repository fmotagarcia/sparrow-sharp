using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.ES20;
using Sparrow.Geom;
using Sparrow.Textures;

namespace Sparrow.Core
{
    /// <summary>
    ///  A Context object manages the state information, commands, and resources needed to draw using OpenGL.
    /// </summary>
    public class Context
    {
        /*
        private static readonly Dictionary<uint, uint> FramebufferCache;
        private Texture _renderTarget;

        static Context()
        {
            FramebufferCache = new Dictionary<uint, uint>();
        }

        /// <summary>
        /// The current OpenGL viewport rectangle.
        /// </summary>
        public Rectangle Viewport
        {
            get
            {
                int[] viewport = new int[4];
                GL.GetInteger(GetPName.Viewport, viewport);
                return new Rectangle(viewport[0], viewport[1], viewport[2], viewport[3]);
            }

            set
            {
                if (value != null)
                {
                    GL.Viewport((int)value.X, (int)value.Top, (int)value.Width, (int)value.Height);
                }
                else
                {
                    GL.Viewport(0, 0, (int)SparrowSharpApp.Stage.DrawableWidth, (int)SparrowSharpApp.Stage.DrawableHeight);
                }
            }
        }

        /// <summary>
        /// The current OpenGL scissor rectangle.
        /// </summary>
        public Rectangle ScissorBox
        {
            get
            {
                int[] scissorBox = new int[4];
                GL.GetInteger(GetPName.ScissorBox, scissorBox);

                return new Rectangle(scissorBox[0], scissorBox[1], scissorBox[2], scissorBox[3]);
            }

            set
            {
                if (value != null) {
                    GL.Enable (EnableCap.ScissorTest);
                    GL.Scissor ((int)value.X, (int)value.Y, (int)value.Width, (int)value.Height);                
                } else {
                    GL.Disable (EnableCap.ScissorTest);
                }
               
            }
        }

        /// <summary>
        /// The specified texture as the rendering target or nil if rendering to the default framebuffer.
        /// </summary>
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

                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

                    GL.Viewport(0, 0, (int)value.NativeWidth, (int)value.NativeHeight);
                }
                else
                {
                    // TODO: double check these on a device, the ifdef seems to be unneeded
                    #if __IOS__
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 1);
                    #else
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    #endif
                    GL.Viewport(0, 0, (int)SparrowSharpApp.Stage.DrawableWidth, (int)SparrowSharpApp.Stage.DrawableHeight);
                }
            }
            get { return _renderTarget; }
        }
        
        private uint CreateFramebufferForTexture(Texture texture)
        {
            uint framebuffer;
            GL.GenFramebuffers(1, out framebuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, texture.Name, 0);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
            {
                Debug.WriteLine("Failed to create framebuffer for render texture");
            }
            return framebuffer;
        }
        // TODO this is not used
        private void DestroyFramebufferForTexture(Texture texture)
        {
            uint framebuffer;
            if (FramebufferCache.TryGetValue(texture.Name, out framebuffer))
            {
                GL.DeleteFramebuffers(1, ref framebuffer);
                FramebufferCache.Remove(texture.Name);
            }
        }*/
    }
}

