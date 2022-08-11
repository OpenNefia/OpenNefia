using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EnchantmentsComponent : Component
    {
        public override string Name => "Enchantments";

        /// <summary>
        /// Locale ID for indexing into <c>Elona.Enchantments.Ego.Major</c>.
        /// </summary>
        [DataField]
        public LocaleKey? EgoMajorEnchantment { get; set; }

        /// <summary>
        /// Locale ID for indexing into <c>Elona.Enchantments.Ego.Minor</c>.
        /// </summary>
        [DataField]
        public LocaleKey? EgoMinorEnchantment { get; set; }
    }
}