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
        [DataField]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncSustainAttributeComponent otherSustainAttribute
                && otherSustainAttribute.SkillID == SkillID;
        }
    }
}