using Sparrow.Core;
using Sparrow.Geom;
using System;
using System.Collections.Generic;

namespace Sparrow.Display
{
	/** 
	 A DisplayObjectContainer represents a collection of display objects.
	 
	 It is the base class of all display objects that act as a container for other objects. By 
	 maintaining an ordered list of children, it defines the back-to-front positioning of the children
	 within the display tree.
	 
	 A container does not have size in itself. The width and height properties represent the extents
	 of its children. Changing those properties will scale all children accordingly.
	 
	 As this is an abstract class, you can't instantiate it directly, but have to 
	 use a subclass instead. The most lightweight container class is Sprite.
	 
	 **Adding and removing children**
	 
	 The class defines methods that allow you to add or remove children. When you add a child, it will
	 be added at the foremost position, possibly occluding a child that was added before. You can access
	 the children via an index. The first child will have index 0, the second child index 1, etc. 
	 
	 Adding and removing objects from a container triggers non-bubbling events.
	 
	 - `EventTypeAdded`: the object was added to a parent.
	 - `EventTypeAddedToStage`: the object was added to a parent that is connected to the stage,
	                                   thus becoming visible now.
	 - `EventTypeRemoved`: the object was removed from a parent.
	 - `EventTypeRemovedFromStage`: the object was removed from a parent that is connected to 
	                                       the stage, thus becoming invisible now.
	 
	 Especially the `ADDED_TO_STAGE` event is very helpful, as it allows you to automatically execute
	 some logic (e.g. start an animation) when an object is rendered the first time.
	 
	 **Sorting children**
	 
	 The `sortChildren` method allows you to sort the children of a container by a custom criteria. 
	 Below is an example how to depth-sort children by their y-coordinate; this will put objects that
	 are lower on the screen in front of those higher on the screen.
	 
		container.SortChildren(DisplayObject child1, DisplayObject *child2) 
		{
		    if (child1.Y < child2.Y) return OrderedAscending;
		    else if (child1.Y > child2.Y) return OrderedDescending;
		    else return OrderedSame;
		}];
	 */
	public abstract class DisplayObjectContainer : DisplayObject
    {
		/// The number of children of this container.
		public int NumChildren { get {return _children.Count; } }

		private readonly List<DisplayObject> _children = new List<DisplayObject>();

		/// Adds a child to the container. It will be at the topmost position.
        public void AddChild(DisplayObject child)
        {
            AddChild(child, _children.Count);
        }

		/// Adds a child to the container at a certain index.
        public void AddChild(DisplayObject child, int index)
        {
            if (index >= 0 && index <= _children.Count)
            {
                child.RemoveFromParent();
                _children.Insert(index, child);
                child.Parent = this;

                DispatchAddedEventWithBubbling();
                if (Stage != null)
                {
                    BroadcastAddedToStageEvent();
                }
            }
            else
                throw new IndexOutOfRangeException("Invalid child index"); 
        }

        protected void DispatchAddedEventWithBubbling()
        {
            DisplayObject node = this;
            while (node != null)
            {
                node.InvokeAdded(this, node);
                node = node.Parent;
            }
        }

        protected void BroadcastAddedToStageEvent()
        {
            InvokeAddedToStage(this, this);
            foreach (var child in _children)
            {
                child.InvokeAddedToStage(this, child);
            }
        }

        protected void BroadcastEnterFrameEvent(float passedTime)
        {
            InvokeEnterFrame(this, this, passedTime);
            foreach (var child in _children)
            {
                child.InvokeEnterFrame(this, child, passedTime);
            }
        }

		/// Determines if a certain object is a child of the container (recursively).
        public bool ContainsChild(DisplayObject child)
        {
            while (child != null)
            {
                if (child == this)
                    return true;
                child = child.Parent;
            }
		    return false;
        }

		/// Returns a child object at a certain index.
        public DisplayObject GetChild(uint index)
        {
            return _children[(int)index];
        }

		/// Returns a child object at a certain index.
        public DisplayObject GetChild(int index)
        {
            return _children[index];
        }

		/// Returns a child object with a certain name (non-recursively).
        public DisplayObject GetChild(String name)
        {
            foreach (DisplayObject currentChild in _children)
            {
                if (currentChild.Name == name)
                    return currentChild;
            }
            return null;
        }

		/// Returns the index of a child within the container. Returns -1 if the child is not within this container
        public int GetChildIndex(DisplayObject child)
        {
			return _children.IndexOf(child);
        }

		/// Moves a child to a certain index. Children at and after the replaced position move up.
		/// @throws ArgumentException if the child is not found
        public void SetChildIndex(int index, DisplayObject child)
        {
            int oldIndex = _children.IndexOf(child); 
            if (oldIndex == -1)
            {
                throw new ArgumentException("child not found");
            }
		    _children.RemoveAt(oldIndex);
		    _children.Insert(index, child);
        }

		/// Removes a child from the container. If the object is not a child, nothing happens.
        public void RemoveChild(DisplayObject child)
        {
            int index = _children.IndexOf(child);
            if (index != -1)
            {
                RemoveChildAt(index);
            }
        }

		/// Removes a child at a certain index. Children above the child will move down.
        public void RemoveChildAt(int index)
        {
            if (index >= 0 && index < _children.Count)
            {
                DisplayObject child = _children[index];
                //[child dispatchEventWithType:SPEventTypeRemoved];
                if (Stage != null)
                {
                    //[child broadcastEventWithType:SPEventTypeRemovedFromStage];
                }
                child.Parent = null; 
                int newIndex = _children.IndexOf(child); // index might have changed in event handler
                if (newIndex != -1)
                    _children.RemoveAt(newIndex);
            }
            else
                throw new IndexOutOfRangeException("Invalid child index");        
        }

		/// Swaps the indexes of two children.
        public void SwapChild(DisplayObject child1, DisplayObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            SwapChildrenAt(index1, index2);
        }

		/// Swaps the indexes of two children.
        public void SwapChildrenAt(int index1, int index2)
        {    
            int numChildren = _children.Count;
            if (index1 < 0 || index1 >= numChildren || index2 < 0 || index2 >= numChildren)
            {
                throw new InvalidOperationException("invalid child indices");
            }
            DisplayObject tmp = _children[index1];
            _children[index1] = _children[index2];
            _children[index2] = tmp;   
        }

		/// Removes all children from the container.
        public void RemoveAllChildren()
        {
            for (int i = _children.Count - 1; i >= 0; --i)
            {
				RemoveChildAt(0);
            }
        }

        override public void Render(RenderSupport support)
        {
            foreach (DisplayObject child in _children)
            {
                if (child.HasVisibleArea)
                {
                    support.PushState(child.TransformationMatrix, child.Alpha, child.BlendMode);

					if (child.Filter != null) 
                    {
						child.Filter.RenderObject(child, support);
                    }
                    else
                    {
                    	child.Render(support);
                    }
                    support.PopState();
                }
            }
        }

        override public Rectangle BoundsInSpace(DisplayObject targetSpace)
        {
            int numChildren = _children.Count;

            if (numChildren == 0)
            {
                Matrix transformationMatrix = TransformationMatrixToSpace(targetSpace);
                Point transformedPoint = transformationMatrix.TransformPoint(X, Y);
                return new Rectangle(transformedPoint.X, transformedPoint.Y, 0.0f, 0.0f);
            }
            else if (numChildren == 1)
            {
                return _children[0].BoundsInSpace(targetSpace);
            }
            else
            {
                float minX = float.MaxValue, maxX = -float.MaxValue, minY = float.MaxValue, maxY = -float.MaxValue;
                foreach (DisplayObject child in _children)
                {
                    Rectangle childBounds = child.BoundsInSpace(targetSpace);
                    minX = Math.Min(minX, childBounds.X);
                    maxX = Math.Max(maxX, childBounds.X + childBounds.Width);
                    minY = Math.Min(minY, childBounds.Y);
                    maxY = Math.Max(maxY, childBounds.Y + childBounds.Height);
                }
                return new Rectangle(minX, minY, maxX - minX, maxY - minY);
            }
        }

        override public DisplayObject HitTestPoint(Point localPoint)
        {
            if (!Visible || !Touchable)
                return null;

            for (int i = _children.Count - 1; i >= 0; --i)
            { // front to back!
                DisplayObject child = _children[i];
                Matrix transformationMatrix = TransformationMatrixToSpace(child);
                Point transformedPoint = transformationMatrix.TransformPoint(localPoint);
                DisplayObject target = child.HitTestPoint(transformedPoint);
                if (target != null)
                    return target;
            }
            return null;
        }

		/// Sorts the children using the given NSComparator block.
		// TODO public void SortChildren(Comparator comparator);
    }
}

