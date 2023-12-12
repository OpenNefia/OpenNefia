using WeatherPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Weather.WeatherPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Weather
        {
            #pragma warning disable format

            public static readonly WeatherPrototypeId Sunny     = new($"Elona.{nameof(Sunny)}");
            public static readonly WeatherPrototypeId Etherwind = new($"Elona.{nameof(Etherwind)}");
            public static readonly WeatherPrototypeId Snow      = new($"Elona.{nameof(Snow)}");
            public static readonly WeatherPrototypeId Rain      = new($"Elona.{nameof(Rain)}");
            public static readonly WeatherPrototypeId HardRain  = new($"Elona.{nameof(HardRain)}");

            #pragma warning restore format
        }
    }
}
