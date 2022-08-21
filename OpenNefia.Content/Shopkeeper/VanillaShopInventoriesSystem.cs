using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Guild;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Utility;
using OpenNefia.Content.CurseStates;

namespace OpenNefia.Content.Shopkeeper
{
    public sealed class VanillaShopInventoriesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        #region Elona.Blackmarket

        public void Blackmarket_CalcTotalItemCount(ShopInventoryPrototype proto, P_ShopInventoryCalcTotalItemCountEvent ev)
        {
            ev.OutTotalItemCount = 6 + (CompOrNull<RoleShopkeeperComponent>(ev.Shopkeeper)?.ShopRank ?? 0) / 10;
        }

        public void Blackmarket_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            if (TryComp<GuildMemberComponent>(_gameSession.Player, out var guild) && guild.GuildID == Protos.Guild.Thief)
            {
                value.Value.Base *= 2;
            }
            else
            {
                value.Value.Base *= 3;
            }
        }

        #endregion

        #region Elona.Trader

        private sealed record CargoAmountRate(int Threshold, ComparisonType Comparison, Func<int, int> Amount, int? DiscardChance = null);

        /// <summary>
        /// Parameters for adjusting the amount of available cargo based on the cargo's fluctuating
        /// value. Cargo of higher value will be more difficult to find in shops. More than one
        /// modifier can be applied if multiple value thresholds are reached.
        /// </summary>
        private static readonly CargoAmountRate[] CargoAmountRates = new CargoAmountRate[]
        {
            new(70, ComparisonType.LessThanOrEqual, (n) => n * 200 / 100),
            new(50, ComparisonType.LessThanOrEqual, (n) => n * 200 / 100),
            new(80, ComparisonType.GreaterThanOrEqual, (n) => n / 2 + 1, DiscardChance: 2),
            new(100, ComparisonType.GreaterThanOrEqual, (n) => n / 2 + 1, DiscardChance: 3),
        };

        private int GetCargoTradeRate(EntityUid item)
        {
            // TODO cargo
            return 0;
        }

        private int CalcCargoItemAmountModifier(int amount)
        {
            return amount * (100 + _skills.Level(_gameSession.Player, Protos.Skill.Negotiation) * 10) / 100 + 1;
        }

        public void Trader_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var rate = GetCargoTradeRate(ev.Item);
            var amount = _stacks.GetCount(ev.Item);

            foreach (var amountRate in CargoAmountRates)
            {
                var apply = false;
                if (ComparisonUtils.EvaluateComparison(rate, amountRate.Threshold, amountRate.Comparison))
                    apply = true;

                if (apply)
                {
                    amount = amountRate.Amount(amount);
                    if (amountRate.DiscardChance.HasValue && _rand.OneIn(amountRate.DiscardChance.Value))
                    {
                        EntityManager.DeleteEntity(ev.Item);
                        return;
                    }
                }
            }

            _stacks.SetCount(ev.Item, CalcCargoItemAmountModifier(amount));
        }

        #endregion

        #region Elona.WanderingMerchant

        public void WanderingMerchant_CalcTotalItemCount(ShopInventoryPrototype proto, P_ShopInventoryCalcTotalItemCountEvent ev)
        {
            ev.OutTotalItemCount = _rand.Next(4, 8);
        }

        public void WanderingMerchant_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            value.Value.Base *= 2;
        }

        #endregion

        #region Elona.Moyer

        public void Moyer_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            value.Value.Base *= 2;
        }

        #endregion

        #region Elona.Miral

        private IEnumerable<EntityPrototype> GetMedalValuedItems()
        {
            return _protos.EnumeratePrototypes<EntityPrototype>()
                .Where(p => p.Components.HasComponent<MedalValueComponent>());
        }

        public void Miral_CalcTotalItemCount(ShopInventoryPrototype proto, P_ShopInventoryCalcTotalItemCountEvent ev)
        {
            ev.OutTotalItemCount = GetMedalValuedItems().Count();
        }

        public void Miral_ModifyRules(ShopInventoryPrototype proto, P_ShopInventoryModifyRulesEvent ev)
        {
            var index = 0;
            foreach (var entityProto in GetMedalValuedItems())
            {
                var modifier = new ShopInventoryModifier(id: entityProto.GetStrongID());
                var rule = new ShopInventoryRule(itemIndex: index, modifier: modifier);

                ev.OutRules.Add(rule);
                index++;
            }
        }

        public void Miral_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            _stacks.SetCount(ev.Item, 1);
            EnsureComp<CurseStateComponent>(ev.Item).CurseState = CurseState.Normal;
            if (ProtoIDOrNull(ev.Item) == Protos.Item.RodOfDomination)
                EnsureComp<ChargedComponent>(ev.Item).Charges = 4;
        }

        #endregion

        #region Elona.SouvenirVendor

        public void SouvenirVendor_CalcTotalItemCount(ShopInventoryPrototype proto, P_ShopInventoryCalcTotalItemCountEvent ev)
        {
            ev.OutTotalItemCount /= 2;
        }

        public void SouvenirVendor_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            var newValue = Math.Clamp(value.Value.Base, 1, 1000000) * 50;
            if (HasComp<GiftComponent>(ev.Item))
                newValue *= 10;
            value.Value.Base = newValue;
        }

        #endregion

        #region Elona.SpellWriter

        public void SpellWriter_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            value.Value.Base = (int)(value.Value.Base * 1.5);
        }

        #endregion

        #region Elona.VisitingMerchant

        public void VisitingMerchant_CalcTotalItemCount(ShopInventoryPrototype proto, P_ShopInventoryCalcTotalItemCountEvent ev)
        {
            ev.OutTotalItemCount = _rand.Next(4, 8);
        }

        public void VisitingMerchant_AfterGenerateItem(ShopInventoryPrototype proto, P_ShopInventoryAfterGenerateItemEvent ev)
        {
            var value = EnsureComp<ValueComponent>(ev.Item);
            value.Value.Base = (int)(value.Value.Base * 1.5);
        }

        #endregion
    }

    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryModifyRulesEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }

        public IList<ShopInventoryRule> OutRules { get; }

        public P_ShopInventoryModifyRulesEvent(EntityUid shopkeeper, IList<ShopInventoryRule> rules)
        {
            Shopkeeper = shopkeeper;
            OutRules = rules;
        }
    }

    /// <summary>
    /// Event for calculating the total number of random items to generate
    /// per shop inventory.
    /// </summary>
    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryCalcTotalItemCountEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }

        public int OutTotalItemCount { get; set; }

        public P_ShopInventoryCalcTotalItemCountEvent(EntityUid shopkeeper, int totalItemCount)
        {
            Shopkeeper = shopkeeper;
            OutTotalItemCount = totalItemCount;
        }
    }
    
    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryAfterGenerateItemEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }
        public int ItemIndex { get; }
        public EntityUid Item { get; }

        public P_ShopInventoryAfterGenerateItemEvent(EntityUid shopkeeper, int itemIndex, EntityUid item)
        {
            Shopkeeper = shopkeeper;
            ItemIndex = itemIndex;
            Item = item;
        }
    }
}