using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncSustainAttributeComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncSustainAttribute";

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        public string? Description => null;

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncSustainAttributeComponent otherSustainAttribute
                && otherSustainAttribute.SkillID == SkillID;
        }
    }
}