using Sparrow.Core;
using Sparrow.Geom;
using System;
using System.Collections.Generic;

namespace Sparrow.Display
{
    public class DisplayObjectContainer : DisplayObject
    {
        public int NumChildren { get; set; }

        private List<DisplayObject> _children;

        public DisplayObjectContainer() : base()
        {
            #if DEBUG
            if (this.GetType().IsInstanceOfType(typeof(DisplayObjectContainer)))
            { 
                throw new Exception("Attempting to instantiate DisplayObjectContainer directly."); 
            }    
            #endif
            _children = new List<DisplayObject>();
        }

        public void AddChild(DisplayObject child)
        {
            AddChild(child, _children.Count);
        }

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
        // this function not a typo; I think it should be called something like IsChildOf
        public bool ContainsChild(DisplayObject child)
        {
            while (child != null)
            {
                if (child == this)
                    return true;
                else
                    child = child.Parent;
            }
            return false;
        }

        public DisplayObject GetChild(uint index)
        {
            return _children[(int)index];
        }

        public DisplayObject GetChild(int index)
        {
            return _children[index];
        }

        public DisplayObject GetChild(String name)
        {
            foreach (DisplayObject currentChild in _children)
            {
                // TODO double check if == works this way for strings
                if (currentChild.name == name)
                    return currentChild;
            }
            return null;
        }

        public int GetChildIndex(DisplayObject child)
        {
            int index = _children.IndexOf(child);
            if (index != -1)
            {
                return index;
            }
            // I really hate this in AS, IMO we should just return -1.
            throw new ArgumentException("child not found");
        }

        public void SetChildIndex(int index, DisplayObject child)
        {
            int oldIndex = _children.IndexOf(child); 
            if (oldIndex == -1)
            {
                throw new ArgumentException("child not found");
            }
            else
            {
                _children.RemoveAt(oldIndex);
                _children.Insert(index, child);
            }
        }

        public void RemoveChild(DisplayObject child)
        {
            int index = _children.IndexOf(child);
            if (index != -1)
            {
                RemoveChildAt(index);
            }
        }

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

        public void SwapChild(DisplayObject child1, DisplayObject child2)
        {
            int index1 = _children.IndexOf(child1);
            int index2 = _children.IndexOf(child2);
            SwapChildrenAt(index1, index2);
        }

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

        public void RemoveAllChildren()
        {
            for (int i = (int)_children.Count - 1; i >= 0; --i)
            {
                RemoveChildAt(i);
            }
        }

        override public void Render(RenderSupport support)
        {
            foreach (DisplayObject child in _children)
            {
                if (child.HasVisibleArea)
                {
                    support.PushState(child.TransformationMatrix, child.Alpha, child.BlendMode);

                    //if (child.HasFilter ??) 
                    //{
                    //    child.Filter(child, support);
                    //}
                    //else
                    //{
                    child.Render(support);
                    //}
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
                Point transformedPoint = TransformationMatrix.TransformPoint(X, Y);
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
            if (!visible || !touchable)
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
    }
}

