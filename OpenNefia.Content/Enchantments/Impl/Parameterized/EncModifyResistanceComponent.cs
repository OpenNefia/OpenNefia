﻿using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
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
    public sealed class EncModifyResistanceComponent : Component, IEnchantmentComponent
    {
        [DataField]
        public PrototypeId<ElementPrototype> ElementID { get; set; }

        public bool CanMergeWith(IEnchantmentComponent other)
        {
            return other is EncModifyResistanceComponent otherModifyResistance
                && otherModifyResistance.ElementID == ElementID;
        }
    }
}