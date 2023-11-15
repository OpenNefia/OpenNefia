using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Area
        {
            public static readonly EntityPrototypeId Noyel = new($"Elona.Area{nameof(Noyel)}");

            public static readonly EntityPrototypeId HomeCave = new($"Elona.Area{nameof(HomeCave)}");
        }
    }
}
