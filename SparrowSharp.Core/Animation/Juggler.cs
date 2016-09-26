
using System;
using System.Collections.Generic;

namespace Sparrow.Animation
{
    /// <summary>
    /// The Juggler takes objects that implement IAnimatable (e.g. 'MovieClip's) and executes them.
    /// 
    /// A juggler is a simple object. It does no more than saving a list of objects implementing 
    /// 'Animatable' and advancing their time if it is told to do so (by calling its own 'AdvanceTime'
    /// method). Furthermore, an object can request to be removed from the juggler by dispatching an
    /// 'RemoveFromJuggler' event.
    /// 
    /// There is a default juggler that you can access from anywhere with the following code:
    /// 
    /// Juggler juggler = SparrowSharp.DefaultJuggler;
    /// 
    /// You can, however, create juggler objects yourself, too. That way, you can group your game 
    /// into logical components that handle their animations independently.
    /// 
    /// A cool feature of the juggler is to delay method calls. Say you want to remove an object from its
    /// parent 2 seconds from now. Call:
    /// 
    /// juggler.DelayInvocationAtTarget(object.RemoveFromParent, 2.0f);
    /// 
    /// This line of code will execute the following method 2 seconds in the future:
    /// 
    /// object.RemoveFromParent();
    /// 
    /// Alternatively, you can use the block-based verson of the method:
    /// 
    /// juggler.DelayInvocationByTime(delegate{ object.RemoveFromParent(); }, 2.0f);
    /// </summary>
    public class Juggler : IAnimatable
    {
        private readonly List<IAnimatable> _objects;
        private Dictionary<IAnimatable, uint> _objectIDs;
        private double _elapsedTime;
        private float _timeScale;

        private static uint sCurrentObjectID;

        public event RemoveFromJugglerHandler RemoveFromJugglerEvent;

        public delegate void RemoveFromJugglerHandler(IAnimatable objectToRemove);

        public Juggler()
        {
            _elapsedTime = 0;
            _timeScale = 1.0f;
            _objects = new List<IAnimatable>();
            _objectIDs = new Dictionary<IAnimatable, uint>();
        }
       
        /// <summary>
        /// Adds an object to the juggler.
        /// </summary>
        public uint Add(IAnimatable animatable)
        {
            return Add(animatable, GetNextID());
        }

        private uint Add(IAnimatable objectToAdd, uint objectID)
        {
            if (objectToAdd != null && !_objectIDs.ContainsKey(objectToAdd))
            {
                objectToAdd.RemoveFromJugglerEvent += OnRemove;
                _objects.Add(objectToAdd);
                _objectIDs[objectToAdd] = objectID;

                return objectID;
            }
            else return 0;
        }

        /// <summary>
        /// Determines if an object has been added to the juggler.
        /// </summary>
        public bool Contains(IAnimatable animatable)
        {
            return _objects.Contains(animatable);
        }

        /// <summary>
        /// Removes an object from the juggler.
        /// </summary>
        public uint Remove(IAnimatable objectToRemove)
        {
            uint objectID = 0;

            if (objectToRemove != null && !_objectIDs.ContainsKey(objectToRemove))
            {
                objectToRemove.RemoveFromJugglerEvent -= OnRemove;
                
                _objects.Remove(objectToRemove);

                objectID = _objectIDs[objectToRemove];
                _objectIDs.Remove(objectToRemove);
            }
            return objectID;
        }

        /** Removes an object from the juggler, identified by the unique numeric identifier you
         *  received when adding it.
         *
         *  <p>It's not uncommon that an animatable object is added to a juggler repeatedly,
         *  e.g. when using an object-pool. Thus, when using the <code>remove</code> method,
         *  you might accidentally remove an object that has changed its context. By using
         *  <code>removeByID</code> instead, you can be sure to avoid that, since the objectID
         *  will always be unique.</p>
         *
         *  @return if successful, the passed objectID; if the object was not found, zero.
         */
        public uint RemoveByID(uint objectID)
        {
            for (int i = _objects.Count - 1; i >= 0; --i)
            {
                IAnimatable obj = _objects[i];

                if (_objectIDs[obj] == objectID)
                {
                    Remove(obj);
                    return objectID;
                }
            }
            return 0;
        }

        // TODO add removeTweens, containsTweens

        /** Removes all objects at once. */
        public void Purge()
        {
            // the object vector is not purged right away, because if this method is called 
            // from an 'advanceTime' call, this would make the loop crash. Instead, the
            // vector is filled with 'null' values. They will be cleaned up on the next call
            // to 'advanceTime'.
            
            for (int i = _objects.Count - 1; i >= 0; --i)
            {
                IAnimatable obj = _objects[i];
                obj.RemoveFromJugglerEvent -= OnRemove;

                _objects[i] = null;
                _objectIDs.Remove(obj);
            }
        }

        /** Advances all objects by a certain time (in seconds). */
        public void AdvanceTime(float time)
        {

            int numObjects = _objects.Count;
            int currentIndex = 0;
            int i;

            time *= _timeScale;
            if (numObjects == 0 || time == 0) return;
            _elapsedTime += time;
            
            // there is a high probability that the "advanceTime" function modifies the list 
            // of animatables. we must not process new objects right now (they will be processed
            // in the next frame), and we need to clean up any empty slots in the list.
            
            for (i=0; i<numObjects; ++i)
            {
                IAnimatable obj = _objects[i];
                if (obj != null)
                {
                    // shift objects into empty slots along the way
                    if (currentIndex != i) 
                    {
                        _objects[currentIndex] = obj;
                        _objects[i] = null;
                    }

                    obj.AdvanceTime(time);
                    ++currentIndex;
                }
            }
            
            if (currentIndex != i)
            {
                numObjects = _objects.Count; // count might have changed!
                
                while (i<numObjects)
                    _objects[currentIndex++] = _objects[i++];

                //_objects.length = currentIndex;
                if (currentIndex < _objects.Count)
                {
                    _objects.RemoveRange(currentIndex, _objects.Count - currentIndex);
                }
            }
        }

        private void OnRemove(IAnimatable objectToRemove)
        {
            uint objectID = Remove(objectToRemove);

            if (objectID != 0)
            {
                /*
                Tween tween = objectToRemove as Tween;
                if (tween != null && tween.IsComplete)
                    AddWithID(tween.nextTween, objectID);
                */
            }
        }


        private static uint GetNextID() { return ++sCurrentObjectID; }

        /// <summary>
        /// The total life time of the juggler.
        /// </summary>
        public double ElapsedTime
        {
            get { return _elapsedTime; }
        }

        /// <summary>
        /// The scale at which the time is passing. This can be used for slow motion or time laps
        /// effects.Values below '1' will make all animations run slower, values above '1' faster.
        /// </summary>
        public float TimeScale
        {
            get { return _timeScale; }
            set
            {
                if (value < 0.0f)
                {
                    throw new Exception("Speed can not be less than 0");
                }
                _timeScale = value;
            }
        }

        /** The actual vector that contains all objects that are currently being animated. */
        protected List<IAnimatable> Objects { get { return _objects; } }
    }
}