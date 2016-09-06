using System;
using Sparrow.Display;
using Sparrow.Core;
using Sparrow.Utils;
using Sparrow.Geom;

namespace Sparrow.Textures
{
    /// <summary>
    // A RenderTexture is a dynamic texture on which you can draw any display object.

    // After creating a render texture, just call the 'DrawObject' method to render an object directly 
    // onto the texture. The object will be drawn onto the texture at its current position, adhering 
    // its current rotation, scale and alpha properties. 

    // Drawing is done very efficiently, as it is happening directly in graphics memory. After you have 
    // drawn objects on the texture, the performance will be just like that of a normal texture - no
    // matter how many objects you have drawn.

    // If you draw lots of objects at once, it is recommended to bundle the drawing calls in a block
    // via the 'DrawBundled' method, like shown below. That will speed it up immensely, allowing
    // you to draw hundreds of objects very quickly.

    // renderTexture.DrawBundled(delegate
    // {
    //     for (int i=0; i<numDrawings; ++i)
    //	   {
    //         image.Rotation = (2 * (float)Math.PI / numDrawings) * i;
    //	       renderTexture.DrawObject(image);            
    //     }             
    // });
    /// </summary>
    public class RenderTexture : SubTexture
    {/*
        private bool _framebufferIsActive;
        private readonly RenderSupport _renderSupport;

        /// <summary>
        /// Initializes a render texture with a certain ARGB color (0xAARRGGBB) and a scale factor.
        /// </summary>
        public RenderTexture(float width, float height, uint argbFillColor = 0x0, float scale = 1.0f)
        {
            int legalWidth = MathUtil.NextPowerOfTwo(width * scale);
            int legalHeight = MathUtil.NextPowerOfTwo(height * scale);

            TextureProperties properties = new TextureProperties
            {
                TextureFormat = Sparrow.Textures.TextureFormat.Rgba8888,
                Scale = scale,
                Width = legalWidth,
                Height = legalHeight,
                NumMipmaps = 0,
                PremultipliedAlpha = true
            };

            Rectangle region = new Rectangle(0, 0, width, height);
            GLTexture glTexture = new GLTexture(IntPtr.Zero, properties);

            Init(glTexture, region);
            _renderSupport = new RenderSupport();
            Clear(argbFillColor, ColorUtil.GetA(argbFillColor) / 255.0f);
        }

        /// <summary>
        /// Draws an object onto the texture, adhering its properties for position, scale, rotation and alpha.
        /// </summary>
        public void DrawObject(DisplayObject obj)
        {
            DrawBundled(delegate
            {
                _renderSupport.PushState(obj.TransformationMatrix, obj.Alpha, obj.BlendMode);
                obj.Render(_renderSupport);
                _renderSupport.PopState();
            });
        }

        /// <summary>
        /// Clears the texture with a certain color and alpha value.
        /// </summary>
        public void Clear(uint color, float alpha)
        {
            DrawBundled(delegate
            {
                _renderSupport.Clear(color, alpha);
            });
        }

        /// <summary>
        /// Bundles several calls to 'drawObject' together in a block. This avoids framebuffer switches.
        /// </summary>
        public void DrawBundled(Action block)
        {
            if (block == null)
            {
                return;
            }

            // the block may call a draw-method again, so we're making sure that the frame buffer switching
            // happens only in the outermost block.
            bool isDrawing = _framebufferIsActive;
            Texture previousTarget = null;

            if (!isDrawing)
            {
                _framebufferIsActive = true;

                // remember standard frame buffer
                previousTarget = _renderSupport.RenderTarget;

                GLTexture rootTexture = Root;
                float width = rootTexture.Width;
                float height = rootTexture.Height;

                // switch to the texture's framebuffer for rendering
                _renderSupport.RenderTarget = rootTexture;

                // prepare clipping and OpenGL matrices
                _renderSupport.PushClipRect(new Rectangle(0, 0, width, height));
                // NOTE: this modifies the render matrix, changes must be reverted!
                _renderSupport.SetupOrthographicProjection(0, width, height, 0);
            }

            block();

            if (!isDrawing)
            {
                _framebufferIsActive = false;

                _renderSupport.FinishQuadBatch();
                _renderSupport.NextFrame();

                // return to standard frame buffer
                _renderSupport.RenderTarget = previousTarget;
            }
        }*/
    }
}

