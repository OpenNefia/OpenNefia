using OpenNefia.Content.GameObjects.Pickable;
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

namespace OpenNefia.Content.GameObjects
{
    public class ActionCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
        [Dependency] private readonly IMessage _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMapTilesetSystem _tilesets = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.Dig, InputCmdHandler.FromDelegate(CommandDig))
                .Register<VerbCommandsSystem>();
        }

        private TurnResult? CommandDig(IGameSessionManager? session)
        {
            _mes.Display(Loc.GetString("Elona.Dig.Prompt"));

            var dir = _uiMgr.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(session!.Player));
            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Common.ItIsImpossible"));
                return TurnResult.Aborted;
            }

            return DoDig(session!.Player, dir.Value.Coords);
        }

        private TurnResult DoDig(EntityUid uid, MapCoordinates digPos)
        {
            var map = _mapManager.GetMap(digPos.MapId);
            
            // TODO
            if (map.IsInBounds(digPos) && map.GetTile(digPos)!.Value.Tile.ResolvePrototype().IsSolid)
            {
                _audio.Play(Protos.Sound.Crush1, digPos);
                var mapCommon = EntityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
                var tile = _tilesets.GetTile(Protos.Tile.MapgenTunnel, mapCommon.Tileset)!;
                map.SetTile(digPos.Position, tile.Value);
            }

            return TurnResult.Succeeded;
        }
    }
}
