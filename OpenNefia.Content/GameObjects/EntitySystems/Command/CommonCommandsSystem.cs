using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.GameObjects
{
    public class CommonCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrderSystem = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(EngineKeyFunctions.ShowEscapeMenu, InputCmdHandler.FromDelegate(ShowEscapeMenu))
                .Bind(EngineKeyFunctions.QuickSaveGame, InputCmdHandler.FromDelegate(QuickSaveGame))
                .Bind(EngineKeyFunctions.QuickLoadGame, InputCmdHandler.FromDelegate(QuickLoadGame))
                .Register<CommonCommandsSystem>();
        }

        private TurnResult? QuickSaveGame(IGameSessionManager? session)
        {
            var save = _saveGameManager.CurrentSave!;

            _saveGameSerializer.SaveGame(save);

            _sounds.Play(Sound.Write1);
            Mes.Display(Loc.GetString("Elona.UserInterface.Save.QuickSave"));

            return TurnResult.Aborted;
        }

        private TurnResult? QuickLoadGame(IGameSessionManager? session)
        {
            var save = _saveGameManager.CurrentSave!;

            _saveGameSerializer.LoadGame(save);

            return TurnResult.Aborted;
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
