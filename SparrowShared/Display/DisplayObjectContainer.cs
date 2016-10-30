using System;
using System.Collections.Generic;
using Sparrow.Geom;
using Sparrow.Rendering;
using Sparrow.Filters;

namespace Sparrow.Display
{
    /// <summary>
    /// A DisplayObjectContainer represents a collection of display objects.
    /// 
    /// It is the base class of all display objects that act as a container for other objects. By 
    /// maintaining an ordered list of children, it defines the back-to-front positioning of the children
    /// within the display tree.
    /// 
    /// A container does not have size in itself. The width and height properties represent the extents
    /// of its children. Changing those properties will scale all children accordingly.
    /// 
    /// As this is an abstract class, you can't instantiate it directly, but have to 
    /// use a subclass instead. The most lightweight container class is Sprite.
    /// 
    /// **Adding and removing children**
    /// 
    /// The class defines methods that allow you to add or remove children. When you add a child, it will
    /// be added at the foremost position, possibly occluding a child that was added before. You can access
    /// the children via an index. The first child will have index 0, the second child index 1, etc. 
    /// 
    /// Adding and removing objects from a container triggers events.
    /// 
    /// - 'Added': the object was added to a DisplayObjectContainer.
    /// - 'AddedToStage': the object was added to a DisplayObjectContainer that is connected to the stage.
    /// - 'Removed': the object was removed from a DisplayObjectContainer.
    /// - 'RemovedFromStage': the object was removed from a DisplayObjectContainer that is connected to the stage.
    /// 
    /// Especially the AddedToStage event is very helpful, as it allows you to automatically execute
    /// some logic (e.g. start an animation) when an object is rendered the first time.
    /// 
    /// **Sorting children**
    /// 
    /// The 'sortChildren' method allows you to sort the children of a container by a custom criteria. 
    /// Below is an example how to depth-sort children by their y-coordinate; this will put objects that
    /// are lower on the screen in front of those higher on the screen.
    /// <code>
    /// public class CompareExample : IComparator {
    ///     public int Compare(DisplayObject child1, DisplayObject child2) 
    ///     {
    ///         if (child1.Y &lt; child2.Y) return -1;
    ///         else if (child1.Y &gt; child2.Y) return 1;
    ///         else return 0;
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class DisplayObjectContainer : DisplayObject
    {

        private static BatchToken sCacheToken = new BatchToken();

        /// <summary>
        /// Disposes the resources of all children. 
        /// </summary>
        public override void Dispose()
        {
            for (int i =_children.Count - 1; i >= 0; --i)
            {
                _children[i].Dispose();
            }
            base.Dispose();
        }

        /// <summary>
        /// The number of children of this container.
        /// </summary>
        public int NumChildren { get { return _children.Count; } }

        private readonly List<DisplayObject> _children = new List<DisplayObject>();

        /// <summary>
        /// Returns the children of this container 
        /// </summary>
        /// <value>The children.</value>
        public IReadOnlyList<DisplayObject> Children
        {
            get { return _children; }
        }

        /// <summary>
        ///  Adds a child to the container. It will be at the topmost position.
        /// </summary>
        public void AddChild(DisplayObject child)
        {
            AddChild(child, _children.Count);
        }

        /// <summary>
        /// Adds a child to the container at a certain index.
        /// </summary>
        public void AddChild(DisplayObject child, int index)
        {
            if (index >= 0 && index <= _children.Count)
            {
                SetRequiresRedraw();

                if (child.Parent == this)
                {
                    SetChildIndex(child, index); // avoids dispatching events
                }
                else
                {
                    child.RemoveFromParent();
                    _children.Insert(index, child); // TODO this is different in Starling
                    child.Parent = this;

                    child.InvokeAdded(child, this);
                    if (Stage != null)
                    {
                        child.BroadcastAddedToStageEvent(this);
                    }
                }
            }
            else
            {
                throw new IndexOutOfRangeException("Invalid child index " + index); 
            }
        }

        /// <summary>
        /// Determines if a certain object is a child of this container (recursively).
        /// </summary>
        public bool Contains(DisplayObject child)
        {
            while (child != null)
            {
                if (child == this)
                {
                    return true;
                }
                child = child.Parent;
            }
            return false;
        }

        /// <summary>
        /// Returns a child object at a certain index.
        /// </summary>
        public DisplayObject GetChild(uint index)
        {
            return _children[(int)index];
        }

        /// <summary>
        /// Returns a child object at a certain index.
        /// </summary>
        public DisplayObject GetChild(int index)
        {
            return _children[index];
        }

        /// <summary>
        /// Returns a child object with a certain name (non-recursively).
        /// </summary>
        public DisplayObject GetChild(string name)
        {
            foreach (DisplayObject currentChild in _children)
            {
                if (currentChild.Name == name)
                {
                    return currentChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the index of a child within the container. Returns -1 if the child is not within this container
        /// </summary>
        public int GetChildIndex(DisplayObject child)
        {
            return _children.IndexOf(child);
        }

        /// <summary>
        /// Moves a child to a certain index. Children at and after the replaced position move up.
        /// throws ArgumentException if the child is not found
        /// </summary>
        public void SetChildIndex(DisplayObject child, int index)
        {
            int oldIndex = _children.IndexOf(child); 
            if (oldIndex == -1)
            {
                throw new ArgumentException("child not found");
            }
            _children.RemoveAt(oldIndex);
            _children.Insert(index, child);
            SetRequiresRedraw();
        }

        /// <summary>
        /// Removes a child from the container. If the object is not a child, nothing happens.
        /// </summary>
        public DisplayObject RemoveChild(DisplayObject child, bool dispose = false)
        {
            int index = _children.IndexOf(child);
            if (index != -1)
            {
                return RemoveChildAt(index, dispose);
            }
            return null;
        }

        /// <summary>
        /// Removes a child at a certain index. Children above the child will move down.
        /// </summary>
        public DisplayObject RemoveChildAt(int index, bool dispose = false)
        {
            if (index >= 0 && index < _children.Count)
            {
                SetRequiresRedraw(); 

                DisplayObject child = _children[index];
                child.InvokeRemoved();

                if (Stage != null)
                {
                    child.BroadcastRemovedFromStageEvent(this);
                }

                child.Parent = null;
                int newIndex = _children.IndexOf(child); // index might have changed in event handler
                if (newIndex != -1)
                {
                    _children.RemoveAt(newIndex);
                }
                if (dispose) child.Dispose();

                return child;
            }
            else
            {
                throw new IndexOutOfRangeException("Invalid child index");
            }
        }

        /// <summary>
        /// Swaps the indexes of two children.
        /// </summary>
        public void SwapChild(DisplayObject child1, DisplayObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            SwapChildrenAt(index1, index2);
        }

        /// <summary>
        /// Swaps the indexes of two children.
        /// </summary>
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
            SetRequiresRedraw();
        }

        /// <summary>
        /// Removes all children from the container.
        /// </summary>
        public void RemoveAllChildren()
        {
            for (int i = _children.Count - 1; i >= 0; --i)
            {
                RemoveChildAt(0);
            }
        }

        override public void Render(Painter painter)
        {
            int numChildren = _children.Count;
            uint frameID = painter.FrameID;
            bool cacheEnabled = frameID == 0 ? false : true;
            bool selfOrParentChanged = _lastParentOrSelfChangeFrameID == frameID;

            for (int i = 0; i < numChildren; ++i)
            {
                DisplayObject child = _children[i];

                if (child._hasVisibleArea)
                {
                    if (selfOrParentChanged)
                    {
                        child._lastParentOrSelfChangeFrameID = frameID;
                    }

                    if (child._lastParentOrSelfChangeFrameID != frameID &&
                        child._lastChildChangeFrameID != frameID &&
                        child._tokenFrameID == frameID - 1 && cacheEnabled)
                    {
                        painter.PushState(sCacheToken);
                        painter.DrawFromCache(child._pushToken, child._popToken);
                        painter.PopState(child._popToken);

                        child._pushToken.CopyFrom(sCacheToken);
                    }
                    else
                    {
                        BatchToken pushToken = cacheEnabled ? child._pushToken : null;
                        BatchToken popToken = cacheEnabled ? child._popToken : null;
                        FragmentFilter filter = child._filter;
                        DisplayObject mask = child._mask;
                        
                        painter.PushState(pushToken);
                        painter.SetStateTo(child.TransformationMatrix, child.Alpha, child.BlendMode);

                        if (mask != null) painter.DrawMask(mask, child);

                        if (filter != null) filter.Render(painter);
                        else child.Render(painter);

                        if (mask != null) painter.EraseMask(mask, child);

                        painter.PopState(popToken);
                    }

                    if (cacheEnabled)
                    {
                        child._tokenFrameID = frameID;
                    }
                }
            }
        }

        override public Rectangle GetBounds(DisplayObject targetSpace)
        {
            int numChildren = _children.Count;

            if (numChildren == 0)
            {
                Matrix2D transformationMatrix = GetTransformationMatrix(targetSpace);
                Point transformedPoint = transformationMatrix.TransformPoint(X, Y);
                return Rectangle.Create(transformedPoint.X, transformedPoint.Y);
            }
            else if (numChildren == 1)
            {
                return _children[0].GetBounds(targetSpace);
            }
            else
            {
                float minX = float.MaxValue, maxX = -float.MaxValue, minY = float.MaxValue, maxY = -float.MaxValue;
                foreach (DisplayObject child in _children)
                {
                    Rectangle childBounds = child.GetBounds(targetSpace);
                    minX = Math.Min(minX, childBounds.X);
                    maxX = Math.Max(maxX, childBounds.X + childBounds.Width);
                    minY = Math.Min(minY, childBounds.Top);
                    maxY = Math.Max(maxY, childBounds.Top + childBounds.Height);
                }
                return Rectangle.Create(minX, minY, maxX - minX, maxY - minY);
            }
        }

        override public DisplayObject HitTest(Point localPoint)
        {
            if (!Visible || !Touchable || !HitTestMask(localPoint))
            {
                return null;
            }

            for (int i = _children.Count - 1; i >= 0; --i)
            { 
                // front to back!
                DisplayObject child = _children[i];
                if (child.IsMask) continue;


                Matrix2D transformationMatrix = Matrix2D.Create();
                transformationMatrix.CopyFromMatrix(child.TransformationMatrix);
                transformationMatrix.Invert();

                Point transformedPoint = transformationMatrix.TransformPoint(localPoint);
                DisplayObject target = child.HitTest(transformedPoint);
                if (target != null)
                {
                    return target;
                }
            }
            return null;
        }

        /// <summary>
        /// Sorts the children using the given IComparer.
        /// </summary>
        public void SortChildren(IComparer<DisplayObject> comparator)
        {
            _children.Sort(comparator);
            SetRequiresRedraw();
        }
    }
}

