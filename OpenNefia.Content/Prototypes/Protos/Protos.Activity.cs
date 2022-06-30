using ActivityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Activity.ActivityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Activity
        {
            #pragma warning disable format

            public static readonly ActivityPrototypeId Eating             = new($"Elona.{nameof(Eating)}");
            public static readonly ActivityPrototypeId ReadingSpellbook   = new($"Elona.{nameof(ReadingSpellbook)}");
            public static readonly ActivityPrototypeId ReadingAncientBook = new($"Elona.{nameof(ReadingAncientBook)}");
            public static readonly ActivityPrototypeId Traveling          = new($"Elona.{nameof(Traveling)}");
            public static readonly ActivityPrototypeId Resting            = new($"Elona.{nameof(Resting)}");
            public static readonly ActivityPrototypeId Mining             = new($"Elona.{nameof(Mining)}");
            public static readonly ActivityPrototypeId Performing         = new($"Elona.{nameof(Performing)}");
            public static readonly ActivityPrototypeId Fishing            = new($"Elona.{nameof(Fishing)}");
            public static readonly ActivityPrototypeId DiggingSpot        = new($"Elona.{nameof(DiggingSpot)}");
            public static readonly ActivityPrototypeId Sex                = new($"Elona.{nameof(Sex)}");
            public static readonly ActivityPrototypeId PreparingToSleep   = new($"Elona.{nameof(PreparingToSleep)}");
            public static readonly ActivityPrototypeId Harvesting         = new($"Elona.{nameof(Harvesting)}");
            public static readonly ActivityPrototypeId Training           = new($"Elona.{nameof(Training)}");
            public static readonly ActivityPrototypeId Pickpocket         = new($"Elona.{nameof(Pickpocket)}");
            public static readonly ActivityPrototypeId Searching          = new($"Elona.{nameof(Searching)}");

            #pragma warning restore format
        }
    }
}
