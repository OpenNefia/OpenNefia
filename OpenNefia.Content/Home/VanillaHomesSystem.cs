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
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.Roles;

namespace OpenNefia.Content.Home
{
    public sealed class VanillaHomesSystem : EntitySystem
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

        #region Elona.Cave

        public void Cave_OnGenerated(HomePrototype proto, ref P_HomeOnGeneratedEvent ev)
        {
            // TODO sidequest
        }

        #endregion

        #region Elona.SmallCastle

        public void SmallCastle_OnGenerated(HomePrototype proto, ref P_HomeOnGeneratedEvent ev)
        {
            var chara = _charaGen.GenerateChara(ev.Map.AtPos(31, 20), Protos.Chara.Shopkeeper);
            if (IsAlive(chara))
            {
                var shopkeeper = EnsureComp<RoleShopkeeperComponent>(chara.Value);
                shopkeeper.ShopInventoryId = Protos.ShopInventory.GeneralVendor;
                shopkeeper.ShopRank = 10;
            }
            
            chara = _charaGen.GenerateChara(ev.Map.AtPos(9, 20), Protos.Chara.Shopkeeper);
            if (IsAlive(chara))
            {
                var shopkeeper = EnsureComp<RoleShopkeeperComponent>(chara.Value);
                shopkeeper.ShopInventoryId = Protos.ShopInventory.Blacksmith;
                shopkeeper.ShopRank = 12;
            }

            chara = _charaGen.GenerateChara(ev.Map.AtPos(4, 20), Protos.Chara.Shopkeeper);
            if (IsAlive(chara))
            {
                var shopkeeper = EnsureComp<RoleShopkeeperComponent>(chara.Value);
                shopkeeper.ShopInventoryId = Protos.ShopInventory.GoodsVendor;
                shopkeeper.ShopRank = 10;
            }

            chara = _charaGen.GenerateChara(ev.Map.AtPos(4, 11), Protos.Chara.Wizard);
            if (IsAlive(chara))
            {
                EnsureComp<RoleIdentifierComponent>(chara.Value);
            }

            chara = _charaGen.GenerateChara(ev.Map.AtPos(30, 11), Protos.Chara.Bartender);
            if (IsAlive(chara))
            {
                EnsureComp<RoleBartenderComponent>(chara.Value);
            }

            chara = _charaGen.GenerateChara(ev.Map.AtPos(30, 4), Protos.Chara.Healer);
            if (IsAlive(chara))
            {
                EnsureComp<RoleHealerComponent>(chara.Value);
            }

            chara = _charaGen.GenerateChara(ev.Map.AtPos(4, 4), Protos.Chara.Wizard);
            if (IsAlive(chara))
            {
                var shopkeeper = EnsureComp<RoleShopkeeperComponent>(chara.Value);
                shopkeeper.ShopInventoryId = Protos.ShopInventory.MagicVendor;
                shopkeeper.ShopRank = 11;
            }
        }

        #endregion
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(HomePrototype))]
    public sealed class P_HomeOnGeneratedEvent : PrototypeEventArgs
    {
        public IMap Map { get; }
        
        public P_HomeOnGeneratedEvent(IMap map)
        {
            Map = map;
        }
    }
}