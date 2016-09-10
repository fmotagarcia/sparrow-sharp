using System;
using Sparrow.Display;
using SparrowSharp.Fonts;
using SparrowSharp.Core;

namespace SparrowSharp.Utils
{
    /// <summary>
    /// The statistics display is used internally by Sparrow to display statistical information.
    /// Use the 'ShowStats()' method of 'ViewController' to show it.
    ///
    /// _This is an internal class. You do not have to use it manually._
    /// </summary>
    public class StatsDisplay : Sprite
    {
        /*
        /// <summary>
        /// The actual frame rate, i.e. the number of frames rendered per second.
        /// </summary>
        public double FramesPerSecond;

        /// <summary>
        /// The number of draw calls per frame.
        /// </summary>
        public int NumDrawCalls;

        TextField textField;
        double totalTime;
        int frameCount;

        public StatsDisplay()
        {
            Quad background = new Quad(220, 18, 0x0);

            AddChild(background);

            FramesPerSecond = 0;
            NumDrawCalls = 0;

            BlendMode = Sparrow.Display.BlendMode.NONE;

            AddedToStage += OnAddedToStage;

            EnterFrame += OnEnterFrame;
        }

        void OnAddedToStage(DisplayObject target, DisplayObject currentTarget)
        {
            FramesPerSecond = NumDrawCalls = 0;
            Update();
        }

        void OnEnterFrame(DisplayObject target, float passedTime)
        {
            totalTime += passedTime;
            frameCount++;

            if (totalTime > 1000)
            {
                FramesPerSecond = Math.Round(frameCount / (totalTime/1000), 1);
                frameCount = 0;
                totalTime = 0;
                Update();
            }
        }

        void Update()
        {
            if (textField == null)
            {
                textField = new TextField(220, 18, "", "mini", 15, 0xFFFFFF);
                textField.HAlign = HAlign.Left;
                textField.VAlign = VAlign.Top;
                textField.X = 2;
                AddChild(textField);
            }
            textField.Text = "DRW: " + NumDrawCalls + " FPS: " + FramesPerSecond;
        }
        */
    }
}

