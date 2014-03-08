using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Sparrow.Core;

namespace Sparrow.Fonts
{
    public class BitmapChar
    {
        private int _charId;
        private Texture _texture;
        private float _xOffset;
        private float _yOffset;
        private float _xAdvance;
        private Dictionary<int, float> _kernings;

        public BitmapChar(int charId, Texture texture, float xOffset, float yOffset, float xAdvance)
        {
            _charId = charId;
            _texture = texture;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _xAdvance = xAdvance;
        }

        public void AddKerning(float amount, int charId)
        {
            // TODO: sparrow lazy loads the kernings dictionary, revisit to check that we really want this
            if (_kernings == null)
            {
                _kernings = new Dictionary<int, float>();
            }

            _kernings.Add(charId, amount);
        }
    }
}

