using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Mount
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MountComponent : Component, IComponentRefreshable
    {
        [DataField]
        public EntityUid? Rider { get; set; }

        [DataField]
        public Stat<MountSuitability> Suitability { get; } = new(MountSuitability.Normal);

        public void Refresh()
        {
            Suitability.Reset();
        }
    }

    public enum MountSuitability
    {
        Normal,
        Good,
        Bad
    }
}