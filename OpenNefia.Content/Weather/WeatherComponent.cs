using OpenNefia.Content.Audio;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Weather
{
    /// <summary>
    /// Component defining a weather entity.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Weather)]
    public class WeatherComponent : Component
    {
        /// <summary>
        /// Applies buffs to enemy encounters when this weather is active.
        /// </summary>
        [DataField]
        public bool IsBadWeather { get; set; } = false;

        /// <summary>
        /// If false, don't show the name of this weather near the clock in the HUD.
        /// </summary>
        [DataField]
        public bool ShowNameInHUD { get; set; } = true;

        /// <summary>
        /// Sound to play when this weather is active.
        /// </summary>
        [DataField]
        public SoundSpecifier? AmbientSound { get; set; }

        [DataField]
        public GameTimeSpan TimeUntilNextChange { get; set; }
    }
}