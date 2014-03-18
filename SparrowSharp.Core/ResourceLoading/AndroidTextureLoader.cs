using OpenTK.Graphics.ES20;
using Android.Graphics;
using Sparrow.Textures;
using Android.Opengl;

namespace Sparrow.ResourceLoading
{
    public class AndroidTextureLoader : LoaderBase
    {
        public static global::Android.Content.Context _context;

        public delegate void EventHandler(GLTexture resource,LoaderBase resourceLoader);

        public event EventHandler ResourceLoaded;

        protected GLTexture _resource;

        /// <summary>
        /// Loads an image based on resource ID, only avaiable on Andoid. 
        /// The resource will be scaled based on screen DPI, put it into the drawable-nodpi folder if you dont want this.
        /// </summary>
        public LoaderBase LoadAndroidResource(int AndroidResourceId)
        {
            _isLoaded = false;
            Bitmap bitmap = BitmapFactory.DecodeResource(_context.Resources, AndroidResourceId);
            GenerateTexture(bitmap);
            return this;
        }

        override protected void DecodeRawResult(byte[] data)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            GenerateTexture(bitmap);
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
            GLTexture texture = new GLTexture(name, bitmap.Width, bitmap.Height, false, 1.0f, false);
            _resource = texture;
            _isLoaded = true;
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = ResourceLoaded;
            if (handler != null)
            {
                handler(_resource, this);
            }
        }

        public GLTexture GetResource()
        {
            return _resource;
        }
    }
}

