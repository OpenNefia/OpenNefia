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
    public sealed class EncAmmoComponent : Component, IEnchantmentComponent
    {
        [DataField]
        public PrototypeId<AmmoEnchantmentPrototype> AmmoEnchantmentID { get; set; }

        [DataField]
        public int CurrentAmmoAmount { get; set; }

        [DataField]
        public int MaxAmmoAmount { get; set; }

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            // TODO is max ammo affected by merge?
            return other is EncAmmoComponent otherAmmo
                && AmmoEnchantmentID == otherAmmo.AmmoEnchantmentID;
        }
    }
}