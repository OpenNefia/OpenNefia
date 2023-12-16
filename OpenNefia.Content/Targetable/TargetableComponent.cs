using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Targetable
{
    /// <summary>
    /// Controls whether an entity should be targeted by various things.
    /// This is so entities like the player's current mount cannot be damaged
    /// by things like ball magic. This flag is open to interpretation depending
    /// on the system.
    /// 
    /// NOTE: If this component is absent from an entity, it is assumed to be targetable.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TargetableComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<bool> IsTargetable { get; } = new(true);

        public void Refresh()
        {
            IsTargetable.Reset();
        }
    }
}