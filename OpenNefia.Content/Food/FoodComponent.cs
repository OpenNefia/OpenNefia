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
        public PrototypeId<FoodTypePrototype>? Type { get; set; }

        [DataField]
        public int Quality { get; set; } = 0;

        [DataField]
        public int Nutrition { get; set; } = 0;

        [DataField]
        public List<ExperienceGain> ExpGains { get; } = new();

        [DataField]
        public GameTimeSpan? SpoilageDuration { get; set; }

        [DataField]
        public GameDateTime? SpoilageDate { get; set; }

        [DataField]
        public bool IsRotten { get; set; } = false;
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