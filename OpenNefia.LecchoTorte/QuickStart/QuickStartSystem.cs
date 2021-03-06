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

            _mapRenderer.SetTileLayerEnabled(typeof(VanillaAIDebugTileLayer), true);

            var testEv = new P_ElementKillCharaEvent(null, player);
            _protoMan.EventBus.RaiseEvent(Protos.Element.Fire, testEv);

            var map = _mapMan.ActiveMap!;

            foreach (var proto in _protoMan.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.Components.HasComponent<FoodComponent>())
                    _itemGen.GenerateItem(map.AtPos((2, 2)), proto.GetStrongID(), amount: 99);
            }
        }
    }
}