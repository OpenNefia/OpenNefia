using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Enchantments
{
    [Prototype("Elona.AmmoEnchantment")]
    public class AmmoEnchantmentPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public int RandomWeight { get; } = 1000;

        [DataField]
        public int AmmoAmountFactor { get; } = 1;

        [DataField]
        public int ExtraAmmoAmount { get; } = 0;

        [DataField]
        public int StaminaCost { get; } = 1;
    }
}