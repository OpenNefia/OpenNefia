using Love;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
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

namespace OpenNefia.Content.GameObjects
{
    public interface IActionDigSystem : IEntitySystem
    {
        TurnResult DoDig(EntityUid player, MapCoordinates digPos);
    }

    public sealed class ActionDigSystem : EntitySystem, IActionDigSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _tilesets = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public override void Initialize()
        {
        }

        public TurnResult DoDig(EntityUid player, MapCoordinates digPos)
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