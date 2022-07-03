using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
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
        public int DiceX { get; set; }

        [DataField]
        public int DiceY { get; set; }

        [DataField]
        public PrototypeId<SkillPrototype> AmmoSkill { get; set; }
    }
}