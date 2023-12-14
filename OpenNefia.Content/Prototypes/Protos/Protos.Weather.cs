using WeatherPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Weather
        {
            #pragma warning disable format

            public static readonly WeatherPrototypeId Sunny     = new($"Elona.Weather{nameof(Sunny)}");
            public static readonly WeatherPrototypeId Etherwind = new($"Elona.Weather{nameof(Etherwind)}");
            public static readonly WeatherPrototypeId Snow      = new($"Elona.Weather{nameof(Snow)}");
            public static readonly WeatherPrototypeId Rain      = new($"Elona.Weather{nameof(Rain)}");
            public static readonly WeatherPrototypeId HardRain  = new($"Elona.Weather{nameof(HardRain)}");

            #pragma warning restore format
        }
    }
}
