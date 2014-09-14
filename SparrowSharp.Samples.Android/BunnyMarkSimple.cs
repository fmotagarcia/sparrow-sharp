using System;
using Sparrow.Textures;
using Sparrow.Geom;
using System.Collections.Generic;
using Sparrow.Display;
using Sparrow.ResourceLoading;
using Sparrow.Touches;
using SparrowSharp.Fonts;

namespace Sparrow.Samples.Android
{
    public class BunnyMarkSimple : Sprite
    {
 
        private int mCount = 0;
        private GLTexture mTexture;
        private List<Bunny> mBunnies = new List<Bunny>();
        private Bunny mBunny;
        private int mIndex;
        private Rectangle mRectangle;
        private float mMinX = 0;
        private float mMaxX = 0;
        private float mMinY = 0;
        private float mMaxY = 0;
        private float mGravity = 0.5f;
        private static readonly Random random = new Random();

        /**
         * Creates a BunnyMark with a certain number of Wabbits.
         * 
         * @param count
         * The number of wabbits.
         * @param rect
         * You can define a rectangle for the borders of the BunnyMark. If you don't specify the rectangle the complete stage will be used.
         */
        public BunnyMarkSimple(int count = 100, Rectangle rect=null) 
        {
            Touchable = false;
            mCount = count;
            if (rect != null) mRectangle = rect;
            mTexture = SimpleTextureLoader.LoadAndroidResource(SparrowSharp.Samples.Android.Resource.Drawable.wabbit_alpha);
            AddedToStage += AddedToStageHandler;
        }

        private void AddBunny()
        {
            mBunny = new Bunny(mTexture);
            mBunny.SpeedX = (float)random.NextDouble() * 5;
            mBunny.SpeedY = (float)random.NextDouble() * 5 - 2.5f;
            mBunny.ScaleX = mBunny.ScaleY = (float)random.NextDouble() + 0.3f; 
            mBunny.Rotation = (float)random.NextDouble() * 30 - 15;
            AddChild(mBunny);
            mBunnies.Add(mBunny);
        }

        private TextField txt = new TextField(200, 20, "", "mini", 14, 0xffadff);

        private void AddedToStageHandler(DisplayObject target, DisplayObject currentTarget)
        {
            AddChild(txt);
            txt.Y = 25;

            SparrowSharpApp.ShowStats = true;
            if (mRectangle != null) {
                mMinX = mRectangle.X;
                mMinY = mRectangle.Y;
                mMaxX = mRectangle.X + mRectangle.Width - 26;
                mMaxY = mRectangle.Y + mRectangle.Height - 37;
            }else{
                mMaxX = Stage.Width - 26;
                mMaxY = Stage.Height - 37;
            }
            Count = mCount;
            EnterFrame += EnterFrameHandler;
            Stage.Touch += MyTouchHandler;
        }

        private void MyTouchHandler(TouchEvent touch)
        {
            if (touch.Touches.Count == 1)
            {
                if (touch.Touches[0].IsTap)
                {
                    Count += 100;
                    txt.Text = "Bunnies: " + Count;
                }
            }
        }

        private void EnterFrameHandler(DisplayObject target, float passedTime) 
        {
            mCount = mBunnies.Count;
            var arr = mBunnies.ToArray();
            //for (int i=0;i<mBunnies.Count;i++) { mBunny = mBunnies[i];
            foreach (Bunny mBunny in mBunnies) {
                mBunny.X += mBunny.SpeedX;
                mBunny.Y += mBunny.SpeedY;
                mBunny.SpeedY += mGravity;

                //b.alpha = 0.3 + 0.7 * b.y / maxY; 

                if (mBunny.X > mMaxX)
                {
                    mBunny.SpeedX *= -1;
                    mBunny.X = mMaxX;
                }
                else if (mBunny.X < mMinX)
                {
                    mBunny.SpeedX *= -1;
                    mBunny.X = mMinX;
                }
                if (mBunny.Y > mMaxY)
                {
                    mBunny.SpeedY *= -0.8f;
                    mBunny.Y = mMaxY;
                    if (random.NextDouble() > 0.5) mBunny.SpeedY -= 3 + (float)random.NextDouble() * 4;
                } 
                else if (mBunny.Y < mMinY)
                {
                    mBunny.SpeedY = 0;
                    mBunny.Y = mMinY;
                }           
            }
        }

        private void RemoveBunny() 
        {
            if (mBunnies.Count > 0){
                mBunny = mBunnies[mBunnies.Count - 1];
                mBunnies.RemoveAt(mBunnies.Count - 1);
                if (mBunny != null) {
                    mBunny.Parent.RemoveChild(mBunny);
                }
            }
        }

        public void Stop()
        {
            EnterFrame -= EnterFrameHandler;
        }

        public int Count {
            get { return mBunnies.Count;}
            set {
                if (Stage == null) {
                    mCount = value;
                    return;
                }
                while (Count < value)
                    AddBunny();
                while (Count > value)
                    RemoveBunny();
            }
        }
    }

    class Bunny : Image
    {
        public float SpeedX;
        public float SpeedY;

        public Bunny(Texture texture) : base(texture)
        {
        }
    }
}

