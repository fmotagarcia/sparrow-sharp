using System;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Geom;
using Sparrow.Utils;
using Android.Content.Res;

namespace Sparrow.Core
{
    public class Texture
    {
        public uint Name { get; set; }

        public float NativeWidth { get; set; }

        public float NativeHeight { get; set; }

        public float Height { get; set; }

        public float Width { get; set; }

        public Rectangle Frame { get; set; }

        public bool PremultipliedAlpha { get { return false; } }

        public float Scale { get; set; }

        public Texture()
        {
            Name = (uint)GL.GenTexture();
            GL.BindTexture(All.Texture2D, Name);

            // setup texture parameters
            GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);
			
            Bitmap b = BitmapFactory.DecodeResource(ViewController.ResourcesRef, sparrowsharp.Resource.Drawable.exampleImageJPG);

            Height = b.Height;
            Width = b.Width;
            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);
        }

        public void AdjustVertexData(VertexData vertexData, int index, int numVertices)
        {
            // override in subclasses
        }
    }
}

