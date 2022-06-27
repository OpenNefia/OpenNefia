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

namespace OpenNefia.LecchoTorte.QuickStart
{
    public sealed class QuickStartSystem : EntitySystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<NewGameStartedEventArgs>(HandleNewGame, nameof(HandleNewGame));
        }

        private void HandleNewGame(NewGameStartedEventArgs ev)
        {
            var player = _gameSession.Player;

            var coords = EntityManager.GetComponent<SpatialComponent>(player).MapPosition;
            var android = _charaGen.GenerateChara(coords, Protos.Chara.Android);
            if (android != null)
            {
                _parties.RecruitAsAlly(player, android.Value);
            }

            var skills = EntityManager.GetComponent<SkillsComponent>(player);
            skills.Ensure(Protos.Skill.AttrConstitution).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrStrength).Level.Base = 2000;
            skills.Ensure(Protos.Skill.AttrLife).Level.Base = 2000;
            _refresh.Refresh(player);
            _skills.HealToMax(player);

            _mapRenderer.SetTileLayerEnabled(typeof(VanillaAIDebugTileLayer), true);
        }
    }
}