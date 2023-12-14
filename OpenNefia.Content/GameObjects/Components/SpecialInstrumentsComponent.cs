using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// These flags are set by associated enchantments, hence why they're not their own components.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SpecialInstrumentsComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<bool> IsStradivarius { get; set; } = new(false);

        [DataField]
        public Stat<bool> IsGouldsPiano { get; set; } = new(false);

        public void Refresh()
        {
            IsStradivarius.Reset();
            IsGouldsPiano.Reset();
        }
    }
}