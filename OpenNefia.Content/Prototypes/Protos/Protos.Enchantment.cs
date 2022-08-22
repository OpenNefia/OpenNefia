using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Enchantment
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId ModifyAttribute = new($"Elona.Enc{nameof(ModifyAttribute)}");

            #pragma warning restore format
        }
    }
}
