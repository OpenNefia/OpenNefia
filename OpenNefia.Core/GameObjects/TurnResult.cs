namespace OpenNefia.Core.GameObjects
{
    public enum TurnResult
    {
        // TODO remove this and replace with TurnResult?
        // There are some cases where a turn result is required,
        // so it would not make sense to be able to pass NoResult
        // in those cases.
        [Obsolete("Replace with TurnResult.Aborted")]
        NoResult = 0,
        
        /// <summary>
        /// The action failed, and turns will pass.
        /// </summary>
        Failed = 1,
        
        /// <summary>
        /// The action failed, and turns should not pass (control is returned to the player).
        /// Commonly used when a menu is canceled out of.
        /// </summary>
        Aborted = 2,
        
        /// <summary>
        /// The action Succeeded, and turns will pass.
        /// </summary>
        Succeeded = 3
    }
}