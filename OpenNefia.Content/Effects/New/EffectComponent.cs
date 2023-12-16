using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeMagicComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeActionComponent : Component
    {
    }

    /// <summary>
    /// Casts a bolt in a line that can hit multiple targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBoltComponent : Component
    {
    }

    /// <summary>
    /// Casts a magic missile that strikes a single target from a distance.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaArrowComponent : Component
    {
    }

    /// <summary>
    /// Applies the effect directly to the target without any further
    /// checks or animations.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaDirectComponent : Component
    {
    }

    /// <summary>
    /// Casts a ball that hits all targets in an area of effect.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBallComponent : Component
    {
    }

    /// <summary>
    /// Causes elemental damage to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageElementalComponent : Component
    {
        [DataField]
        public PrototypeId<ElementPrototype> Element { get; set; } = Protos.Element.Fire;
    }

    [EventUsage(EventTarget.Effect)]
    public sealed class EffectHitEvent : HandledEntityEventArgs
    {
    }
}