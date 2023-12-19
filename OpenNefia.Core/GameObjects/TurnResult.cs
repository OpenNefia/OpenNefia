using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public enum TurnResult
    {
        // TODO remove this and replace with 'TurnResult?'.
        // There are some cases where a turn result is required,
        // so it would not make sense to be able to pass NoResult
        // in those cases.
        [Obsolete("Replace with TurnResult.Aborted")]
        NoResult = 0,

        /// <summary>
        /// The action failed, and turns should not pass (control is returned to the player).
        /// Commonly used when a menu is canceled out of.
        /// Equivalent to <see cref="Failed"/> for the AI.
        /// </summary>
        Aborted = 1,

        /// <summary>
        /// The action failed, and turns will pass.
        /// </summary>
        Failed = 2,

        /// <summary>
        /// The action succeeded, and turns will pass.
        /// </summary>
        Succeeded = 3,
    }

    public static class TurnResultExt
    {
        public static TurnResult Combine(this TurnResult us, TurnResult them)
        {
            // Turn results are ordered in priority:
            // - Succeeded overrides failed
            // - Failed overrides aborted
            // - Aborted overrides having no result
            return EnumHelpers.Max(us, them);
        }
    }
}