namespace OpenNefia.Core.Timing
{
    /// <summary>
    ///     Arguments of the GameLoop frame event.
    /// </summary>
    public readonly struct FrameEventArgs
    {
        /// <summary>
        ///     Seconds passed since this event was last called.
        /// </summary>
        public float DeltaSeconds { get; }

        /// <summary>
        /// If false, do not update keybinds.
        /// </summary>
        public bool StepInput { get; }

        /// <summary>
        ///     Constructs an instance of this object.
        /// </summary>
        /// <param name="deltaSeconds">Seconds passed since this event was last called.</param>
        public FrameEventArgs(float deltaSeconds, bool stepInput = true)
        {
            DeltaSeconds = deltaSeconds;
            StepInput = stepInput;
        }
    }
}
