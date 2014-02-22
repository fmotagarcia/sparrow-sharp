using System;
using Sparrow.Geom;
using OpenTK.Graphics.ES20;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sparrow.Core
{
	public class Context
	{
		private readonly Dictionary<string, uint> _framebufferCache;

		private Rectangle Viewport
		{
			get 
			{
				int[] viewport = new int[4];
				GL.GetInteger (All.Viewport, out viewport);
				return new Rectangle (viewport [0], viewport [1], viewport [2], viewport [3]);
			}

			set
			{
				if (value != null) 
				{
					GL.Viewport (value.X, value.Y, value.Width, value.Height);
				} 
				else 
				{
					GL.Viewport (0, 0, SP.CurrentController.DrawableWidth, SP.CurrentController.DrawableWidth);
				}
			}
		}

		private Rectangle ScissorBox
		{
			get 
			{
				int[] scissorBox = new int[4];
				GL.GetInteger (All.ScissorBox, out scissorBox);
				return new Rectangle (scissorBox [0], scissorBox [1], scissorBox [2], scissorBox [3]);
			}

			set
			{
				if (value != null) 
				{
					GL.Enable (All.ScissorTest);
					GL.Scissor (value.X, value.Y, value.Width, value.Height);
				} 
				else 
				{
					GL.Disable (All.ScissorTest);
				}
			}
		}

		private Texture RenderTarget
		{
			set
			{
				if (value != null) 
				{
					uint framebuffer;
					if (!_framebufferCache.TryGetValue (value.Name, out framebuffer)) 
					{
						framebuffer = CreateFramebufferForTexture (value);
						_framebufferCache [value.Name] = framebuffer;
					}

					GL.BindFramebuffer (All.Framebuffer, framebuffer);
					GL.Viewport (0, 0, value.NativeWidth, value.NativeHeight);
				}
				else 
				{
					GL.BindFramebuffer (All.Framebuffer, 1);
					GL.Viewport (0, 0, SP.CurrentController.DrawableWidth, SP.CurrentController.DrawableHeight);
				}
				RenderTarget = value;
				// SP_RELEASE_AND_RETAIN(_renderTarget, renderTarget);
			}
		}

		public Context CurrentContext { get; set; }

//		SPContext *current = currentThreadDictionary[currentContextKey];
//		if (!current || current->_nativeContext != [EAGLContext currentContext])
//			return nil;
//
//		return current;

		public Context ()
		{
			_framebufferCache = new Dictionary<string, uint>();
		}

		private void RenderToBackBuffer()
		{
			RenderTarget = null;
		}

		private uint CreateFramebufferForTexture(Texture texture)
		{
			uint framebuffer;
			GL.GenFramebuffers (1, out framebuffer);
			GL.BindFramebuffer (All.Framebuffer, framebuffer);

			GL.FramebufferTexture2D (All.Framebuffer, All.ColorAttachment0, All.Texture2D, texture.Name, 0);
			if (GL.CheckFramebufferStatus (All.Framebuffer) != All.FramebufferComplete) 
			{
				Debug.WriteLine ("Failed to create framebuffer for render texture");
			}

			return framebuffer;
		}

		private void DestroyFramebufferForTexture(Texture texture)
		{
			uint framebuffer;
			if (_framebufferCache.TryGetValue (texture.Name, out framebuffer)) 
			{
				GL.DeleteFramebuffers(1, framebuffer);
				_framebufferCache.Remove (texture.Name);
			}
		}

		private void PresentBufferForDisplay() 
		{
			// [_nativeContext presentRenderbuffer:GL_RENDERBUFFER];
		}

		public bool DeviceSupportsOpenGLExtension(string extensionName)
		{
//			static dispatch_once_t once;
//			static NSArray *extensions = nil;
//
//			dispatch_once(&once, ^{
//				NSString *extensionsString = [NSString stringWithCString:(const char *)glGetString(GL_EXTENSIONS) encoding:NSASCIIStringEncoding];
//				extensions = [[extensionsString componentsSeparatedByString:@" "] retain];
//			});
//
//			return [extensions containsObject:extensionName];
		}
	}
}

