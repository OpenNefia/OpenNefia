using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SplittableComponent : Component, IComponentRefreshable
    {
        public override string Name => "Splittable";

        // TODO merge the following two into Stat<float?>
        [DataField]
        public Stat<bool> SplitsOnHighDamage { get; set; } = new(false);

        [DataField]
        public Stat<float> SplitOnHighDamageChance { get; set; } = new(0.1f);

        [DataField]
        public Stat<int> SplitOnHighDamageThreshold { get; set; } = new(20);

        // TODO merge the following two into Stat<float?>
        [DataField]
        public Stat<bool> SplitsRandomlyWhenAttacked { get; set; } = new(false);

        [DataField]
        public Stat<float> SplitRandomlyWhenAttackedChance { get; set; } = new(0.3333f);

        public void Refresh()
        {
            SplitsOnHighDamage.Reset();
            SplitOnHighDamageChance.Reset();
            SplitOnHighDamageThreshold.Reset();
            SplitsRandomlyWhenAttacked.Reset();
            SplitRandomlyWhenAttackedChance.Reset();
        }
    }
}