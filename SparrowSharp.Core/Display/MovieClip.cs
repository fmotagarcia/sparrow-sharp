using System;
using System.Collections.Generic;
using System.Linq;
using Sparrow.Display;
using Sparrow.Textures;

namespace SparrowSharp.Display
{
    /// <summary>
    /// A MovieClip is a simple way to display an animation depicted by a list of textures.

    /// You can add the frames one by one or pass them all at once (in an array) at initialization time.
    /// The movie clip will have the width and height of the first frame.
 
    /// At initialization, you can specify the desired framerate. You can, however, manually give each
    /// frame a custom duration. You can also play a sound whenever a certain frame appears.
 
    /// The methods 'Play' and 'Pause' control playback of the movie. You will receive an event of type
    /// 'Completed' when the movie finished playback. When the movie is looping, the event is 
    /// dispatched once per loop.
 
    /// As any animated object, a movie clip has to be added to a Juggler (or have its 'AdvanceTime' 
    /// method called regularly) to run.
    /// </summary>
    public class MovieClip : Image, IAnimatable
    {
        private readonly List<Texture> _textures;
        //List _sounds;
        private readonly List<float> _durations;
        private float _defaultFrameDuration;
        private float _totalTime;
        private float _currentTime;
        private bool _playing;
        private int _currentFrame;

        /// <summary>
        /// Initializes a movie with the first frame and the default number of frames per second.
        /// </summary>
        public MovieClip(Texture texture, float fps) : base(texture)
        {
            _defaultFrameDuration = 1.0f / fps;
            Loop = true;
            _playing = true;
            _totalTime = 0.0f;
            _currentTime = 0.0f;
            _currentFrame = 0;
            _textures = new List<Texture>();
            //_sounds = new List<>;
            _durations = new List<float>();        
            AddFrame(texture);
        }

        /// <summary>
        /// Initializes a MovieClip with an array of textures and the default number of frames per second.
        /// </summary>
        public MovieClip(IList<Texture> textures, float fps) : this(textures[0], fps)
        {
            if (textures.Count > 1)
            {
                for (int i = 1; i < textures.Count; ++i)
                    AddFrame(textures[i], i);
            }
        }

        /// <summary>
        /// Adds a frame to the end of the animation with a certain texture, using the default duration (1/fps).
        /// </summary>
        public void AddFrame(Texture texture)
        {
            AddFrame(texture, NumFrames);   
        }

        /// <summary>
        /// Adds a frame with a certain texture and duration.
        /// </summary>
        public void AddFrame(Texture texture, float duration)
        {
            AddFrame(texture, NumFrames, duration);
        }

        /// <summary>
        /// Inserts a frame at the specified position. The successors will move down.
        /// </summary>
        public void AddFrame(Texture texture, int position)
        {
            AddFrame(texture, position, _defaultFrameDuration);
        }

        /// <summary>
        /// Adds a frame with a certain texture, duration and sound.
        /// </summary>
        public void AddFrame(Texture texture, int position, float duration, object sound = null)
        {
            _totalTime += duration;
            _textures.Insert(position, texture);
            _durations.Insert(position, duration);
            //[_sounds insertObject:(sound ? sound : [NSNull null]) atIndex:position];
        }

        /// <summary>
        /// Removes the frame at the specified position. The successors will move up.
        /// </summary>
        public void RemoveFrameAt(int position)
        {
            _totalTime -= GetDurationAt(position);
            _textures.RemoveAt(position);
            _durations.RemoveAt(position);
            //_sounds removeObjectAtIndex:position];   
        }

        /// <summary>
        ///  Sets the texture of a certain frame.
        /// </summary>
        public void SetTexture(Texture texture, int position)
        {
            _textures[position] = texture;
        }

        /// <summary>
        /// Sets the sound that will be played back when a certain frame is active.
        /// </summary>
        public void SetSoundAt(object sound, int position)
        {
            //_sounds[frameID] = sound ? sound : null;
        }

        /// <summary>
        /// Sets the duration of a certain frame in seconds.
        /// </summary>
        public void SetDuration(float duration, int position)
        {
            _totalTime -= GetDurationAt(position);
            _durations[position] = duration;
            _totalTime += duration;
        }

        /// <summary>
        /// Returns the texture of a frame at a certain position.
        /// </summary>
        public Texture GetTextureAt(int position)
        {
            return _textures[position];
        }

        /// Returns the sound of a frame at a certain position.
        //public SoundChannel GetSoundAt(int position) {}

        /// <summary>
        /// Returns the duration (in seconds) of a frame at a certain position.
        /// </summary>
        public float GetDurationAt(int position)
        {
            return _durations[position];
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
            CurrentFrame = 0;
        }

        /// <summary>
        /// The total duration of the clip in seconds.
        /// </summary>
        public float TotalTime
        {
            get { return _totalTime; }
        }

        /// <summary>
        /// The time that has passed since the clip was started (each loop starts at zero).
        /// </summary>
        public float CurrentTime
        {
            get { return _currentTime; }
        }

        /// <summary>
        /// Indicates if the movie is looping.
        /// </summary>
        public bool Loop;

        /// <summary>
        /// The number of frames of the clip.
        /// </summary>
        public int NumFrames
        {
            get { return _textures.Count; }
        }

        /// <summary>
        /// The default frames per second. Used when you add a frame without specifying a duration.
        /// </summary>
        public float Fps
        {
            get { return (1.0f / _defaultFrameDuration); }
            set
            {
                float newFrameDuration = (value == 0.0f ? int.MaxValue : 1.0f / value);
                float acceleration = newFrameDuration / _defaultFrameDuration;
                _currentTime *= acceleration;
                _defaultFrameDuration = newFrameDuration;

                for (int i = 0; i < NumFrames; ++i)
                    SetDuration(_durations[i] * acceleration, i);
            }
        }

        /// <summary>
        /// Indicates if the movie is currently playing. Returns 'false' when the end has been reached.
        /// </summary>
        public bool Playing
        {
            get
            {
                if (_playing)
                {
                    return Loop || _currentTime < _totalTime;
                }
                return false; 
            }
        }

        /// <summary>
        /// Indicates if a (non-looping) movie has come to its end.
        /// </summary>
        public bool Complete
        {
            get { return !Loop && _currentTime >= _totalTime; }
        }

        /// <summary>
        /// The position of the frame that is currently displayed.
        /// </summary>
        public int CurrentFrame
        {
            get { return _currentFrame; }
            set
            { 
                _currentFrame = value;
                _currentTime = 0.0f;

                for (int i = 0; i < value; ++i)
                {
                    _currentTime += _durations[i];
                }
                UpdateCurrentFrame();
            }
        }

        public void AdvanceTime(float seconds)
        {
            if (Loop && _currentTime == _totalTime)
            {
                _currentTime = 0.0f;  
            }

            if (!_playing || seconds == 0.0f || _currentTime == _totalTime)
            {
                return;
            }
		
            int i = 0;
            float durationSum = 0.0f;
            float previousTime = _currentTime;
            float restTime = _totalTime - _currentTime;
            float carryOverTime = seconds > restTime ? seconds - restTime : 0.0f;
            _currentTime = Math.Min(_totalTime, _currentTime + seconds);

            foreach (float frameDuration in _durations)
            {
                if (durationSum + frameDuration >= _currentTime)
                {
                    if (_currentFrame != i)
                    {
                        _currentFrame = i;
                        UpdateCurrentFrame();
                        PlayCurrentSound();
                    }
                    break;
                }
                ++i;
                durationSum += frameDuration;
            }
            if (previousTime < _totalTime && _currentTime == _totalTime)
            {
                //[self dispatchEventWithType:SPEventTypeCompleted];
            }     
            AdvanceTime(carryOverTime);
        }

        private void UpdateCurrentFrame()
        {
            Texture = _textures[_currentFrame];
        }

        private void PlayCurrentSound()
        {
            //var sound = _sounds[_currentFrame];
            //if ([NSNull class] != [sound class])
            //    [sound play];
        }
    }
}