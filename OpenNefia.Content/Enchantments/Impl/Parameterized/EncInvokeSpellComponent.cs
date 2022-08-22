using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
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
    public sealed class EncInvokeSpellComponent : Component, IEnchantmentComponent
    {
        public override string Name => "EncInvokeSpell";

        [DataField(required: true)]
        public PrototypeId<EnchantmentSpellPrototype> EnchantmentSpellID { get; set; }

        public string? Description => null;

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncInvokeSpellComponent otherInvokeSpell
                && otherInvokeSpell.EnchantmentSpellID == EnchantmentSpellID;
        }
    }
}