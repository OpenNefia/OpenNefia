using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// Concerns verbs that can have effects on the simulation, but don't
    /// operate on the player's inventory.
    /// </summary>
    public class VerbCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
        [Dependency] private readonly IMessage _mes = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Ascend,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDAscend)))
                .Bind(ContentKeyFunctions.Descend,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDDescend)))
                .Bind(ContentKeyFunctions.Activate,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDActivate)))
                .Bind(ContentKeyFunctions.Close, InputCmdHandler.FromDelegate(CommandClose))
                .Register<VerbCommandsSystem>();
        }

        private TurnResult? CommandClose(IGameSessionManager? session)
        {
            _mes.Display(Loc.GetString("Elona.Door.QueryClose"));

            var dir = _uiMgr.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            var verb = new Verb(DoorSystem.VerbIDClose);

            var targets = _lookup.GetLiveEntitiesAtCoords(dir.Value.Coords)
                .Where(spatial => _verbSystem.GetLocalVerbs(session.Player, spatial.Owner).Contains(verb));

            if (!targets.Any())
            {
                _mes.Display(Loc.GetString("Elona.Door.Close.NothingToClose"));
                return TurnResult.Aborted;
            }

            var targetSpatial = targets.First()!;
            return _verbSystem.ExecuteVerb(session.Player, targetSpatial.Owner, verb);
        }

        private TurnResult? HandleVerb(IGameSessionManager? session, Verb verb)
        {
            if (session == null)
                return null;

            var player = session.Player;

            foreach (var targetSpatial in _lookup.EntitiesUnderneath(player, includeMapEntity: true).ToList())
            {
                if (targetSpatial.Owner != player)
                {
                    var verbs = _verbSystem.GetLocalVerbs(player, targetSpatial.Owner);
                    if (verbs.Contains(verb))
                    {
                       return _verbSystem.ExecuteVerb(player, targetSpatial.Owner, verb);
                    }
                }
            }

            return null;
        }

        private sealed class VerbInputCmdHandler : InputCmdHandler
        {
            private readonly Verb _verb;

            public VerbInputCmdHandler(Verb verb)
            {
                _verb = verb;
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage full)
                {
                    return null;
                }

                if (full.State == BoundKeyState.Down)
                {
                    return Get<VerbCommandsSystem>().HandleVerb(session, _verb);
                }
                return null;
            }
        }
    }
}
