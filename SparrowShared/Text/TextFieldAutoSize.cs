namespace Sparrow.Text
{
    /// <summary>
    /// This class is an enumeration of constant values used in setting the 
    /// autoSize property of the TextField class.
    /// </summary>
    public enum TextFieldAutoSize
    {
        /** No auto-sizing will happen. */
        NONE,
        /** The text field will grow/shrink sidewards; no line-breaks will be added.
         *  The height of the text field remains unchanged. */
        HORIZONTAL,
        /** The text field will grow/shrink downwards, adding line-breaks when necessary.
          * The width of the text field remains unchanged. */
        VERTICAL,
        /** The text field will grow to the right and bottom; no line-breaks will be added. */
        BOTH_DIRECTIONS
    }
}
