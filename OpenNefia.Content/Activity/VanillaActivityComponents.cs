using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Activity
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityEatingComponent : Component
    {
        [DataField]
        public EntityUid? Food { get; set; }

        [DataField]
        public bool ShowMessage { get; set; } = true;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityReadingSpellbookComponent : Component
    {
        [DataField]
        public EntityUid Spellbook { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityReadingAncientBookComponent : Component
    {
        [DataField]
        public EntityUid AncientBook { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityTravelingComponent : Component
    {
        [DataField]
        public MapCoordinates Destination { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityRestingComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityMiningComponent : Component
    {
        [DataField]
        public MapCoordinates TargetTile { get; set; }

        [DataField]
        public int TurnsSpentMining { get; set; } = 0;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityPerformingComponent : Component
    {
        [DataField]
        public EntityUid Instrument { get; set; }

        [DataField]
        public int PerformanceQuality { get; set; } = 40;

        [DataField]
        public int TotalTipGold { get; set; } = 0;

        [DataField]
        public int TotalNumberOfTips { get; set; } = 0;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityFishingComponent : Component
    {
        [DataField]
        public MapCoordinates TargetTile { get; set; }

        [DataField]
        public EntityUid? FishingPole { get; set; }

        [DataField]
        public bool ShowAnimation { get; set; } = true;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityDiggingSpotComponent : Component
    {
        [DataField]
        public MapCoordinates TargetTile { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivitySexComponent : Component
    {
        [DataField]
        public EntityUid Partner { get; set; }

        [DataField]
        public bool IsTopping { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityPreparingToSleepComponent : Component
    {
        [DataField]
        public EntityUid? Bed { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityHarvestingComponent : Component
    {
        [DataField]
        public EntityUid Item { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityTrainingComponent : Component
    {
        [DataField]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        [DataField]
        public EntityUid? Item { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityPickpocketComponent : Component
    {
        [DataField]
        public EntityUid? TargetItem { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivitySearchingComponent : Component
    {
        [DataField]
        public EntityUid? MaterialSpot { get; set; }

        [DataField]
        public bool NoMoreMaterials { get; set; }
    }
}