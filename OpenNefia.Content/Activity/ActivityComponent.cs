using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Activity
{
    /// <summary>
    /// Defines an activity entity that is held by a character.
    /// </summary>
    [RegisterComponent]
    public sealed class ActivityComponent : Component
    {
        [DataField]
        public int DefaultTurns { get; set; } = 10;

        [DataField]
        public int AnimationWait { get; set; } = 20;

        [DataField]
        public ActivityInterruptAction? OnInterrupt { get; set; } = null;

        /// <summary>
        /// Must be derived from <see cref="BaseAutoTurnAnim"/>.
        /// </summary>
        [DataField("autoTurnAnimType")]
        public Type? AutoTurnAnimationType { get; set; }

        [DataField]
        public bool CanScroll { get; set; } = false;

        [DataField]
        public bool InterruptOnDisplace { get; set; } = false;

        #region Instance properties

        [DataField]
        public EntityUid Actor { get; internal set; } = EntityUid.Invalid;

        /// <summary>
        /// Turns remaining for the activity.
        /// </summary>
        [DataField]
        public int TurnsRemaining { get; set; } = 0;

        [DataField]
        public bool WasInterrupted { get; set; } = false;

        #endregion
    }

    public enum ActivityInterruptAction
    {
        Ignore,
        Prompt,
        Stop
    }
}
