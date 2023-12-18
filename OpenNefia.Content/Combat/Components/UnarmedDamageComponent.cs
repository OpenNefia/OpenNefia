using OpenNefia.Content.Damage;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class UnarmedDamageComponent : Component
    {
        [DataField("damageType", required: true)]
        public IDamageType DamageType { get; set; } = new DefaultDamageType();
    }

    /// <summary>
    /// Affects the damage message output of this entity's unarmed attacks.
    /// Usually attached to a <see cref="RacePrototype"/>.
    /// </summary>
    // TODO this should be merged with UnarmedDamageComponent but merging component
    // fields between EntityPrototype and a child ComponentRegistry (in RacePrototype, etc.)
    // at deserialize time is not supported yet
    // that's because this component typically goes into RacePrototype while UnarmedDamage
    // typically goes into EntityPrototype
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class UnarmedDamageTextComponent : Component
    {
        /// Key into "Elona.Damage.UnarmedText.<XXX>"
        // TODO rework
        [DataField(required: true)]
        public LocaleKey DamageTextType { get; set; } = "Elona.Default";
    }
}