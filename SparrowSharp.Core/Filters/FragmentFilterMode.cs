namespace SparrowSharp.Filters
{
    /// <summary>
    /// Defines how a Filter will be rendered.
    /// </summary>
	public enum FragmentFilterMode
	{
        /// <summary>
        /// The filter will be below the object (e.g. a drop shadow)
        /// </summary>
		Below,
        /// <summary>
        /// The filter image will replace the object (e.g. a blur filter)
        /// </summary>
		Replace,
        /// <summary>
        /// the filter image will be above the object (e.g. a glow)
        /// </summary>
		Above
	}
}

