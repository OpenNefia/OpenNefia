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
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Data;
using System;
using OpenNefia.Content.Book;

namespace OpenNefia.Content.Shopkeeper
{
    [Prototype("Elona.ShopInventory")]
    public class ShopInventoryPrototype : IPrototype
    {
        /// <inheritdoc />
        [IdDataField]
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
        /// Shopkeeper for whom the item is being generated.
        /// </summary>
        public EntityUid Shopkeeper { get; }

        /// <summary>
        /// Currently generated item's position in the shop inventory.
        /// </summary>
        public int ItemIndex { get; }

        /// <summary>
        /// Item filter to pass to <see cref="IItemGen.GenerateItem(Core.Containers.IContainer, ItemFilter)"/>.
        /// </summary>
        public ItemFilter ItemFilter { get; set; }
    }

    public sealed class ShopItemArgs : IShopItemArgs
    {
        public EntityUid Shopkeeper { get; }
        public int ItemIndex { get; }
        public ItemFilter ItemFilter { get; set; } = new();

        public ShopItemArgs(EntityUid shopkeeper, int itemIndex)
        {
            Shopkeeper = shopkeeper;
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
        public ShopInventoryRule() { }

        public ShopInventoryRule(ShopInventoryModifier modifier, int? oneIn = null, int? allButOneIn = null, float? prob = null, int? itemIndex = null, IShopInventoryPredicate? predicate = null)
        {
            Modifier = modifier;
            OneIn = oneIn;
            AllButOneIn = allButOneIn;
            Prob = prob;
            ItemIndex = itemIndex;
            Predicate = predicate;
        }

        [DataField]
        public ShopInventoryModifier Modifier { get; } = new();

        [DataField]
        public int? OneIn { get; } = null;

        [DataField]
        public int? AllButOneIn { get; } = null;

        [DataField]
        public float? Prob { get; } = null;

        [DataField]
        public int? ItemIndex { get; } = null;

        [DataField]
        public IShopInventoryPredicate? Predicate { get; }
    }

    [DataDefinition]
    public sealed class ShopInventoryModifier
    {
        public ShopInventoryModifier() { }

        public ShopInventoryModifier(PrototypeId<EntityPrototype>? id = null, int? minLevel = null, PrototypeId<TagPrototype>[]? tags = null, string? fltselect = null, int? amount = null, Quality? quality = null, IShopInventoryAction? action = null)
        {
            Id = id;
            MinLevel = minLevel;
            Tags = tags;
            Fltselect = fltselect;
            Amount = amount;
            Quality = quality;
            Action = action;
        }

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

        [DataField]
        public IShopInventoryAction? Action { get; }
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

    public sealed class ChoicesShopInventoryAction : IShopInventoryAction
    {
        [DataField("choices", required: true)]
        public List<ShopInventoryModifier> _choices { get; } = new();

        public IReadOnlyList<ShopInventoryModifier> Choices => _choices;

        public ShopInventoryResult Apply(IShopItemArgs args)
        {
            if (_choices.Count == 0)
                return ShopInventoryResult.Continue;

            var _rand = IoCManager.Resolve<IRandom>();
            var _shopkeepers = EntitySystem.Get<IShopkeeperSystem>();

            var choice = _rand.Pick(_choices);
            return _shopkeepers.ApplyShopInventoryModifier(args, choice);
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

            args.ItemFilter.Tags = new[] { _randomGen.PickTag(Protos.TagSet.ItemWear) };

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
