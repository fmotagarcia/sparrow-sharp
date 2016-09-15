namespace SparrowSharp.Display
{
    /// <summary>
    /// The Animatable interface describes objects that are animated depending on the passed time. 
    /// Any object that implements this interface can be added to the Juggler.
 
    /// When an object should no longer be animated, it has to be removed from the juggler.
    /// To do this, you can manually remove it via the 'Remove' method of its juggler,
    /// or the object can request to be removed by dispatching a RemoveFromJugglerEvent event.
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        ///  Advance the animation by a number of seconds.
        /// </summary>
        void AdvanceTime(float seconds);

        /// <summary>
        /// Dispatch if you want this removed from its Juggler
        /// </summary>
        event Sparrow.Utils.Juggler.RemoveFromJugglerHandler RemoveFromJugglerEvent;

    }
}