﻿using System;
using Sparrow.Textures;
using Android.Graphics;
using OpenTK.Graphics.ES20;
using Android.Opengl;
using System.Threading.Tasks;
using System.Net.Http;

namespace Sparrow.ResourceLoading
{
	public class TextureLoader 
	{
		protected bool _isLoaded = false;
		protected GLTexture _glTexture;
		public static global::Android.Content.Context _context;
		public bool IsLoaded { get { return _isLoaded;}	}
		public GLTexture Texture { get { return _glTexture;} }
		public event EventHandler<GLTexture> ResourceLoaded;

		public TextureLoader LoadRemoteImage (string remoteURL)
		{
			_isLoaded = false;
			LoadRemoteImageAsync (remoteURL);
			return this; 
		}

		private async void LoadRemoteImageAsync(string url)
		{
			HttpClient client = new HttpClient ();
			HttpResponseMessage msg = await client.GetAsync(url);
			if (msg.IsSuccessStatusCode)
			{
				using(var stream = await msg.Content.ReadAsStreamAsync())
				{
					﻿var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
					GenerateTexture( bitmap );
				}
			}
			// TODO dispatch some error
		}

		public GLTexture LoadLocalImage (string pathToFile)
		{
			_isLoaded = false;
			GenerateTexture( BitmapFactory.DecodeFile (pathToFile) );
			return _glTexture;
		}

		public TextureLoader LoadLocalImageAsync (string pathToFile)
		{
			_isLoaded = false;
			LoadLocalBitmapAsync (pathToFile);
			// TODO check wether the async call can be executed instantly, 
			// because in that case it will be impossible to cacth the event
			return this; 
		}

		private async void LoadLocalBitmapAsync (string path)
		{
			Bitmap newBitmap = await BitmapFactory.DecodeFileAsync (path);
			GenerateTexture (newBitmap);
		}

		/// <summary>
		/// Loads an image based on resource ID, only avaiable on Andoid. 
		/// Note that the resource will be scaled based on screen DPI if you put it into the drawable folder.
		/// If you want to avoid this, put it into the drawable-nodpi folder.
		/// </summary>
		public GLTexture LoadAndroidResource(int AndroidResourceId)
		{
			_isLoaded = false;
			Bitmap bitmap = BitmapFactory.DecodeResource(_context.Resources, AndroidResourceId);
			GenerateTexture(bitmap);
			return _glTexture;
		}

		private void GenerateTexture(Bitmap bitmap)
		{
			uint name = (uint)GL.GenTexture();
			GL.BindTexture(All.Texture2D, name);
			GL.TexParameter(All.Texture2D, All.TextureMaxAnisotropyExt, 1);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.NearestMipmapNearest);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.GenerateMipmapHint, (int)All.False);

			GLUtils.TexImage2D(GLES20.GlTexture2d, 0, bitmap, 0);

			// see https://github.com/mono/MonoGame/blob/develop/MonoGame.Framework/Graphics/Texture2D.cs
			// for how MonoGame does it
			_glTexture = new GLTexture(name, bitmap.Width, bitmap.Height, false, 1.0f, false);
			_isLoaded = true;
			// Make a temporary copy of the event to avoid possibility of 
			// a race condition if the last subscriber unsubscribes 
			// immediately after the null check and before the event is raised.
			EventHandler<GLTexture> handler = ResourceLoaded;
			if (handler != null)
			{
				handler(this, _glTexture);
			}
		}
	}
}
