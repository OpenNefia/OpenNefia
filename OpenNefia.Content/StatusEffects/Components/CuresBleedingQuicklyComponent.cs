using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.StatusEffects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class CuresBleedingQuicklyComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<bool> CuresBleedingQuickly { get; set; } = new(true);

        public void Refresh()
        {
            CuresBleedingQuickly.Reset();
        }
    }
}