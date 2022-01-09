using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Mef
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId Web    = new($"Elona.Mef{nameof(Web)}");
            public static readonly EntityPrototypeId Mist   = new($"Elona.Mef{nameof(Mist)}");
            public static readonly EntityPrototypeId Acid   = new($"Elona.Mef{nameof(Acid)}");
            public static readonly EntityPrototypeId Ether  = new($"Elona.Mef{nameof(Ether)}");
            public static readonly EntityPrototypeId Fire   = new($"Elona.Mef{nameof(Fire)}");
            public static readonly EntityPrototypeId Potion = new($"Elona.Mef{nameof(Potion)}");
            public static readonly EntityPrototypeId Nuke   = new($"Elona.Mef{nameof(Nuke)}");

            #pragma warning restore format
        }
    }
}
