using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AmmoComponent : Component
    {
        public override string Name => "Ammo";

        [DataField]
        public PrototypeId<SkillPrototype> AmmoSkill { get; set; }

        [DataField]
        public Stat<int> DiceX { get; set; } = new(0);

        [DataField]
        public Stat<int> DiceY { get; set; } = new(0);
    }
}