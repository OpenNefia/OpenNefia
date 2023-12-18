using OpenNefia.Content.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;
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
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Ascend,
                    new VerbInputCmdHandler(new VerbRequest(StairsSystem.VerbTypeAscend)))
                .Bind(ContentKeyFunctions.Descend,
                    new VerbInputCmdHandler(new VerbRequest(StairsSystem.VerbTypeDescend)))
                .Bind(ContentKeyFunctions.Activate,
                    new VerbInputCmdHandler(new VerbRequest(StairsSystem.VerbTypeActivate)))
                .Register<VerbCommandsSystem>();
        }

        private TurnResult? HandleVerb(IGameSessionManager? session, VerbRequest verbReq)
        {
            if (session == null)
                return null;

            var player = session.Player;

            foreach (var targetSpatial in _lookup.EntitiesUnderneath(player, includeMapEntity: true).ToList())
            {
                if (targetSpatial.Owner != player)
                {
                    if (_verbSystem.TryGetVerb(player, targetSpatial.Owner, verbReq, out var verb))
                    {
                        var result = verb.Act();
                        if (result != TurnResult.NoResult)
                            return result;
                    }
                }
            }

            return null;
        }

        private sealed class VerbInputCmdHandler : InputCmdHandler
        {
            private readonly VerbRequest _verbReq;

            public VerbInputCmdHandler(VerbRequest verb)
            {
                _verbReq = verb;
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage full)
                {
                    return null;
                }

                if (full.State == BoundKeyState.Down)
                {
                    return Get<VerbCommandsSystem>().HandleVerb(session, _verbReq);
                }
                
                return null;
            }
        }
    }
}
