using OpenNefia.Content.Damage;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Mefs;
using OpenNefia.Content.World;

namespace OpenNefia.Content.Items.Impl
{
    public interface INuclearBombSystem : IEntitySystem
    {
        TurnResult UseNuclearBomb(EntityUid source, EntityUid target, NuclearBombComponent? nuke = null);
    }

    public sealed class NuclearBombSystem : EntitySystem, INuclearBombSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMefSystem _mefs = default!;

        public override void Initialize()
        {
            SubscribeComponent<NuclearBombComponent, GetVerbsEventArgs>(GetVerbs_Nuke);
        }

        private void GetVerbs_Nuke(EntityUid uid, NuclearBombComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use Nuke", () => UseNuclearBomb(args.Source, args.Target)));
        }

        public TurnResult UseNuclearBomb(EntityUid source, EntityUid target, NuclearBombComponent? nukeComp = null)
        {
            // >>>>>>>> shade2/action.hsp:2027 	case effNuke ...
            if (!Resolve(target, ref nukeComp))
                return TurnResult.Aborted;

            if (!TryMap(source, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                if (_gameSession.IsPlayer(source))
                    _mes.Display(Loc.GetString("Elona.Item.Nuke.CannotPlaceHere", ("source", source), ("item", target)));
                return TurnResult.Aborted;
            }

            // TODO sidequest
            var sidequestActive = false;
            if (!sidequestActive)
            {
                if (_gameSession.IsPlayer(source) && !_playerQuery.YesOrNo(Loc.GetString("Elona.Item.Nuke.PromptNotQuestGoal")))
                {
                    return TurnResult.Aborted;
                }
            }

            _stacks.Use(target, 1);
            _mes.Display(Loc.GetString("Elona.Item.Nuke.SetUp", ("source", source), ("item", target)));
            _audio.Play(Protos.Sound.Build1, source);
            _mefs.SpawnMef(Protos.Mef.NuclearBomb, target, GameTimeSpan.FromMinutes(10), power: nukeComp.Power);
            return TurnResult.Succeeded;
            // <<<<<<<< shade2/action.hsp:2037 	swbreak ..
        }
    }
}