using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class CommonCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrderSystem = default!;
        [Dependency] private readonly IFieldLayer _field = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(EngineKeyFunctions.ShowEscapeMenu, InputCmdHandler.FromDelegate(ShowEscapeMenu))
                .Register<CommonCommandsSystem>();
        }

        private TurnResult? ShowEscapeMenu(IGameSessionManager? session)
        {
            if (!_turnOrderSystem.IsInGame())
                return null;

            if (_playerQuery.YesOrNo(Loc.GetString("Elona.UserInterface.Exit.Prompt.Text")))
                _field.Cancel();

            return TurnResult.Aborted;
        }
    }
}
