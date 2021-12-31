using OpenNefia.Content.GameObjects.Pickable;
using OpenNefia.Content.Input;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maths;
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
        [Dependency] private readonly IFieldLayer _field = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Ascend,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDAscend)))
                .Bind(ContentKeyFunctions.Descend,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDDescend)))
                .Bind(ContentKeyFunctions.Activate,
                    new VerbInputCmdHandler(new Verb(StairsSystem.VerbIDActivate)))
                .Register<VerbCommandsSystem>();
        }

        private TurnResult? HandleVerb(IGameSessionManager? session, Verb verb)
        {
            var player = session?.Player;
            if (player == null)
                return null;

            foreach (var target in _lookup.EntitiesUnderneath(player.Uid, includeMapEntity: true).ToList())
            {
                if (target.Uid != player.Uid)
                {
                    var verbs = _verbSystem.GetLocalVerbs(player.Uid, target.Uid);
                    if (verbs.Contains(verb))
                    {
                       return _verbSystem.ExecuteVerb(player.Uid, target.Uid, verb);
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
