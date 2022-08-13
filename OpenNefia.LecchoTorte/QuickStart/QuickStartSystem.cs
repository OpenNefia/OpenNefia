using OpenNefia.Content.CharaMake;
using OpenNefia.Content.Parties;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Game;
using OpenNefia.Content.Skills;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Resists;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Food;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Currency;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Items.Impl;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Chest;

namespace OpenNefia.LecchoTorte.QuickStart
{
    public sealed class QuickStartSystem : EntitySystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<NewGameStartedEventArgs>(HandleNewGame);
        }

        private void HandleNewGame(NewGameStartedEventArgs ev)
        {
            var player = _gameSession.Player;

            var coords = EntityManager.GetComponent<SpatialComponent>(player).MapPosition;
            var ally = _charaGen.GenerateChara(coords, Protos.Chara.GoldenKnight);
            if (ally != null)
            {
                _parties.RecruitAsAlly(player, ally.Value);
            } 
            
            var skills = EntityManager.GetComponent<SkillsComponent>(player);
            skills.Ensure(Protos.Skill.AttrConstitution).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrStrength).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrLife).Level.Base = 2000;
            _refresh.Refresh(player);
            _damage.HealToMax(player);

            skills = EntityManager.GetComponent<SkillsComponent>(ally!.Value);
            skills.Ensure(Protos.Skill.AttrConstitution).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrStrength).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrLife).Level.Base = 2000;
            _refresh.Refresh(ally.Value);
            _damage.HealToMax(ally.Value);

            var wallet = EntityManager.GetComponent<MoneyComponent>(player);
            wallet.Gold = 1000000;
            wallet.Platinum = 1000;

            _mapRenderer.SetTileLayerEnabled(typeof(VanillaAIDebugTileLayer), true);

            var testEv = new P_ElementKillCharaEvent(null, player);
            _protoMan.EventBus.RaiseEvent(Protos.Element.Fire, testEv);

            var map = _mapMan.ActiveMap!;

            foreach (var proto in _protoMan.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.HasComponent<FoodComponent>())
                    _itemGen.GenerateItem(map.AtPos((2, 2)), proto.GetStrongID(), amount: 99);
            }

            foreach (var proto in _protoMan.EnumeratePrototypes<MusicPrototype>())
            {
                var item = _itemGen.GenerateItem(map.AtPos((2, 3)), Protos.Item.Disc);
                if (IsAlive(item))
                    Comp<MusicDiscComponent>(item.Value).MusicID = proto.GetStrongID();
            }

            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.KittyBank);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.Textbook);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.Book);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.MonsterBall, minLevel: 100, amount: 99);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.Wallet, amount: 99);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.Suitcase, amount: 99);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.BookOfRachel);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.Bill);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.AncientBook);
            _itemGen.GenerateItem(map.AtPos(2, 4), Protos.Item.SpellbookOfAcidGround);

            _itemGen.GenerateItem(map.AtPos(3, 4), Protos.Item.GoldPiece, amount: 1000);
            _itemGen.GenerateItem(map.AtPos(3, 4), Protos.Item.PlatinumCoin, amount: 50);

            foreach (var proto in _protoMan.EnumeratePrototypes<EntityPrototype>().Where(p => p.Components.HasComponent<ChestComponent>()))
            {
                _itemGen.GenerateItem(map.AtPos(3, 2), proto.GetStrongID(), amount: 99);
            }

            _itemGen.GenerateItem(map.AtPos(3, 2), Protos.Item.Lockpick, amount: 999);
            _itemGen.GenerateItem(map.AtPos(3, 2), Protos.Item.SkeletonKey, amount: 999);

            foreach (var identify in _entityLookup.EntityQueryInMap<IdentifyComponent>(map))
            {
                identify.IdentifyState = IdentifyState.Full;
            }
        }
    }
}