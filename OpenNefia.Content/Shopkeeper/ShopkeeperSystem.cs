using OpenNefia.Content.Cargo;
using OpenNefia.Content.Chest;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Guild;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Items;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Book;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Shopkeeper
{
    public enum ItemValueMode
    {
        /// <summary>
        /// The player is buying this item from a shopkeeper.
        /// </summary>
        Buy,

        /// <summary>
        /// The player is selling this item to a shopkeeper.
        /// </summary>
        Sell,

        /// <summary>
        /// The player is selling this item in their own shop.
        /// </summary>
        PlayerShop
    }

    public interface IShopkeeperSystem : IEntitySystem
    {
        ShopInventoryResult ApplyShopInventoryModifier(IShopItemArgs args, ShopInventoryModifier modifier);
        ShopInventoryResult ApplyShopInventoryRule(IShopItemArgs args, ShopInventoryRule rule);
        void RestockShop(EntityUid shopkeeper, RoleShopkeeperComponent? shopkeeperComp = null);

        int CalcItemValue(EntityUid player, EntityUid item, ItemValueMode mode, bool isShop = false);
        bool CanSellItemToShopkeeper(EntityUid player, EntityUid shopkeeper, EntityUid item);
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
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IContainerSystem _containers = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IRandomItemSystem _randomItems = default!;

        public override void Initialize()
        {
            SubscribeComponent<RoleShopkeeperComponent, GetDisplayNameEventArgs>(HandleGetDisplayName, priority: EventPriorities.Low);
        }

        private void HandleGetDisplayName(EntityUid uid, RoleShopkeeperComponent component, ref GetDisplayNameEventArgs args)
        {
            if (component.ShowTitleInName && Loc.TryGetPrototypeString(component.ShopInventoryId, "Title", out var name, ("name", args.OutName)))
            {
                args.OutName = name;
            }
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
                Quality = _randomGen.CalcObjectQuality(Quality.Normal)
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

                EnsureComp<ValueComponent>(item.Value);

                // Items are generated with amount 1 by default, but attaching an
                // ExtShopAmountAdjustment to the tag/entity prototypes can increase this.
                var itemAmount = _rand.Next(CalcShopItemMaxAmount(item.Value)) + 1;

                // Blessed items are never generated in multiple (per cycle).
                if (_curseStates.IsBlessed(item.Value))
                    itemAmount = 1;

                _stacks.SetCount(item.Value, itemAmount);

                var pevAfterGenerate = new P_ShopInventoryAfterGenerateItemEvent(shopkeeper, index, item.Value);
                _protos.EventBus.RaiseEvent(shopInvProto, pevAfterGenerate);
                if (!IsAlive(item.Value))
                    return true;

                if (ShouldRemoveGeneratedShopItem(item.Value, shopInvProto))
                {
                    EntityManager.DeleteEntity(item.Value);
                    return true;
                }

                _stacks.TryStackAtSamePos(item.Value);
                return true;
            }

            for (var index = 0; index < genCount; index++)
            {
                if (!TryGenerate(index))
                    break;
            }
        }

        public void RestockShop(EntityUid shopkeeper, RoleShopkeeperComponent? shopkeeperComp = null)
        {
            if (!Resolve(shopkeeper, ref shopkeeperComp))
                return;

            var invProto = _protos.Index(shopkeeperComp.ShopInventoryId);

            _containers.CleanContainer(shopkeeperComp.ShopContainer);
            GenerateShopInventory(shopkeeper, shopkeeperComp.ShopContainer, shopkeeperComp.ShopInventoryId);

            shopkeeperComp.RestockDate = _world.State.GameDate + invProto.RestockInterval;
        }

        public int CalcItemValue(EntityUid player, EntityUid item, ItemValueMode mode, bool isShop = false)
        {
            // >>>>>>>> shade2/calculation.hsp:595 #defcfunc calcItemValue int id ,int mode ..
            var identifyState = CompOrNull<IdentifyComponent>(item)?.IdentifyState ?? IdentifyState.None;
            var curseState = CompOrNull<CurseStateComponent>(item)?.CurseState ?? CurseState.Normal;
            var baseValue = CompOrNull<ValueComponent>(item)?.Value.Buffed ?? 0;

            int value;

            if (identifyState == IdentifyState.None)
            {
                if (mode == ItemValueMode.PlayerShop)
                {
                    value = (int)(baseValue * 0.4);
                }
                else
                {
                    var pcLevel = _levels.GetLevel(player);
                    var randIndex = _randomItems.GetRandomEntityIndex(item);
                    value = (int)(pcLevel / 5 * ((randIndex * 31) % pcLevel + 4) + 10);
                }
            }
            else
            {
                if (HasComp<EquipmentComponent>(item))
                {
                    switch (identifyState)
                    {
                        case IdentifyState.Name:
                            value = (int)(baseValue * 0.2);
                            break;
                        case IdentifyState.Quality:
                            value = (int)(baseValue * 0.5);
                            break;
                        case IdentifyState.Full:
                        default:
                            value = baseValue;
                            break;
                    }
                }
                else
                {
                    value = baseValue;
                }
            }

            if (identifyState >= IdentifyState.Full)
            {
                switch (curseState)
                {
                    case CurseState.Blessed:
                        value = (int)(value * 1.2);
                        break;
                    case CurseState.Cursed:
                        value /= 2;
                        break;
                    case CurseState.Doomed:
                        value /= 5;
                        break;
                }
            }

            var food = CompOrNull<FoodComponent>(item);
            var cargo = CompOrNull<CargoComponent>(item);

            if (cargo != null && food != null)
            {
                if (mode == ItemValueMode.Buy)
                {
                    var fame = CompOrNull<FameComponent>(player)?.Fame?.Buffed ?? 0;
                    value += Math.Clamp(fame / 40 + value * (fame / 80) / 100, 0, 800);
                }
            }
            else if (food != null)
            {
                var quality = food.FoodQuality;
                if (quality > 0)
                    value = value * quality * quality / 10;
            }
            else if (cargo != null)
            {
                if (isShop)
                {
                    // TODO cargo
                    var tradeRate = 1;
                    value = value * tradeRate / 100;
                    if (mode == ItemValueMode.Sell)
                    {
                        return (int)(value * 0.65);
                    }
                    else
                    {
                        return value;
                    }
                }
            }

            if (TryComp<ChargedComponent>(item, out var charged))
            {
                if (charged.Charges == 0)
                {
                    value /= 10;
                }
                else
                {
                    if (HasComp<SpellbookComponent>(item))
                    {
                        value = value / 5 + value * charged.Charges / (charged.MaxCharges * 2 + 1);
                    }
                    else
                    {
                        value = value / 2 + value * charged.Charges / (charged.MaxCharges * 3 + 1);
                    }
                }
            }

            if (TryComp<ChestComponent>(item, out var chest))
            {
                if (!chest.HasItems)
                {
                    value = value / 100 + 1;
                }
            }

            var negotiation = _skills.Level(player, Protos.Skill.Negotiation);

            switch (mode)
            {
                case ItemValueMode.Buy:
                    var valueLimit = value / 2;
                    value = (int)(value * 100.0 / (100.0 + negotiation));
                    if (CompOrNull<GuildMemberComponent>(player)?.GuildID == Protos.Guild.Mage
                        && HasComp<SpellbookComponent>(item))
                        value = (int)(value * 0.8);
                    value = Math.Max(value, valueLimit);
                    break;
                case ItemValueMode.Sell:
                    valueLimit = Math.Min(negotiation * 250 + 5000, value / 3);
                    value = (int)(value * (100.0 + negotiation * 5.0) / 1000.0);
                    if (HasComp<EquipmentComponent>(item))
                    {
                        value /= 20;
                    }
                    else
                    {
                        if (HasComp<StolenComponent>(item))
                        {
                            if (CompOrNull<GuildMemberComponent>(player)?.GuildID == Protos.Guild.Thief)
                            {
                                value = value / 3 * 2;
                            }
                            else
                            {
                                value /= 10;
                            }
                        }
                    }
                    value = Math.Min(value, valueLimit);
                    break;
                case ItemValueMode.PlayerShop:
                    value /= 5;
                    if (HasComp<EquipmentComponent>(item))
                        value /= 3;
                    value = Math.Min(value, 15000);
                    if (HasComp<StolenComponent>(item))
                        value = 1;
                    break;
            }

            if (_config.GetCVar(CCVars.DebugDevelopmentMode))
                value = Math.Max(value, 1);

            return value;
            // <<<<<<<< shade2/calculation.hsp:670 	return value ..
        }

        public bool CanSellItemToShopkeeper(EntityUid player, EntityUid shopkeeper, EntityUid item)
        {
            if (HasComp<CargoComponent>(item))
                return false;

            if (!TryComp<ValueComponent>(item, out var value) || value.Value.Buffed <= 1)
                return false;

            if (TryComp<ItemComponent>(item, out var itemComp) && itemComp.IsPrecious)
                return false;

            if (TryComp<FoodComponent>(item, out var food) && food.IsRotten)
                return false;

            if (TryComp<QualityComponent>(item, out var quality) && quality.Quality.Base == Quality.Unique)
                return false;

            return true;
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