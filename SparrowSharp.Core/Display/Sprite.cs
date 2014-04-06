using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Core;

namespace Sparrow.Display
{
    /// <summary>
    /// A Sprite is the most lightweight, non-abstract container class. 

    /// Use it as a simple means of grouping objects together in one coordinate system.

    /// Sprite sprite = new Sprite();
    /// // create children
    /// Image venus = TextureLoader.LoadLocalImage("../venus.png");
    /// Image mars = TextureLoader.LoadLocalImage("mars.png");
    /// // move children to some relative positions
    /// venus.X = 50;
    /// mars.X = -20;
    /// // add children to the sprite
    /// sprite.AddChild(venus);
    /// sprite.AddChild(mars);
    /// // calculate total width of all children
    /// float totalWidth = sprite.Width;
    /// // rotate the whole group
    /// sprite.Rotation = Math.PI;

    /// **Flattened Sprites**

    /// The 'Flatten' method allows you to optimize the rendering of static parts of your display list.

    /// It analyzes the tree of children attached to the sprite and optimizes the rendering calls
    /// in a way that makes rendering extremely fast. The speed-up comes at a price, though: you will
    /// no longer see any changes in the properties of the children (position, rotation, alpha, etc).
    /// To update the object after changes have happened, simply call 'flatten' again, or 'unflatten'
    /// the object.
    /// </summary>
    public class Sprite : DisplayObjectContainer
    {
        private List<QuadBatch> _flattenedContents = new List<QuadBatch>();
        private bool _flattenRequested = false;
        /// <summary>
        /// The sprite's clipping rectangle in its local coordinate system. Only pixels within this
        /// rectangle will be drawn. The clipping rectangle is axis aligned with the screen, so it will
        /// not be rotated or skewed if the sprite is.
        /// </summary>
        public Rectangle ClipRect;

        /// <summary>
        /// Optimizes the sprite for optimal rendering performance. Changes in the children of a flattened
        /// sprite will not be displayed any longer. For this to happen, either call 'Flatten' again, or
        /// 'Unflatten' the sprite. Beware that the actual flattening will not happen right away, but right
        /// before the next rendering.
        /// </summary>
        public void Flatten()
        {
            _flattenRequested = true;
            //[self broadcastEventWithType:SPEventTypeFlatten];
        }

        /// <summary>
        /// Removes the rendering optimizations that were created when flattening the sprite.
        /// Changes to the sprite's children will immediately become visible again.
        /// </summary>
        public void Unflatten()
        {
            _flattenRequested = false;
            _flattenedContents.Clear();
        }

        /// <summary>
        /// Returns true if this sprite has been flattened.
        /// </summary>
        public bool IsFlattened { get { return (_flattenedContents.Count > 0 || _flattenRequested); } }

        /// <summary>
        /// Returns the bounds of the container's clipRect in the given coordinate space, or
        /// null if the sprite doens't have a clipRect.
        /// </summary>
        public Rectangle ClipRectInSpace(DisplayObject targetSpace)
        {
            if (ClipRect == null)
            {
                return null;
            }
				
            float minX = float.MaxValue;
            float maxX = -float.MaxValue;
            float minY = float.MaxValue;
            float maxY = -float.MaxValue;

            float clipLeft = ClipRect.Left;
            float clipRight = ClipRect.Right;
            float clipTop = ClipRect.Top;
            float clipBottom = ClipRect.Bottom;

            Matrix transform = TransformationMatrixToSpace(targetSpace);

            float x = 0;
            float y = 0;

            for (int i = 0; i < 4; ++i)
            {
                switch (i)
                {
                    case 0:
                        x = clipLeft;
                        y = clipTop;
                        break;
                    case 1:
                        x = clipLeft;
                        y = clipBottom;
                        break;
                    case 2:
                        x = clipRight;
                        y = clipTop;
                        break;
                    case 3:
                        x = clipRight;
                        y = clipBottom;
                        break;
                }

                Point transformedPoint = transform.TransformPoint(x, y);
                if (minX > transformedPoint.X)
                {
                    minX = transformedPoint.X;
                }
                if (maxX < transformedPoint.X)
                {
                    maxX = transformedPoint.X;
                }
                if (minY > transformedPoint.Y)
                {
                    minY = transformedPoint.Y;
                }
                if (maxY < transformedPoint.Y)
                {
                    maxY = transformedPoint.Y;
                }
            }
            return new Rectangle(minX, minY, maxX - minX, maxY - minY); // TODO: negative value checks?
        }

        override public void Render(RenderSupport support)
        {
            if (ClipRect != null)
            {
                Rectangle stageClipRect = support.PushClipRect(ClipRectInSpace(Stage));
                if (stageClipRect != null || stageClipRect.IsEmpty())
                {
                    // empty clipping bounds - no need to render children
                    support.PopClipRect();
                    return;
                }
            }

            if (_flattenRequested)
            {
                _flattenedContents = QuadBatch.Compile(this, _flattenedContents);
                _flattenRequested = false;
            }

            if (_flattenedContents.Count > 0)
            {
                support.FinishQuadBatch();
                support.AddDrawCalls(_flattenedContents.Count);

                Matrix mvpMatrix = support.MvpMatrix;
                float alpha = support.Alpha;
                uint supportBlendMode = support.BlendMode;

                foreach (QuadBatch quadBatch in _flattenedContents)
                {
                    uint blendMode = quadBatch.BlendMode;
                    if (blendMode == Sparrow.Display.BlendMode.AUTO)
                        blendMode = supportBlendMode;

                    quadBatch.Render(mvpMatrix, alpha, blendMode);
                }
            }
            else
            {
                base.Render(support);
            }

            if (ClipRect != null)
            {
                support.PopClipRect();
            }
        }

        override public Rectangle BoundsInSpace(DisplayObject targetSpace)
        {
            Rectangle bounds = base.BoundsInSpace(targetSpace);
            // if we have a scissor rect, intersect it with our bounds
            if (ClipRect != null)
            {
                bounds = bounds.Intersection(ClipRectInSpace(targetSpace));
            }
            return bounds;
        }

        override public DisplayObject HitTestPoint(Point localPoint)
        {
            if (ClipRect != null && !ClipRect.Contains(localPoint))
            {
                return null;
            }
            return base.HitTestPoint(localPoint);
        }
    }
}