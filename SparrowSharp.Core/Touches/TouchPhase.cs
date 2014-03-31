using System;

namespace Sparrow.Touches
{
	public enum TouchPhase
	{
		Began,      // The finger just touched the screen.
		Moved,      // The finger moves around.    
		Stationary, // The finger has not moved since the last frame.    
		Ended,      // The finger was lifted from the screen.    
		Cancelled   // The touch was aborted by the system (e.g. because of an Alert popping up)
	}

}

