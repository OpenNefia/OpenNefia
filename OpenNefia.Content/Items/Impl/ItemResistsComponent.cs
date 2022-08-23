using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ItemResistsComponent : Component, IComponentRefreshable
    {
        public override string Name => "ItemResists";

        [DataField]
        public Stat<bool> IsFireproof { get; set; } = new(false);
        
        [DataField]
        public Stat<bool> IsAcidproof { get; set; } = new(false);
        
        [DataField]
        public Stat<bool> IsColdproof { get; set; } = new(false);

        public void Refresh()
        {
            IsFireproof.Reset();
            IsAcidproof.Reset();
            IsColdproof.Reset();
        }
    }
}