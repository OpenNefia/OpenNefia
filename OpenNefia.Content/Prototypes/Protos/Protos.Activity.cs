using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Activity
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId Eating             = new($"Elona.Activity{nameof(Eating)}");
            public static readonly EntityPrototypeId ReadingSpellbook   = new($"Elona.Activity{nameof(ReadingSpellbook)}");
            public static readonly EntityPrototypeId ReadingAncientBook = new($"Elona.Activity{nameof(ReadingAncientBook)}");
            public static readonly EntityPrototypeId Traveling          = new($"Elona.Activity{nameof(Traveling)}");
            public static readonly EntityPrototypeId Resting            = new($"Elona.Activity{nameof(Resting)}");
            public static readonly EntityPrototypeId Mining             = new($"Elona.Activity{nameof(Mining)}");
            public static readonly EntityPrototypeId Performing         = new($"Elona.Activity{nameof(Performing)}");
            public static readonly EntityPrototypeId Fishing            = new($"Elona.Activity{nameof(Fishing)}");
            public static readonly EntityPrototypeId DiggingSpot        = new($"Elona.Activity{nameof(DiggingSpot)}");
            public static readonly EntityPrototypeId Sex                = new($"Elona.Activity{nameof(Sex)}");
            public static readonly EntityPrototypeId PreparingToSleep   = new($"Elona.Activity{nameof(PreparingToSleep)}");
            public static readonly EntityPrototypeId Harvesting         = new($"Elona.Activity{nameof(Harvesting)}");
            public static readonly EntityPrototypeId Training           = new($"Elona.Activity{nameof(Training)}");
            public static readonly EntityPrototypeId Pickpocket         = new($"Elona.Activity{nameof(Pickpocket)}");
            public static readonly EntityPrototypeId Searching          = new($"Elona.Activity{nameof(Searching)}");

            #pragma warning restore format
        }
    }
}
