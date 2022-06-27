using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.StatusEffects
{
    [RegisterComponent]
    public sealed class StatusSickComponent : Component
    {
        public override string Name => "StatusSick";
    }

    [RegisterComponent]
    public sealed class StatusPoisonComponent : Component
    {
        public override string Name => "StatusPoison";
    }

    [RegisterComponent]
    public sealed class StatusSleepComponent : Component
    {
        public override string Name => "StatusSleep";
    }

    [RegisterComponent]
    public sealed class StatusBlindnessComponent : Component
    {
        public override string Name => "StatusBlindness";
    }

    [RegisterComponent]
    public sealed class StatusParalysisComponent : Component
    {
        public override string Name => "StatusParalysis";
    }

    [RegisterComponent]
    public sealed class StatusChokingComponent : Component
    {
        public override string Name => "StatusChoking";
    }

    [RegisterComponent]
    public sealed class StatusConfusionComponent : Component
    {
        public override string Name => "StatusConfusion";
    }

    [RegisterComponent]
    public sealed class StatusFearComponent : Component
    {
        public override string Name => "StatusFear";
    }

    [RegisterComponent]
    public sealed class StatusDimmingComponent : Component
    {
        public override string Name => "StatusDimming";
    }

    [RegisterComponent]
    public sealed class StatusFuryComponent : Component
    {
        public override string Name => "StatusFury";
    }

    [RegisterComponent]
    public sealed class StatusBleedingComponent : Component
    {
        public override string Name => "StatusBleeding";
    }

    [RegisterComponent]
    public sealed class StatusInsanityComponent : Component
    {
        public override string Name => "StatusInsanity";
    }

    [RegisterComponent]
    public sealed class StatusDrunkComponent : Component
    {
        public override string Name => "StatusDrunk";
    }

    [RegisterComponent]
    public sealed class StatusWetComponent : Component
    {
        public override string Name => "StatusWet";
    }

    [RegisterComponent]
    public sealed class StatusGravityComponent : Component
    {
        public override string Name => "StatusGravity";
    }
}
