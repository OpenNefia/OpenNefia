﻿using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SpellbookComponent : Component
    {
        public override string Name => "Spellbook";

        [DataField]
        public PrototypeId<SpellPrototype> SpellID { get; set; }
    }
}