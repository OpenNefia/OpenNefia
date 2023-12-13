using OpenNefia.Content.Food;
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
    /// Quest in which the player must give the client a specific cooked food.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1003</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeCookComponent : Component
    {
        [DataField]
        public PrototypeId<FoodTypePrototype> TargetFoodType { get; set; } = Protos.FoodType.Meat;

        [DataField]
        public int TargetFoodQuality { get; set; } = 3;
    }
}