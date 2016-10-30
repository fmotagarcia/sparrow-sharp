using System;
using System.Net.Http;
using Android.Graphics;
using Android.Opengl;
using Sparrow.Textures;
using System.IO;
using Sparrow.Core;
using OpenGL;

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
            
            // from https://github.com/labnation/MonoGame/blob/master/MonoGame.Framework/Graphics/Texture2D.OpenGL.cs
            bitmap.LockPixels();
            int[] pixels = new int[bitmap.Width * bitmap.Height];
            bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
            bitmap.UnlockPixels();
            
            ConvertToABGR(bitmap.Height, bitmap.Width, pixels); // Convert from ARGB to ABGR
            // NOTE: GLUtils can figure out the exact pixel format, maybe implement it in the future.
            TextureOptions opts = new TextureOptions(TextureFormat.Rgba8888, SparrowSharp.ContentScaleFactor);
            _glTexture = Texture.FromData(pixels, opts, bitmap.Width, bitmap.Height);
            bitmap.Recycle();

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

        //Converts Pixel Data from ARGB to ABGR
        private static void ConvertToABGR(int pixelHeight, int pixelWidth, int[] pixels)
        {
            int pixelCount = pixelWidth * pixelHeight;
            for (int i = 0; i < pixelCount; ++i)
            {
                uint pixel = (uint)pixels[i];
                pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
        }
    }
}

