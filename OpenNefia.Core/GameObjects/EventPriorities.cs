namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Set of standard event priorities.
    /// "High priority" means "executed sooner".
    /// </summary>
    public static class EventPriorities
    {
        /// <summary>
        /// Event is executed as late as possible.
        /// <see cref="VeryLow"/> is recommended instead.
        /// </summary>
        public const long Lowest   =  1000000000L;

        /// <summary>
        /// Event is executed very late.
        /// </summary>
        public const long VeryLow  =  10000000L;

        /// <summary>
        /// Event is executed later than most.
        /// </summary>
        public const long Low      =  100000L;

        /// <summary>
        /// Priority shouldn't matter for this event.
        /// </summary>
        public const long Default  =  0L;

        /// <summary>
        /// Event is executed earlier than most.
        /// </summary>
        public const long High     = -100000L;

        /// <summary>
        /// Event is executed very early.
        /// </summary>
        public const long VeryHigh = -10000000L;

        /// <summary>
        /// Event is executed as early as possible.
        /// <see cref="VeryHigh"/> is recommended instead.
        /// </summary>
        public const long Highest  = -1000000000L;
    }
}