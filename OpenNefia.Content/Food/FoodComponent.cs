using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Food
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class FoodComponent : Component
    {
        public override string Name => "Food";

        [DataField]
        public PrototypeId<FoodTypePrototype>? FoodType { get; set; }

        [DataField]
        public int FoodQuality { get; set; } = 0;

        [DataField]
        public int BaseNutrition { get; set; } = 2500;

        [DataField]
        public int? Nutrition { get; set; }

        [DataField]
        public List<ExperienceGain> ExperienceGains { get; } = new();

        [DataField]
        public GameTimeSpan? SpoilageInterval { get; set; }

        #region Instance fields

        [DataField]
        public GameDateTime? SpoilageDate { get; set; }

        [DataField]
        public bool IsRotten { get; set; } = false;

        #endregion
    }

    [DataDefinition]
    public sealed class ExperienceGain
    {
        [DataField]
        public PrototypeId<SkillPrototype> SkillID { get; set; }

        [DataField]
        public int Experience { get; set; } = 10;
    }
}