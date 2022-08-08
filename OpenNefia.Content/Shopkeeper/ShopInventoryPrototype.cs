using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.World;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using static ICSharpCode.Decompiler.TypeSystem.ReflectionHelper;
using NuGet.Packaging;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Memory;
using OpenNefia.Content.GameObjects.EntitySystems;

namespace OpenNefia.Content.Shopkeeper
{
    [Prototype("Elona.ShopInventory")]
    public class ShopInventoryPrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;

        [DataField("rules")]
        private List<ShopInventoryRule> _rules { get; } = new();

        public IReadOnlyList<ShopInventoryRule> Rules => _rules;

        [DataField]
        public GameTimeSpan RestockInterval { get; set; } = GameTimeSpan.FromHours(24);

        /// <summary>
        /// If true, allows items with the <see cref="Protos.Tag.ItemNoshop"/> tag to be generated.
        /// </summary>
        [DataField]
        public bool IgnoresNoShop { get; set; }
    }

    public enum ShopInventoryResult
    {
        Continue,
        Abort
    }

    public interface IShopItemArgs
    {
        /// <summary>
        /// Currently generated item's position in the shop inventory.
        /// </summary>
        public int ItemIndex { get; }

        /// <summary>
        /// Item filter to pass to <see cref="IItemGen.GenerateItem(Core.Containers.IContainer, ItemFilter)"/>.
        /// </summary>
        public ItemFilter ItemFilter { get; set; }
    }

    public sealed class ShopItemParams : IShopItemArgs
    {
        public int ItemIndex { get; }
        public ItemFilter ItemFilter { get; set; } = new();

        public ShopItemParams(int itemIndex)
        {
            ItemIndex = itemIndex;
        }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IShopInventoryPredicate
    {
        bool ShouldApply(IShopItemArgs args);
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IShopInventoryAction
    {
        ShopInventoryResult Apply(IShopItemArgs args);
    }

    [DataDefinition]
    public sealed class ShopInventoryRule
    {
        [DataField]
        public int? OneIn { get; set; } = null;

        [DataField]
        public int? AllButOneIn { get; set; } = null;

        [DataField]
        public double? Prob { get; set; } = null;

        [DataField]
        public int? ItemIndex { get; set; } = null;

        [DataField]
        public IShopInventoryPredicate? Predicate { get; }

        [DataField]
        public ShopInventoryModifier Modifier { get; } = new();

        [DataField]
        public IShopInventoryAction? ExtraAction { get; set; }
    }

    #region Standard predicates

    public sealed class IndexShopInventoryPredicate : IShopInventoryPredicate
    {
        [DataField]
        public int ItemIndex { get; set; }

        [DataField]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        public bool ShouldApply(IShopItemArgs args)
        {
            return ComparisonUtils.EvaluateComparison(args.ItemIndex, ItemIndex, Comparison);
        }
    }

    #endregion

    #region Standard actions

    public sealed class AbortShopInventoryAction : IShopInventoryAction
    {
        public ShopInventoryResult Apply(IShopItemArgs args)
            => ShopInventoryResult.Abort;
    }

    public sealed class ChoiceShopInventoryAction : IShopInventoryAction
    {
        [DataField("choices", required: true)]
        public List<ShopInventoryModifier> _choices { get; } = new();

        public IReadOnlyList<ShopInventoryModifier> Choices => _choices;

        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            if (_choices.Count == 0)
                return ShopInventoryResult.Continue;

            var _rand = IoCManager.Resolve<IRandom>();

            var choice = _rand.Pick(_choices);
            return choice.Apply(args);
        }
    }

    public sealed class ShopInventoryModifier
    {
        [DataField]
        public PrototypeId<EntityPrototype>? Id { get; set; }

        [DataField]
        public int? MinLevel { get; set; }

        [DataField]
        public PrototypeId<TagPrototype>[]? Tags { get; set; }

        [DataField]
        public string? Fltselect { get; set; }

        [DataField]
        public int? Amount { get; set; }

        [DataField]
        public Quality? Quality { get; set; }

        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            if (Id != null)
                args.ItemFilter.Id = Id;
            if (MinLevel != null)
                args.ItemFilter.MinLevel = MinLevel.Value;
            if (Tags != null)
            {
                args.ItemFilter.Tags ??= new PrototypeId<TagPrototype>[] {};
                args.ItemFilter.Tags.AddRange(Tags);
            }
            if (Fltselect != null)
                args.ItemFilter.Fltselect = Fltselect;
            if (Amount != null)
                args.ItemFilter.Amount = Amount;
            if (Quality != null)
                args.ItemFilter.Quality = Quality;

            return ShopInventoryResult.Continue;
        }
    }

    #endregion

    #region Special actions

    /// <summary>
    /// TODO: Maybe filter sets can be defined in YAML/config instead?
    /// </summary>
    public sealed class FilterSetWearShopInventoryAction : IShopInventoryAction
    {
        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            var _rand = IoCManager.Resolve<IRandom>();

            args.ItemFilter.Tags = new[] { _rand.Pick(RandomGenConsts.FilterSets.Wear) };

            return ShopInventoryResult.Continue;
        }
    }

    /// <summary>
    /// TODO: Maybe item sets can be defined in YAML/config instead?
    /// </summary>
    public sealed class ItemSetDeedShopInventoryAction : IShopInventoryAction
    {
        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            var _rand = IoCManager.Resolve<IRandom>();

            args.ItemFilter.Id = _rand.Pick(RandomGenConsts.ItemSets.Deed);

            return ShopInventoryResult.Continue;
        }
    }

    public sealed class SpellWriterShopInventoryAction : IShopInventoryAction
    {
        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            var _rand = IoCManager.Resolve<IRandom>();
            var _spellbooks = EntitySystem.Get<ISpellbookSystem>();

            var reserved = _spellbooks.SpellbookReserveStates
                .Where(pair => pair.Value == SpellbookReserveState.Reserved)
                .Select(pair => pair.Key)
                .ToList();

            if (reserved.Count == 0)
                return ShopInventoryResult.Abort;

            args.ItemFilter.Id = _rand.Pick(reserved);
            return ShopInventoryResult.Continue;
        }
    }

    #endregion
}
