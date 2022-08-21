using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.LivingWeapon
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LivingWeaponComponent : Component
    {
        public override string Name => "LivingWeapon";

        [DataField]
        public Stat<int> Level { get; set; } = new(1);

        [DataField]
        public int Experience { get; set; }

        [DataField]
        public int ExperienceToNext { get; set; }

        [DataField]
        public int RandomSeed { get; set; }
    }
}