using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EncModifySkillComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncModifySkill";

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        public string? Description => null;

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncModifySkillComponent otherModifySkill
                && otherModifySkill.SkillID == SkillID;
        }
    }
}