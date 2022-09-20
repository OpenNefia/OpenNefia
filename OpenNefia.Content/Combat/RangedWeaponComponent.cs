using OpenNefia.Content.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class RangedWeaponComponent : Component
    {
        [DataField]
        public PrototypeId<ChipPrototype>? AnimChip { get; }

        [DataField]
        public Color? AnimColor { get; }

        [DataField]
        public SoundSpecifier? AnimSound { get; }

        [DataField]
        public SoundSpecifier? AnimImpactSound { get; }

        [DataField]
        public IRangedAccuracy RangedAccuracy { get; } = new RangedAccuracyTable();
    }
}