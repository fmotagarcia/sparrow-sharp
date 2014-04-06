namespace SparrowSharp.Display
{
    /// <summary>
    /// The Animatable protocol describes objects that are animated depending on the passed time. 
    /// Any object that implements this protocol can be added to the Juggler.
 
    /// When an object should no longer be animated, it has to be removed from the juggler.
    /// To do this, you can manually remove it via the method 'Remove',
    /// or the object can request to be removed by dispatching an event with the type
    /// 'RemoveFromJuggler'. The 'Tween' class is an example of a class that
    /// dispatches such an event; you don't have to remove tweens manually from the juggler.
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        ///  Advance the animation by a number of seconds.
        /// </summary>
        void AdvanceTime(float seconds);
    }
}