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
    /// <summary>
    /// An entity that can be eaten with the [e]at command.
    /// Restores hunger and provides various effects.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class FoodComponent : Component
    {
        /// <summary>
        /// Type of the food. Controls the benefits from eating it.
        /// </summary>
        [DataField]
        public PrototypeId<FoodTypePrototype>? FoodType { get; set; }

        /// <summary>
        /// Quality of the food, usually 0-9. Values > 0 indicate
        /// the food is cooked.
        /// </summary>
        [DataField]
        public int FoodQuality { get; set; } = 0;

        /// <summary>
        /// Nutrition before any modifiers are taken into account.
        /// </summary>
        [DataField]
        public int BaseNutrition { get; set; } = 2500;

        /// <summary>
        /// If non-null, overrides any nutrition modifiers on the food.
        /// </summary>
        [DataField]
        public int? Nutrition { get; set; }

        /// <summary>
        /// Experience gains caused by eating the food.
        /// </summary>
        [DataField]
        public List<ExperienceGain> ExperienceGains { get; } = new();

        /// <summary>
        /// Amount of time it takes for this food to spoil.
        /// If <c>null</c>, this food will never spoil.
        /// </summary>
        [DataField]
        public GameTimeSpan? SpoilageInterval { get; set; }

        #region Instance fields

        /// <summary>
        /// The exact date this food will spoil.
        /// </summary>
        /// <remarks>
        /// Instance field, shouldn't be set in prototypes.
        /// </remarks>
        [DataField]
        public GameDateTime? SpoilageDate { get; set; }

        /// <summary>
        /// True if the food is rotten, and will cause adverse effects if eaten.
        /// </summary>
        /// <remarks>
        /// Instance field, shouldn't be set in prototypes.
        /// </remarks>
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