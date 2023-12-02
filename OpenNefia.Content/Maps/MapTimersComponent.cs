using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Arbitrary timers that decrement with time passed in the active map.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MapTimersComponent : Component
    {
        [DataField]
        public Dictionary<string, MapTimer> Timers { get; set; } = new();
    }

    [DataDefinition]
    public class MapTimer
    {
        /// <summary>
        /// Time left.
        /// </summary>
        [DataField]
        public GameTimeSpan TimeRemaining { get; set; } = GameTimeSpan.Zero;
    }
}