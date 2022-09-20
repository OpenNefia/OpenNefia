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
        [DataField]
        public PrototypeId<EnchantmentSpellPrototype> EnchantmentSpellID { get; set; }

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncInvokeSpellComponent otherInvokeSpell
                && otherInvokeSpell.EnchantmentSpellID == EnchantmentSpellID;
        }
    }
}