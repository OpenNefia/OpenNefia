﻿using OpenNefia.Content.Damage;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
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
        public override string Name => "UnarmedDamage";

        [DataField("damageType", required: true)]
        public IDamageType DamageType { get; set; } = new DefaultDamageType();
    }
}