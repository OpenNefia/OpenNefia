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
        public override string Name => "EncAmmo";

        [DataField(required: true)]
        public PrototypeId<AmmoEnchantmentPrototype> AmmoEnchantmentID { get; set; }

        [DataField(required: true)]
        public int CurrentAmmoAmount { get; set; }

        [DataField(required: true)]
        public int MaxAmmoAmount { get; set; }

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncAmmoComponent otherAmmo
                && true; // TODO
        }
    }
}