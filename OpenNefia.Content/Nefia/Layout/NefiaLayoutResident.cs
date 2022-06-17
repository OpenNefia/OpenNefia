using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Nefia.Layout;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Large room or tunnel in the middle, with rooms on outer extremities
    /// </summary>
    public class NefiaLayoutResident : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MinRoomSize = 8;

            var map = _nefiaLayout.CreateMap(mapId, baseParams);
            map.Clear(Protos.Tile.MapgenWall);

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            var n = baseParams.MinRoomSize - 1;

            foreach (var tile in map.AllTiles)
            {
                var pos = tile.Position;
                if (pos.X > n && pos.Y > n && pos.X + 1 < map.Width - n && pos.Y + 1 < map.Height - n)
                    map.SetTile(pos, Protos.Tile.MapgenTunnel);
            }

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Edge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var surfacingRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for upstairs");
                return null;
            }

            _nefiaLayout.PlaceStairsSurfacingInRoom(map, surfacingRoom.Value);

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Edge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var delvingRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for downstairs");
                return null;
            }

            _nefiaLayout.PlaceStairsDelvingInRoom(map, delvingRoom.Value);

            for (int i = 0; i < baseParams.RoomCount; i++)
            {
                _nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Edge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out _);
            }

            if (_rand.OneIn(2))
            {
                for (var i = 0; i < baseParams.RoomCount / 4; i++)
                {
                    _nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Small, baseParams.MinRoomSize, baseParams.MaxRoomSize, out _);
                }
            }
            else
            {
                // fill in the center, creating a ring
                var fillSize = baseParams.MinRoomSize + 1 + _rand.Next(3);
                for (var j = 0; j < map.Height - fillSize * 2; j++)
                {
                    for (var i = 0; i < map.Width - fillSize * 2; i++)
                    {
                        var pos = (fillSize + i, fillSize + j);
                        map.SetTile(pos, Protos.Tile.MapgenWall);
                    }
                }
            }

            return map;
        }
    }
}