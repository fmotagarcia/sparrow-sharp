﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using Sparrow.Textures;

namespace Sparrow.ResourceLoading
{
    public class TextureLoader
    {
        protected bool _isLoaded;
        protected Texture _glTexture;

        public bool IsLoaded => _isLoaded;

        public Texture Texture => _glTexture;

        public event EventHandler<Texture> ResourceLoaded;

        public TextureLoader LoadRemoteImage(string remoteUrl)
        {
            throw new NotImplementedException();
            _isLoaded = false;
            return this; 
        }

        public Texture LoadLocalImage(string pathToFile)
        {
            _isLoaded = false;
            using (Image<Rgba32> image = Image.Load(pathToFile))
            {
                GenerateTexture(image);
            }
            return _glTexture;
        }

        public TextureLoader LoadLocalImageAsync(string pathToFile)
        {
            _isLoaded = false;
            LoadLocalBitmapAsync(pathToFile);
            // + check wether the async call can be executed instantly, 
            // because in that case it will be impossible to catch the event
            return this; 
        }

        public Texture LoadFromStream(Stream stream)
        {
            _isLoaded = false;
            using (Image<Rgba32> image = Image.Load(stream))
            {
                GenerateTexture(image);
            }
            return _glTexture;
        }

        private async void LoadLocalBitmapAsync(string path)
        {
            throw new NotImplementedException();
        }

        private void GenerateTexture(Image<Rgba32> image)
        {
            _isLoaded = false;
            
            TextureOptions opts = new TextureOptions(TextureFormat.Rgba8888);
            byte[] data = image.SavePixelData();
            int len = image.Width * image.Height * 4;
            
            // Premultiply alpha
            for (int i = 0; i < len; i += 4)
            {
                float alpha = (float)data[i + 3] / 255;
                byte r = data[i + 0];
                byte g = data[i + 1];
                byte b = data[i + 2];
                data[i + 0] = (byte)(r * alpha);
                data[i + 1] = (byte)(g * alpha);
                data[i + 2] = (byte)(b * alpha);
                data[i + 3] = (byte)(alpha * 255);
            }
            
            _glTexture = Texture.FromData(data, opts, image.Width, image.Height);

            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler<Texture> handler = ResourceLoaded;
            handler?.Invoke(this, _glTexture);
        }
    }
}