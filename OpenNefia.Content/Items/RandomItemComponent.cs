using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    /// <summary>
    /// Component for items like potions and scrolls that should have a randomized
    /// unidentified name/color.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class RandomItemComponent : Component
    {
        public override string Name => "RandomItem";

        /// <summary>
        /// Type of the entity in its unidentified name.
        /// This localizes as "spellbook" in "a mossy [spellbook]".
        /// All random items should have this field set.
        /// This indexes into <c>Elona.RandomItem.KnownNameRef</c>.
        /// </summary>
        [DataField(required: true)]
        public LocaleKey KnownNameRef { get; }
    }
}