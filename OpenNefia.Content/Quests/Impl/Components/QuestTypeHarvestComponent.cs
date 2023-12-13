using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must harvest a specified total weight of
    /// food planted in the ground.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1006</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeHarvestComponent : Component
    {
        [DataField]
        public int RequiredWeight { get; set; }

        [DataField]
        public int CurrentWeight { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class HarvestQuestCropComponent : Component
    {
        [DataField]
        public int WeightClass { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class HarvestDeliveryChestComponent : Component
    {
    }
}