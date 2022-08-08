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

        #region Elona.Blackmarket

        public void Blackmarket_CalcItemAmount(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemAmountEvent ev)
        {
            ev.OutItemAmount = 6 + (CompOrNull<RoleShopkeeperComponent>(ev.Shopkeeper)?.ShopRank ?? 0) / 10;
        }

        public void Blackmarket_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            if (TryComp<GuildMemberComponent>(_gameSession.Player, out var guild) && guild.GuildID == Protos.Guild.Thief)
            {
                ev.OutBaseValue *= 2;
            }
            else
            {
                ev.OutBaseValue *= 3;
            }
        }

        #endregion

        #region Elona.WanderingMerchant

        public void WanderingMerchant_CalcItemAmount(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemAmountEvent ev)
        {
            ev.OutItemAmount = _rand.Next(4, 8);
        }

        public void WanderingMerchant_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            ev.OutBaseValue *= 2;
        }

        #endregion

        #region Elona.Moyer

        public void Moyer_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            ev.OutBaseValue *= 2;
        }

        #endregion

        #region Elona.Miral

        private IEnumerable<EntityPrototype> GetMedalValuedItems()
        {
            return _protos.EnumeratePrototypes<EntityPrototype>()
                .Where(p => p.Components.HasComponent<MedalValueComponent>());
        }
        
        public void Miral_CalcItemAmount(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemAmountEvent ev)
        {
            ev.OutItemAmount = GetMedalValuedItems().Count();
        }

        public void Miral_ModifyRules(ShopInventoryPrototype proto, ref P_ShopInventoryModifyRulesEvent ev)
        {
            var i = 0;
            foreach (var entityProto in GetMedalValuedItems())
            {
                var rule = new ShopInventoryRule()
                {

                };

                ev.OutRules.Add(rule);
            }
        }

        public void Miral_AfterGenerateItem(ShopInventoryPrototype proto, ref P_ShopInventoryAfterGenerateItemEvent ev)
        {
            _stacks.SetCount(ev.Item, 1);
            EnsureComp<CurseStateComponent>(ev.Item).CurseState = CurseState.Normal;
            if (ProtoIDOrNull(ev.Item) == Protos.Item.RodOfDomination)
                EnsureComp<ChargedComponent>(ev.Item).Charges = 4;
        }

        #endregion

        #region Elona.SouvenirVendor

        public void SouvenirVendor_CalcItemAmount(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemAmountEvent ev)
        {
            ev.OutItemAmount /= 2;
        }

        public void SouvenirVendor_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            var price = Math.Clamp(ev.OutBaseValue, 1, 1000000) * 50;
            if (HasComp<GiftComponent>(ev.Item))
                price *= 10;
            ev.OutBaseValue = price;
        }

        #endregion

        #region Elona.SpellWriter

        public void SpellWriter_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            ev.OutBaseValue = (int)(ev.OutBaseValue * 1.5);
        }

        #endregion

        #region Elona.VisitingMerchant

        public void VisitingMerchant_CalcItemAmount(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemAmountEvent ev)
        {
            ev.OutItemAmount = _rand.Next(4, 8);
        }

        public void VisitingMerchant_CalcItemBaseValue(ShopInventoryPrototype proto, ref P_ShopInventoryCalcItemBaseValueEvent ev)
        {
            ev.OutBaseValue = (int)(ev.OutBaseValue * 1.5);
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

    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryCalcItemAmountEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }
        
        public int OutItemAmount { get; set; }

        public P_ShopInventoryCalcItemAmountEvent(EntityUid shopkeeper, int itemAmount)
        {
            Shopkeeper = shopkeeper;
            OutItemAmount = itemAmount;
        }
    }

    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryCalcItemBaseValueEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }
        public EntityUid Item { get; }
        
        public int OutBaseValue { get; set; }

        public P_ShopInventoryCalcItemBaseValueEvent(EntityUid shopkeeper, EntityUid item, int baseValue)
        {
            Shopkeeper = shopkeeper;
            Item = item;
            OutBaseValue = baseValue;
        }
    }

    [PrototypeEvent(typeof(ShopInventoryPrototype))]
    public sealed class P_ShopInventoryAfterGenerateItemEvent : PrototypeEventArgs
    {
        public EntityUid Shopkeeper { get; }
        public EntityUid Item { get; }

        public P_ShopInventoryAfterGenerateItemEvent(EntityUid shopkeeper, EntityUid item)
        {
            Shopkeeper = shopkeeper;
            Item = item;
        }
    }
}