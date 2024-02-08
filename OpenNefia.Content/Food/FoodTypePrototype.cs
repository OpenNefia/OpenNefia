using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Items;
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
    /// <summary>
    /// A type of food that can be cooked: fruit, vegetables, meat, etc. 
    /// </summary>
    [Prototype("Elona.FoodType")]
    public class FoodTypePrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        /// <summary>
        /// If <c>true</c>, this food is created by cooking the product
        /// of an animal/creature. Examples are meat ("a corpse of putit")
        /// and eggs ("an egg of putit").
        /// </summary>
        /// <remarks>
        /// The food must have an <see cref="EntityProtoSourceComponent"/>
        /// with the entity ID to use the name of.
        /// </remarks>
        [DataField]
        public bool UsesCharaName { get; } = false;

        [DataField("expGains")]
        private readonly List<ExperienceGain> _expGains = new();
        /// <summary>
        /// Experience gains caused by eating food of this type.
        /// </summary>
        public IReadOnlyList<ExperienceGain> ExpGains => _expGains;

        /// <summary>
        /// Base nutrition before food quality is taken into account.
        /// </summary>
        [DataField]
        public int? BaseNutrition { get; }

        [DataField("itemChips")]
        private readonly Dictionary<int, PrototypeId<ChipPrototype>> _itemChips = new();
        /// <summary>
        /// Mapping of item quality number to the chip to display. The quality should
        /// be between 1-9.
        /// </summary>
        public IReadOnlyDictionary<int, PrototypeId<ChipPrototype>> ItemChips => _itemChips;

        /// <summary>
        /// If this food type is requested in a Cook quest, this is the type of reward
        /// item that quest should give out.
        /// </summary>
        [DataField]
        public PrototypeId<TagPrototype>? QuestRewardCategory { get; }
    }
}