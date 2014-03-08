using System;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Geom;
using Sparrow.Utils;
using Android.Content.Res;
using System.Net;
using Sparrow.Display;

namespace Sparrow.Core
{
	public class Texture
	{
		virtual public uint Name { get {throw new Exception ("Override 'Name' in subclasses.");} }
		virtual public float NativeWidth { get {throw new Exception ("Override 'NativeWidth' in subclasses.");} }
		virtual public float NativeHeight { get {throw new Exception ("Override 'NativeHeight' in subclasses.");} }
		virtual public float Height { get {throw new Exception ("Override 'Height' in subclasses.");} }
		virtual public float Width { get {throw new Exception ("Override 'Width' in subclasses.");} }
		virtual public float Scale { get {return 1.0f;} }
		// TODO virtual public GLTexture Root { get {null;} }
		virtual public Rectangle Frame { get {return null;} }
		virtual public bool PremultipliedAlpha { get { return false; } }
		// TODO virtual public bool Format { get { return TextureFormat.RGBA; } }
		virtual public bool MipMaps { get { return false; } }
		virtual public bool Repeat { 
			get {throw new Exception ("Override 'Repeat' in subclasses.");}
			set {throw new Exception ("Override 'Repeat' in subclasses.");}
		}
		//TODO virtual public TextureSmoothing Smoothing { 
		//	get {throw new Exception ("Override 'TextureSmoothing' in subclasses.");}
		//	set {throw new Exception ("Override 'TextureSmoothing' in subclasses.");}
		//}

		public Texture()
		{
			int name = GL.GenTexture ();
			GL.BindTexture (All.Texture2D, name);

			// setup texture parameters
			GL.TexParameter (All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.TexParameter (All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter (All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter (All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			
			Bitmap b = BitmapFactory.DecodeResource (ViewController.ResourcesRef, sparrowsharp.Resource.Drawable.exampleImageJPG);

			// this uses Android to set up things, it might not be safe to use Android calls mixed with OpenTK calls
			Android.Opengl.GLUtils.TexImage2D (Android.Opengl.GLES20.GlTexture2d, 0, b, 0);
			// see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
			// for how MonoGame does it
		}

		virtual public void AdjustVertexData(VertexData vertexData, int index, int numVertices)
		{
			// override in subclasses
		}

		virtual public void AdjustTexCoords(Object data, int count, int stride)
		{
			// override in subclasses
		}

		virtual public void AdjustPositions(Object data, int count, int stride)
		{
			// override in subclasses
		}

	}
}

