using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Enchantment
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId ModifyAttribute = new($"Elona.Enc{nameof(ModifyAttribute)}");
            public static readonly EntityPrototypeId ModifyResistance = new($"Elona.Enc{nameof(ModifyResistance)}");
            public static readonly EntityPrototypeId ModifySkill = new($"Elona.Enc{nameof(ModifySkill)}");
            public static readonly EntityPrototypeId SustainAttribute = new($"Elona.Enc{nameof(SustainAttribute)}");
            public static readonly EntityPrototypeId ElementalDamage = new($"Elona.Enc{nameof(ElementalDamage)}");

            #pragma warning restore format
        }
    }
}
