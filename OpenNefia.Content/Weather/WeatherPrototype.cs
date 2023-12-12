using OpenNefia.Content.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Weather
{
    [Prototype("Elona.Weather")]
    public class WeatherPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public bool IsBadWeather { get; } = false;

        [DataField]
        public SoundSpecifier? AmbientSound { get; }
    }
}