using System;
using System.Net.Http;
using Android.Graphics;
using Android.Opengl;
using Sparrow.Textures;
using System.IO;
using Sparrow.Core;

namespace Sparrow.ResourceLoading
{
    public class TextureLoader
    {
        protected bool _isLoaded = false;
        protected Texture _glTexture;
        public static global::Android.Content.Context _context;

        public bool IsLoaded { get { return _isLoaded; } }

        public Texture Texture { get { return _glTexture; } }

        public event EventHandler<Texture> ResourceLoaded;
        public event EventHandler<string> LoadingError;

        public TextureLoader LoadRemoteImage(string remoteURL)
        {
            _isLoaded = false;
            LoadRemoteImageAsync(remoteURL);
            return this; 
        }

        private async void LoadRemoteImageAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage msg = await client.GetAsync(url);
            if (msg.IsSuccessStatusCode)
            {
                using (var stream = await msg.Content.ReadAsStreamAsync())
                {
                    ﻿
                    var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
                    GenerateTexture(bitmap);
                }
            }
            else if (LoadingError != null)
            {
                LoadingError(this, msg.Content + msg.StatusCode.ToString());
            }
        }

        public Texture LoadLocalImage(string pathToFile)
        {
            _isLoaded = false;
            GenerateTexture(BitmapFactory.DecodeFile(pathToFile));
            return _glTexture;
        }

        public TextureLoader LoadLocalImageAsync(string pathToFile)
        {
            _isLoaded = false;
            LoadLocalBitmapAsync(pathToFile);
            // TODO check wether the async call can be executed instantly, 
            // because in that case it will be impossible to cacth the event
            return this; 
        }

        private async void LoadLocalBitmapAsync(string path)
        {
            Bitmap newBitmap = await BitmapFactory.DecodeFileAsync(path);
            GenerateTexture(newBitmap);
        }

        /// <summary>
        /// Loads an image based on resource ID; only avaiable on Andoid. 
        /// Note that the resource will be scaled based on screen DPI if you put it into the drawable folder.
        /// If you want to avoid this, put it into the drawable-nodpi folder.
        /// </summary>
        public Texture LoadAndroidResource(int AndroidResourceId)
        {
            _isLoaded = false;
            Bitmap bitmap = BitmapFactory.DecodeResource(_context.Resources, AndroidResourceId);
            GenerateTexture(bitmap);
            return _glTexture;
        }

        public Texture LoadFromStream(Stream stream)
        {
            _isLoaded = false;
            Bitmap bitmap = BitmapFactory.DecodeStream(stream);
            GenerateTexture(bitmap);
            return _glTexture;
        }

        protected void GenerateTexture(Bitmap bitmap)
        {
            bitmap.SetPremultiplied(true); // note: Android 4.4 function

            TextureOptions opts = new TextureOptions(TextureFormat.Rgba8888, SparrowSharp.ContentScaleFactor);
            _glTexture = Texture.Empty(bitmap.Width, bitmap.Height, true, 0, false, -1, TextureFormat.Rgba8888);

            if (bitmap.Width > 0 && bitmap.Height > 0)
            {
                GLUtils.TexSubImage2D(GLES20.GlTexture2d,
                    0, // level
                    0, // xOffset
                    0, // yOffset
                    bitmap);
            }
            else
            {
                Console.Out.WriteLine("WARNING: empty bitmap loaded");
            }
            //_glTexture = new ConcreteTexture(name, TextureFormat.Rgba8888, bitmap.Width, bitmap.Height, 0, true, false, 1.0f);
            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler<Texture> handler = ResourceLoaded;
            if (handler != null)
            {
                handler(this, _glTexture);
            }
        }
    }
}

