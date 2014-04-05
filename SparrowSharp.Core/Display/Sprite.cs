using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Core;

namespace Sparrow.Display
{
	 
	public class Sprite : DisplayObjectContainer
	{
		private List<QuadBatch> _flattenedContents = new List<QuadBatch>();
		private bool _flattenRequested = false;
		public Rectangle ClipRect;

		public void Flatten() {
			_flattenRequested = true;
			//[self broadcastEventWithType:SPEventTypeFlatten];
		}

		public void Unflatten() {
			_flattenRequested = false;
			_flattenedContents.Clear ();
		}

		public bool IsFlattened() {
			return (_flattenedContents.Count > 0 || _flattenRequested);
		}

		public Rectangle ClipRectInSpace(DisplayObject targetSpace)
		{
			if (ClipRect == null)
				return null;

			float minX =  float.MaxValue;
			float maxX = -float.MaxValue;
			float minY =  float.MaxValue;
			float maxY = -float.MaxValue;

			float clipLeft = ClipRect.Left;
			float clipRight = ClipRect.Right;
			float clipTop = ClipRect.Top;
			float clipBottom = ClipRect.Bottom;

			Matrix transform = TransformationMatrixToSpace(targetSpace);

			float x = 0;
			float y = 0;

			for (int i=0; i<4; ++i)
			{
				switch (i)
				{
					case 0: x = clipLeft;  y = clipTop;    break;
					case 1: x = clipLeft;  y = clipBottom; break;
					case 2: x = clipRight; y = clipTop;    break;
					case 3: x = clipRight; y = clipBottom; break;
				}

				Point transformedPoint = transform.TransformPoint(x, y);
				if (minX > transformedPoint.X) minX = transformedPoint.X;
				if (maxX < transformedPoint.X) maxX = transformedPoint.X;
				if (minY > transformedPoint.Y) minY = transformedPoint.Y;
				if (maxY < transformedPoint.Y) maxY = transformedPoint.Y;
			}
			return new Rectangle(minX, minY, maxX-minX, maxY-minY); // TODO: negative value checks?
		}

		override public void Render(RenderSupport support)
		{
			if (ClipRect != null)
			{
				Rectangle stageClipRect = support.PushClipRect (ClipRectInSpace(Stage));
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
				support.AddDrawCalls( _flattenedContents.Count );

				Matrix mvpMatrix = support.MvpMatrix;
				float alpha = support.Alpha;
				uint supportBlendMode = support.BlendMode;

				foreach (QuadBatch quadBatch in _flattenedContents)
				{
					uint blendMode = quadBatch.BlendMode;
					if (blendMode == Sparrow.Display.BlendMode.AUTO) blendMode = supportBlendMode;

					quadBatch.Render(mvpMatrix, alpha, blendMode);
				}
			}
			else base.Render(support);

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