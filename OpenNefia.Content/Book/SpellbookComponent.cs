using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Book
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SpellbookComponent : Component
    {
        [DataField]
        public PrototypeId<SpellPrototype> SpellID { get; set; }
    }
}