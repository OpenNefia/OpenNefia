using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Mef
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId Web            = new($"Elona.Mef{nameof(Web)}");
            public static readonly EntityPrototypeId MistOfDarkness = new($"Elona.Mef{nameof(MistOfDarkness)}");
            public static readonly EntityPrototypeId AcidGround     = new($"Elona.Mef{nameof(AcidGround)}");
            public static readonly EntityPrototypeId EtherGround    = new($"Elona.Mef{nameof(EtherGround)}");
            public static readonly EntityPrototypeId Fire           = new($"Elona.Mef{nameof(Fire)}");
            public static readonly EntityPrototypeId PotionPuddle   = new($"Elona.Mef{nameof(PotionPuddle)}");
            public static readonly EntityPrototypeId NuclearBomb    = new($"Elona.Mef{nameof(NuclearBomb)}");

            #pragma warning restore format
        }
    }
}
