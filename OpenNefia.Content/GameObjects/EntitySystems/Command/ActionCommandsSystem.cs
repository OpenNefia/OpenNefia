using OpenNefia.Content.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Mining;

namespace OpenNefia.Content.GameObjects
{
    public class ActionCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IActionBashSystem _actionBash = default!;
        [Dependency] private readonly IActionDigSystem _actionDig = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Dig, InputCmdHandler.FromDelegate(CommandDig))
                .Bind(ContentKeyFunctions.Bash, InputCmdHandler.FromDelegate(CommandBash))
                .Bind(ContentKeyFunctions.Fire, InputCmdHandler.FromDelegate(CommandFire))
                .Bind(ContentKeyFunctions.Rest, InputCmdHandler.FromDelegate(CommandRest))
                .Register<ActionCommandsSystem>();
        }

        private bool BlockIfWorldMap(EntityUid player)
        {
            if (!TryMap(player, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                _mes.Display(Loc.GetString("Elona.Common.CannotDoInGlobal"));
                return true;
            }
            return false;
        }

        private TurnResult? CommandDig(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            _mes.Display(Loc.GetString("Elona.Dig.Prompt"));

            var dir = _uiMgr.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            return _actionDig.StartMining(session!.Player, dir.Value.Coords);
        }

        private TurnResult? CommandBash(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            _mes.Display(Loc.GetString("Elona.Bash.Prompt"));

            var dir = _uiMgr.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            return _actionBash.DoBash(session!.Player, dir.Value.Coords);
        }

        private TurnResult? CommandFire(IGameSessionManager? session)
        {
            if (BlockIfWorldMap(session!.Player))
                return TurnResult.Aborted;

            if (!_targeting.TryGetTarget(session.Player, out var target))
                return TurnResult.Aborted;

            if (_factions.GetRelationTowards(session.Player, target.Value) >= Relation.Neutral
                && !_targeting.PromptReallyAttack(session.Player, target.Value))
                return TurnResult.Aborted;

            if (!_combat.TryGetRangedWeaponAndAmmo(session.Player, out var pair, out var errorReason))
            {
                _mes.Display(Loc.GetString($"Elona.Combat.RangedAttack.Errors.{errorReason}"), combineDuplicates: true);
                return TurnResult.Aborted;
            }

            return _combat.RangedAttack(session.Player, target.Value, pair.Value.Item1);
        }

        private TurnResult? CommandRest(IGameSessionManager? session)
        {
            _activities.StartActivity(session!.Player, Protos.Activity.Resting);
            return TurnResult.Succeeded;
        }
    }
}