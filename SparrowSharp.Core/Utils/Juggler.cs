using System;
using System.Collections.Generic;
using SparrowSharp.Display;

namespace SparrowSharp.Utils
{
    public class Juggler
    {
        readonly List<IAnimatable> _objects = new List<IAnimatable>();
        private double _elapsedTime;
        private float _speed = 1.0f;

        /// <summary>
        /// Adds an object to the juggler.
        /// </summary>
        public void Add(IAnimatable animatable)
        {
            if (animatable != null && !_objects.Contains(animatable))
            {
                _objects.Add(animatable);
                //if ([(id)object isKindOfClass:[SPEventDispatcher class]])
                //    [(SPEventDispatcher *)object addEventListener:@selector(onRemove:) atObject:self
                //                                          forType:SPEventTypeRemoveFromJuggler];
            }
        }
        //private void OnRemove:(Event event)
        //{
        //    RemoveObject(event.target);
        //}
        /// <summary>
        /// Removes an object from the juggler.
        /// </summary>
        public void Remove(IAnimatable animatable)
        {
            _objects.Remove(animatable);
            //if ([(id)object isKindOfClass:[SPEventDispatcher class]])
            //    [(SPEventDispatcher *)object removeEventListenersAtObject:self
            //                                forType:SPEventTypeRemoveFromJuggler];
        }

        /// <summary>
        /// Removes all objects at once.
        /// </summary>
        public void RemoveAll()
        {
            foreach (IAnimatable animatable in _objects)
            {
                //if ([(id)object isKindOfClass:[SPEventDispatcher class]])
                //     [(SPEventDispatcher *)object removeEventListenersAtObject:self
                //                                  forType:SPEventTypeRemoveFromJuggler]; 
            }
            _objects.Clear();
        }

        /// <summary>
        /// Removes all objects with a 'target' property referencing a certain object (e.g. tweens or
        /// delayed invocations).
        /// </summary>
        public void RemoveObjectsWithTarget(IAnimatable animatable)
        {
            /* 
            SEL targetSel = @selector(target);
            NSMutableOrderedSet *remainingObjects = [[NSMutableOrderedSet alloc] init];
    
            for (id currentObject in _objects)
            {
                if (![currentObject respondsToSelector:targetSel] || ![[currentObject target] isEqual:object])
                    [remainingObjects addObject:currentObject];
                else if ([(id)currentObject isKindOfClass:[SPEventDispatcher class]])
                    [(SPEventDispatcher *)currentObject removeEventListenersAtObject:self
                                                        forType:SPEventTypeRemoveFromJuggler];
            }

            SP_RELEASE_AND_RETAIN(_objects, remainingObjects);
            [remainingObjects release];
            */
        }

        /// <summary>
        /// Determines if an object has been added to the juggler.
        /// </summary>
        public bool Contains(IAnimatable animatable)
        {
            return _objects.Contains(animatable);
        }

        /// <summary>
        /// Delays the execution of a certain method. Returns a proxy object on which to call the method
        /// instead. Execution will be delayed until 'time' has passed.
        /// </summary>
        public object DelayInvocationOf(IAnimatable animatable, float time)
        {
            //DelayedInvocation delayedInv = DelayedInvocation.InvocationWithTarget(target, time);
            //AddObject(delayedInv);
            //return delayedInv;
            return null;
        }

        public void AdvanceTime(float seconds)
        {
            if (seconds > 0.0)
            {
                seconds *= _speed;

                _elapsedTime += seconds;

                // we need work with a copy, since user-code could modify the collection while enumerating
                IAnimatable[] objectsCopy = new IAnimatable[_objects.Count];
                _objects.CopyTo(objectsCopy);

                foreach (IAnimatable animatable in objectsCopy)
                {
                    animatable.AdvanceTime(seconds);
                }
            }
            else if (seconds < 0.0)
            {
                throw new Exception("time must be positive");
            }
        }

        /// <summary>
        /// The total life time of the juggler.
        /// </summary>
        public double ElapsedTime
        {
            get { return _elapsedTime; }
        }

        /// <summary>
        /// The speed factor adjusts how fast a juggler's animatables run.
        /// For example, a speed factor of 2.0 means the juggler runs twice as fast.
        /// </summary>
        public float Speed
        {
            get { return _speed; }
            set
            {
                if (value < 0.0f)
                    throw new Exception("Speed can not be less than 0");
                _speed = value;
            }
        }
    }
}