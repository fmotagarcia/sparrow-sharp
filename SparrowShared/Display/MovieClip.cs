using System;
using System.Collections.Generic;
using Sparrow.Textures;
using Sparrow.Animation;

namespace Sparrow.Display
{

    /** A MovieClip is a simple way to display an animation depicted by a list of textures.
     *  
     *  <p>Pass the frames of the movie in a vector of textures to the constructor. The movie clip 
     *  will have the width and height of the first frame. If you group your frames with the help 
     *  of a texture atlas (which is recommended), use the <code>getTextures</code>-method of the 
     *  atlas to receive the textures in the correct (alphabetic) order.</p> 
     *  
     *  <p>You can specify the desired framerate via the constructor. You can, however, manually 
     *  give each frame a custom duration. You can also play a sound whenever a certain frame 
     *  appears, or execute a callback (a "frame action").</p>
     *  
     *  <p>The methods <code>play</code> and <code>pause</code> control playback of the movie. You
     *  will receive an event of type <code>Event.COMPLETE</code> when the movie finished
     *  playback. If the movie is looping, the event is dispatched once per loop.</p>
     *  
     *  <p>As any animated object, a movie clip has to be added to a juggler (or have its 
     *  <code>advanceTime</code> method called regularly) to run. The movie will dispatch 
     *  an event of type "Event.COMPLETE" whenever it has displayed its last frame.</p>
     *  
     *  @see Sparrow.Textures.TextureAtlas
     */
    public class MovieClip : Image, IAnimatable
    {
        
        public event Juggler.RemoveFromJugglerHandler RemoveFromJugglerEvent;

        public delegate void OnCompleteEvent();
        public OnCompleteEvent OnComplete;

        private List<MovieClipFrame> _frames;
        private float _defaultFrameDuration;
        private float _currentTime;
        private int _currentFrameId;
        private bool _playing;
        //private bool _muted;
        private bool _wasStopped;
        //private SoundTransform _soundTransform;

        /// <summary>
        /// Removes this object from its juggler(s) (if it has one)
        /// </summary>
        public void RemoveFromJuggler()
        {
            RemoveFromJugglerEvent?.Invoke(this);
        }

        /// <summary>
        /// Initializes a movie with the first frame and the default number of frames per second.
        /// </summary>
        public MovieClip(List<Texture> textures, float fps = 12) : base(textures[0])
        {
            if (textures.Count > 0)
            {
                Init(textures, fps);
            }
            else
            {
                throw new ArgumentException("Empty texture array");
            }
        }

        private void Init(List<Texture> textures, float fps)
        {
            if (fps <= 0) throw new ArgumentException("Invalid fps: " + fps);
            int numFrames = textures.Count;
            
            _defaultFrameDuration = 1.0f / fps;
            Loop = true;
            _playing = true;
            _currentTime = 0.0f;
            _currentFrameId = 0;
            _wasStopped = true;
            _frames = new List<MovieClipFrame>();

            for (int i = 0; i < numFrames; ++i)
            {
                _frames.Add(new MovieClipFrame(
                        textures[i], _defaultFrameDuration, _defaultFrameDuration * i));
            }
        }

        // frame manipulation

        /** Adds an additional frame, optionally with a sound and a custom duration. If the 
         *  duration is omitted, the default framerate is used (as specified in the constructor). */
        public void AddFrame(Texture texture, float duration = 1)
        {
            AddFrame(NumFrames, texture, duration);   
        }

        /// <summary>
        /// Adds a frame with a certain texture and duration.
        /// </summary>
        public void AddFrame(int frameId, Texture texture, float duration = -1f)
        {
            if (frameId < 0 || frameId > NumFrames) throw new ArgumentException("Invalid frame id");
            if (duration < 0) duration = _defaultFrameDuration;

            MovieClipFrame frame = new MovieClipFrame(texture, duration);
            //frame.sound = sound;
            _frames.Insert(frameId, frame);
            

            if (frameId == NumFrames)
            {
                float prevStartTime = frameId > 0 ? _frames[frameId - 1].StartTime : 0.0f;
                float prevDuration = frameId > 0 ? _frames[frameId - 1].Duration : 0.0f;
                frame.StartTime = prevStartTime + prevDuration;
            }
            else
                UpdateStartTimes();
        }

        /// <summary>
        /// Removes the frame at a certain id. The successors will move down.
        /// </summary>
        /// <exception cref="ArgumentException">thrown when the frame id is invalid</exception>
        /// <exception cref="InvalidOperationException">thrown when the MovieClip is empty</exception>
        public void RemoveFrameAt(int frameId)
        {
            if (frameId < 0 || frameId >= NumFrames) throw new ArgumentException("Invalid frame id");
            if (NumFrames == 1) throw new InvalidOperationException("Movie clip must not be empty");
            
            _frames.RemoveAt(frameId);

            if (frameId != NumFrames)
                UpdateStartTimes();
        }

        /// <summary>
        /// Returns the texture of a certain frame.
        /// </summary>
        /// <exception cref="ArgumentException">thorwn when the frame id is invalid</exception>
        public Texture GetFrameTexture(int frameId)
        {
            if (frameId < 0 || frameId >= NumFrames) throw new ArgumentException("Invalid frame id");
            return _frames[frameId].Texture;
        }

        /// <summary>
        ///  Sets the texture of a certain frame.
        /// </summary>
        public void SetFrameTexture(int frameId, Texture texture)
        {
            if (frameId < 0 || frameId >= NumFrames) throw new ArgumentException("Invalid frame id");
            _frames[frameId].Texture = texture;
        }
        /*
        /// Returns the sound of a certain frame. 
        public function getFrameSound(frameID:int):Sound
        {
            if (frameID< 0 || frameID >= numFrames) throw new ArgumentException("Invalid frame id");
            return _frames[frameID].sound;
        }

        /// Sets the sound of a certain frame. The sound will be played whenever the frame 
        /// is displayed. 
        public function setFrameSound(frameID:int, sound:Sound):void
        {
            if (frameID< 0 || frameID >= numFrames) throw new ArgumentException("Invalid frame id");
            _frames[frameID].sound = sound;
        }

        /// Returns the method that is executed at a certain frame.
        public function getFrameAction(frameID:int):Function
        {
            if (frameID< 0 || frameID >= numFrames) throw new ArgumentException("Invalid frame id");
            return _frames[frameID].action;
        }

        /// Sets an action that will be executed whenever a certain frame is reached.
        public function setFrameAction(frameID:int, action:Function):void
        {
            if (frameID< 0 || frameID >= numFrames) throw new ArgumentException("Invalid frame id");
            _frames[frameID].action = action;
        }
        */

        /// <summary>
        /// Returns the duration (in seconds) of a frame at a certain position.
        /// </summary>
        public float GetFrameDuration(int frameId)
        {
            if (frameId < 0 || frameId >= NumFrames) throw new ArgumentException("Invalid frame id");
            return _frames[frameId].Duration;
        }

        /// <summary>
        /// Sets the duration of a certain frame in seconds.
        /// </summary>
        public void SetFrameDuration(int frameId, float duration)
        {
            if (frameId < 0 || frameId >= NumFrames) throw new ArgumentException("Invalid frame id");
            _frames[frameId].Duration = duration;
            UpdateStartTimes();
        }

        /** Reverses the order of all frames, making the clip run from end to start.
          *  Makes sure that the currently visible frame stays the same. */
        public void RreverseFrames()
        {
            _frames.Reverse();
            _currentTime = TotalTime - _currentTime;
            _currentFrameId = NumFrames - _currentFrameId - 1;
            UpdateStartTimes();
        }

        /// <summary>
        ///  Start playback. Beware that the clip has to be added to a Juggler too!
        /// </summary>
        public void Play()
        {
            _playing = true;
        }

        /// <summary>
        /// Pause playback.
        /// </summary>
        public void Pause()
        {
            _playing = false;
        }

        /// <summary>
        /// Stop playback. Resets currentFrame to beginning.
        /// </summary>
        public void Stop()
        {
            _playing = false;
            _wasStopped = true;
            CurrentFrame = 0;
        }

        // helpers

        private void UpdateStartTimes()
        {
            int numFrames = NumFrames;
            MovieClipFrame prevFrame = _frames[0];
            prevFrame.StartTime = 0;
            
            for (int i =1 ; i < numFrames; ++i)
            {
                _frames[i].StartTime = prevFrame.StartTime + prevFrame.Duration;
                prevFrame = _frames[i];
            }
        }

        // IAnimatable
        public void AdvanceTime(float passedTime)
        {
            if (!_playing) return;

            // The tricky part in this method is that whenever a callback is executed
            // (a frame action or a 'COMPLETE' event handler), that callback might modify the clip.
            // Thus, we have to start over with the remaining time whenever that happens.

            MovieClipFrame frame = _frames[_currentFrameId];

            if (_wasStopped)
            {
                // if the clip was stopped and started again,
                // sound and action of this frame need to be repeated.

                _wasStopped = false;
                //frame.playSound(_soundTransform);
                /*
                if (frame.action != null)
                {
                    frame.executeAction(this, _currentFrameID);
                    advanceTime(passedTime);
                    return;
                }*/
            }

            if (_currentTime == TotalTime)
            {
                if (Loop)
                {
                    _currentTime = 0.0f;
                    _currentFrameId = 0;
                    frame = _frames[0];
                    //frame.playSound(_soundTransform);
                    Texture = frame.Texture;
                    /*
                    if (frame.action != null)
                    {
                        frame.executeAction(this, _currentFrameID);
                        advanceTime(passedTime);
                        return;
                    }*/
                }
                else return;
            }

            int finalFrameId = _frames.Count - 1;
            float restTimeInFrame = frame.Duration - _currentTime + frame.StartTime;
            bool dispatchCompleteEvent = false;
            //Function frameAction = null;
            int previousFrameId = _currentFrameId;
            //bool changedFrame;

            while (passedTime >= restTimeInFrame)
            {
                //changedFrame = false;
                passedTime -= restTimeInFrame;
                _currentTime = frame.StartTime + frame.Duration;

                if (_currentFrameId == finalFrameId)
                {
                    if (OnComplete != null)
                    {
                        dispatchCompleteEvent = true;
                    }
                    else if (Loop)
                    {
                        _currentTime = 0;
                        _currentFrameId = 0;
                        //changedFrame = true;
                    }
                    else return;
                }
                else
                {
                    _currentFrameId += 1;
                    //changedFrame = true;
                }

                frame = _frames[_currentFrameId];
                //frameAction = frame.action;

                //if (changedFrame)
                //    frame.playSound(_soundTransform);

                if (dispatchCompleteEvent)
                {
                    Texture = frame.Texture;
                    OnComplete();
                    AdvanceTime(passedTime);
                    return;
                }
                /*else if (frameAction != null)
                {
                    texture = frame.texture;
                    frame.executeAction(this, _currentFrameID);
                    advanceTime(passedTime);
                    return;
                }*/

                restTimeInFrame = frame.Duration;

                // prevent a mean floating point problem (issue #851)
                if (passedTime + 0.0001f > restTimeInFrame && passedTime - 0.0001f < restTimeInFrame)
                    passedTime = restTimeInFrame;
            }

            if (previousFrameId != _currentFrameId)
                Texture = _frames[_currentFrameId].Texture;

            _currentTime += passedTime;
        }

        /// <summary>
        /// The number of frames of the clip.
        /// </summary>
        public int NumFrames
        {
            get { return _frames.Count; }
        }

        /// <summary>
        /// The total duration of the clip in seconds.
        /// </summary>
        public float TotalTime
        {
            get
            {
                MovieClipFrame lastFrame = _frames[_frames.Count - 1];
                return lastFrame.StartTime + lastFrame.Duration;
            }
        }

        /// <summary>
        /// The time that has passed since the clip was started (each loop starts at zero).
        /// </summary>
        public float CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (value < 0 || value > TotalTime) throw new ArgumentException("Invalid time: " + value);

                int lastFrameId = _frames.Count - 1;
                _currentTime = value;
                _currentFrameId = 0;

                while (_currentFrameId < lastFrameId && _frames[_currentFrameId + 1].StartTime <= value)
                    ++_currentFrameId;

                MovieClipFrame frame = _frames[_currentFrameId];
                Texture = frame.Texture;
            }
        }

        /// <summary>
        /// Indicates if the movie is looping.
        /// </summary>
        public bool Loop;

        //public bool Muted;

        /// <summary>
        /// The position of the frame that is currently displayed.
        /// </summary>
        public int CurrentFrame
        {
            get { return _currentFrameId; }
            set
            {
                if (value < 0 || value >= NumFrames) throw new ArgumentException("Invalid frame id");
                CurrentTime = _frames[value].StartTime;
            }
        }

        /// <summary>
        /// The default frames per second. Used when you add a frame without specifying a duration.
        /// </summary>
        public float Fps
        {
            get { return (1.0f / _defaultFrameDuration); }
            set
            {
                if (value <= 0) throw new ArgumentException("Invalid fps: " + value);

                float newFrameDuration = 1.0f / value;
                float acceleration = newFrameDuration / _defaultFrameDuration;
                _currentTime *= acceleration;
                _defaultFrameDuration = newFrameDuration;

                for (int i = 0; i < NumFrames; ++i)
                _frames[i].Duration *= acceleration;

                UpdateStartTimes();
            }
        }

        /// <summary>
        /// Indicates if the movie is currently playing. Returns 'false' when the end has been reached.
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                if (_playing)
                {
                    return Loop || _currentTime < TotalTime;
                }
                return false; 
            }
        }

        /// <summary>
        /// Indicates if a (non-looping) movie has come to its end.
        /// </summary>
        public bool IsComplete
        {
            get { return !Loop && _currentTime >= TotalTime; }
        }
        
    }

    internal class MovieClipFrame
    {
        public MovieClipFrame(Texture texture, float duration = 0.1f, float startTime = 0)
        {
            Texture = texture;
            Duration = duration;
            StartTime = startTime;
        }

        public Texture Texture;
        //public var Sound:Sound;
        public float Duration;
        public float StartTime;
        //public Function action;

        //public void PlaySound(SoundTransform transform)
        //{
        //    if (sound != null) sound.play(0, 0, transform);
        //}

        public void ExecuteAction(MovieClip movie, int frameId)
        {
            throw new NotImplementedException();
            /*if (action != null)
            {
                int numArgs = action.length;

                if (numArgs == 0) action();
                else if (numArgs == 1) action(movie);
                else if (numArgs == 2) action(movie, frameID);
                else throw new Exception("Frame actions support zero, one or two parameters: " +
                        "movie:MovieClip, frameID:int");
            }*/
        }

    }
}