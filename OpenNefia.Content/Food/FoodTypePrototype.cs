using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Food
{
    [Prototype("Elona.FoodType")]
    public class FoodTypePrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public bool UsesCharaName { get; } = false;

        [DataField("expGains")]
        private readonly List<ExperienceGain> _expGains = new();
        public IReadOnlyList<ExperienceGain> ExpGains => _expGains;

        [DataField]
        public int? BaseNutrition { get; }

        [DataField("itemChips")]
        private readonly Dictionary<int, PrototypeId<ChipPrototype>> _itemChips = new();
        public IReadOnlyDictionary<int, PrototypeId<ChipPrototype>> ItemChips => _itemChips;

        [DataField]
        public string? QuestRewardCategory { get; }
    }
}