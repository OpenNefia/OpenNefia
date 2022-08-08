using ICSharpCode.Decompiler.IL;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Shopkeeper
{
    public interface IShopkeeperSystem : IEntitySystem
    {
        ShopInventoryResult ApplyShopInventoryModifier(IShopItemArgs args, ShopInventoryModifier modifier);
        ShopInventoryResult ApplyShopInventoryRule(IShopItemArgs args, ShopInventoryRule rule);
        void RefreshShop(EntityUid shopkeeper, RoleShopkeeperComponent? shopkeeperComp = null);
    }

    [DataDefinition]
    public sealed class ShopItemExclusion
    {
        public ShopItemExclusion() { }

        public ShopItemExclusion(PrototypeId<EntityPrototype>? id = null, PrototypeId<TagPrototype>[]? categories = null)
        {
            Id = id;
            Categories = categories;
        }

        [DataField]
        public PrototypeId<EntityPrototype>? Id { get; }

        [DataField]
        public PrototypeId<TagPrototype>[]? Categories { get; }
    }

    public sealed class ShopkeeperSystem : EntitySystem, IShopkeeperSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IContainerSystem _containers = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly ITagSystem _tags = default!;

        public override void Initialize()
        {
        }

        private int CalcShopGeneratedItemCount(EntityUid shopkeeper)
        {
            return Math.Min(80, 20 + (CompOrNull<RoleShopkeeperComponent>(shopkeeper)?.ShopRank ?? 0) / 2);
        }

        private bool TestShopInventoryRulePredicates(ShopInventoryRule rule, IShopItemArgs args)
        {
            if (rule.ItemIndex != null)
            {
                if (args.ItemIndex != rule.ItemIndex)
                    return false;
            }

            // OneIn, AllButOneIn and Prob are mutually exclusive.
            if (rule.OneIn != null)
            {
                if (!_rand.OneIn(rule.OneIn.Value))
                    return false;
            }
            else if (rule.AllButOneIn != null)
            {
                if (_rand.OneIn(rule.AllButOneIn.Value))
                    return false;
            }
            else if (rule.Prob != null)
            {
                if (!_rand.Prob(rule.Prob.Value))
                    return false;
            }

            if (rule.Predicate != null)
            {
                if (!rule.Predicate.ShouldApply(args))
                    return false;
            }

            return true;
        }

        public ShopInventoryResult ApplyShopInventoryModifier(IShopItemArgs args, ShopInventoryModifier modifier)
        {
            var itemFilter = args.ItemFilter;

            if (modifier.Id != null)
                itemFilter.Id = modifier.Id;
            if (modifier.MinLevel != null)
                itemFilter.MinLevel = modifier.MinLevel.Value;
            if (modifier.Tags != null)
            {
                itemFilter.Tags ??= new PrototypeId<TagPrototype>[] { };
                var tags = itemFilter.Tags.ToList();
                tags.AddRange(modifier.Tags);
                itemFilter.Tags = tags.ToArray();
            }
            if (modifier.Fltselect != null)
                itemFilter.Fltselect = modifier.Fltselect;
            if (modifier.Amount != null)
                itemFilter.Amount = modifier.Amount;
            if (modifier.Quality != null)
                itemFilter.Quality = modifier.Quality;

            if (modifier.Action != null)
                return modifier.Action.Apply(args);
            else
                return ShopInventoryResult.Continue;
        }

        public ShopInventoryResult ApplyShopInventoryRule(IShopItemArgs args, ShopInventoryRule rule)
        {
            if (TestShopInventoryRulePredicates(rule, args))
            {
                return ApplyShopInventoryModifier(args, rule.Modifier);
            }
            
            return ShopInventoryResult.Continue;
        }

        private bool ApplyShopInventoryRules(EntityUid shopkeeper, int index, IList<ShopInventoryRule> rules, [NotNullWhen(true)] out ItemFilter? itemFilter)
        {
            var args = new ShopItemArgs(shopkeeper, index);
            var shopRank = CompOrNull<RoleShopkeeperComponent>(args.Shopkeeper)?.ShopRank ?? 0;

            args.ItemFilter = new ItemFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(shopRank),
                Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal)
            };

            foreach (var rule in rules)
            {
                switch (ApplyShopInventoryRule(args, rule))
                {
                    case ShopInventoryResult.Abort:
                        itemFilter = null;
                        return false;
                    case ShopInventoryResult.Continue:
                    default:
                        break;
                }
            }

            itemFilter = args.ItemFilter;
            return true;
        }

        private bool IsItemExcludedFromShops(EntityUid item)
        {
            if (TryComp<TagComponent>(item, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtShopExclusion>(tag, out var exclusion))
                    {
                        if (exclusion.IsExcludedFromShops)
                            return true;
                    }
                }
            }

            if (TryProtoID(item, out var protoID))
            {
                if (_protos.TryGetExtendedData<EntityPrototype, ExtShopExclusion>(protoID.Value, out var exclusion))
                {
                    if (exclusion.IsExcludedFromShops)
                        return true;
                }
            }

            return false;
        }

        private bool ShouldRemoveGeneratedShopItem(EntityUid item, ShopInventoryPrototype invProto)
        {
            // Don't generate items with negative effects.
            if (_tags.HasTag(item, Protos.Tag.ItemNeg))
                return true;

            // Don't generate items with NoShop set, except if the
            // shop ignores that tag (only the souvenir vendor in vanilla,
            // to allow for generating things like treasure maps).
            if (_tags.HasTag(item, Protos.Tag.ItemNoshop) && !invProto.IgnoresNoShop)
                return true;

            // Don't generate cursed or doomed items.
            if (_curseStates.IsCursed(item))
                return true;

            // Special-case exclusions.
            if (IsItemExcludedFromShops(item))
                return true;

            return false;
        }

        private int CalcShopItemMaxAmount(EntityUid item)
        {
            var amount = 1;

            if (TryComp<TagComponent>(item, out var tags))
            {
                foreach (var tag in tags.Tags)
                {
                    if (_protos.TryGetExtendedData<TagPrototype, ExtShopAmountAdjustment>(tag, out var adjust))
                    {
                        amount = adjust.Adjustment.GetAdjustedAmount(item);
                    }
                }
            }

            if (TryProtoID(item, out var protoID))
            {
                if (_protos.TryGetExtendedData<EntityPrototype, ExtShopAmountAdjustment>(protoID.Value, out var adjust))
                {
                    amount = adjust.Adjustment.GetAdjustedAmount(item);
                }
            }

            return Math.Max(amount, 1);
        }

        private void GenerateShopInventory(EntityUid shopkeeper, IContainer shopContainer, PrototypeId<ShopInventoryPrototype> shopInvID)
        {
            var shopInvProto = _protos.Index(shopInvID);

            var rules = new List<ShopInventoryRule>();
            rules.AddRange(shopInvProto.Rules);
            var pevModifyRules = new P_ShopInventoryModifyRulesEvent(shopkeeper, rules);
            _protos.EventBus.RaiseEvent(shopInvProto, pevModifyRules);

            var genCount = CalcShopGeneratedItemCount(shopkeeper);
            var pevCount = new P_ShopInventoryCalcTotalItemCountEvent(shopkeeper, genCount);
            _protos.EventBus.RaiseEvent(shopInvProto, pevCount);
            genCount = pevCount.OutTotalItemCount;

            bool TryGenerate(int index)
            {
                if (!ApplyShopInventoryRules(shopkeeper, index, rules, out var itemFilter))
                    return true;

                itemFilter.CommonArgs.NoStack = true;
                itemFilter.Args.Ensure<ItemGenArgs>().IsShop = true;

                var item = _itemGen.GenerateItem(shopContainer, itemFilter);
                if (!IsAlive(item))
                {
                    // Container is full, don't try to generate more items.
                    return false;
                }

                var pevAfterGenerate = new P_ShopInventoryAfterGenerateItemEvent(shopkeeper, index, item.Value);
                _protos.EventBus.RaiseEvent(shopInvProto, pevAfterGenerate);
                if (pevAfterGenerate.Handled || !IsAlive(item.Value))
                    return true;

                if (ShouldRemoveGeneratedShopItem(item.Value, shopInvProto))
                {
                    EntityManager.DeleteEntity(item.Value);
                    return true;
                }

                // Items are generated with amount 1 by default, but attaching an
                // ExtShopAmountAdjustment to the tag/entity prototypes can increase this.
                var itemAmount = _rand.Next(CalcShopItemMaxAmount(item.Value)) + 1;

                // Cargo traders have special behavior for calculating the sold item number. They
                // also have a chance to discard the item entirely (setting the amount to 0).
                var pevItemAmount = new P_ShopInventoryCalcItemAmountEvent(shopkeeper, index, item.Value, itemAmount);
                itemAmount = pevItemAmount.OutItemAmount;

                if (itemAmount <= 0)
                {
                    EntityManager.DeleteEntity(item.Value);
                    return true;
                }

                // Blessed items are never generated in multiple (per cycle)
                if (_curseStates.IsBlessed(item.Value))
                    itemAmount = 1;

                _stacks.SetCount(item.Value, itemAmount);

                // Shops can adjust the prices of items through a formula.
                var value = EnsureComp<ValueComponent>(item.Value);
                var pevItemValue = new P_ShopInventoryCalcItemBaseValueEvent(shopkeeper, index, item.Value, value.Value);
                value.Value = pevItemValue.OutBaseValue;

                _stacks.TryStackAtSamePos(item.Value);
                return true;
            }

            for (var index = 0; index < genCount; index++)
            {
                if (!TryGenerate(index))
                    break;
            }
        }

        public void RefreshShop(EntityUid shopkeeper, RoleShopkeeperComponent? shopkeeperComp = null)
        {
            if (!Resolve(shopkeeper, ref shopkeeperComp))
                return;

            var invProto = _protos.Index(shopkeeperComp.ShopInventoryId);

            _containers.CleanContainer(shopkeeperComp.ShopContainer);
            GenerateShopInventory(shopkeeper, shopkeeperComp.ShopContainer, shopkeeperComp.ShopInventoryId);

            shopkeeperComp.RestockDate = _world.State.GameDate + invProto.RestockInterval;
        }
    }

    /// <summary>
    /// Allows adjusting the generated item amount in shops for
    /// items with the specified tag (item category) or prototype ID.
    /// </summary>
    public sealed class ExtShopAmountAdjustment
        : IPrototypeExtendedData<TagPrototype>,
          IPrototypeExtendedData<EntityPrototype>
    {
        [DataField]
        public IShopAmountAdjustment Adjustment { get; } = default!;
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IShopAmountAdjustment
    {
        int GetAdjustedAmount(EntityUid item);
    }

    public sealed class ConstantShopAmountAdjustment : IShopAmountAdjustment
    {
        [DataField]
        public int Amount { get; set; } = 1;

        public int GetAdjustedAmount(EntityUid item) => Amount;
    }

    public sealed class RarityShopAmountAdjustment : IShopAmountAdjustment
    {
        [DataField]
        public string RandomGenTableID { get; } = RandomGenTables.Item;

        [DataField]
        public int Coefficient { get; set; } = 100;

        public int GetAdjustedAmount(EntityUid item)
        {
            var entityMan = IoCManager.Resolve<IEntityManager>();

            var rarity = 1000000;
            if (entityMan.TryGetComponent<RandomGenComponent>(item, out var randomGen)
                && randomGen.Tables.TryGetValue(RandomGenTableID, out var table))
                rarity = table.Rarity;

            return (rarity / 1000) / Coefficient;
        }
    }

    public sealed class RandomShopAmountAdjustment : IShopAmountAdjustment
    {
        [DataField]
        public int MinAmount { get; set; } = 0;

        [DataField]
        public int MaxAmount { get; set; } = 1;

        public int GetAdjustedAmount(EntityUid item)
        {
            return IoCManager.Resolve<IRandom>().Next(MinAmount, MaxAmount);
        }
    }

    /// <summary>
    /// Allows excluding an item category or prototype from shops.
    /// </summary>
    public sealed class ExtShopExclusion
        : IPrototypeExtendedData<TagPrototype>,
          IPrototypeExtendedData<EntityPrototype>
    {
        [DataField]
        public bool IsExcludedFromShops { get; set; } = true;
    }
}