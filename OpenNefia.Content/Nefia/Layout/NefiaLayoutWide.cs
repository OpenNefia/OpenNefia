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
    public class NefiaLayoutWide : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            var map = _nefiaLayout.CreateMap(mapId, baseParams);

            baseParams.MaxRoomSize = 3;
            baseParams.CanHaveMultipleMonsterHouses = true;

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            var pos = map.Size / 2;
            map.SetTile(pos, Protos.Tile.MapgenRoom);

            var chance = 2;
            var dir = Direction.South;

            for (var i = 0; i < baseParams.TunnelLength; i++)
            {
                if (_rand.OneIn(chance))
                    dir = DirectionUtility.RandomCardinalDirections().First();

                pos += dir.ToIntVec();

                switch (dir)
                {
                    case Direction.East:
                        if (pos.X > map.Width - 2)
                        {
                            dir = Direction.South;
                            pos.X = map.Width - 2;
                        }
                        break;
                    case Direction.West:
                        if (pos.X < 1)
                        {
                            dir = Direction.North;
                            pos.X = 1;
                        }
                        break;
                    case Direction.South:
                        if (pos.Y > map.Height - 2)
                        {
                            dir = Direction.West;
                            pos.Y = map.Height - 2;
                        }
                        break;
                    case Direction.North:
                        if (pos.Y < 1)
                        {
                            dir = Direction.East;
                            pos.Y = 1;
                        }
                        break;
                }

                map.SetTile(pos, Protos.Tile.MapgenRoom);
            }

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Anywhere, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var surfacingRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for surfacing");
                return null;
            }

            _nefiaLayout.PlaceStairsSurfacingInRoom(map, surfacingRoom.Value);

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Anywhere, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var delvingRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for delving");
                return null;
            }

            _nefiaLayout.PlaceStairsDelvingInRoom(map, delvingRoom.Value);

            for (var i = 0; i < baseParams.RoomCount; i++)
            {
                _nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.Anywhere, baseParams.MinRoomSize, baseParams.MaxRoomSize, out _);
            }

            for (var i = 0; i < baseParams.ExtraRoomCount; i++)
            {
                var found = false;
                var roomSize = Vector2i.Zero;
                var roomPos = Vector2i.Zero;

                for (var j = 0; j < 100; j++)
                {
                    roomPos = _rand.NextVec2iInBounds(map.Bounds);
                    if (map.GetTile(roomPos)!.Value.Tile.GetStrongID() == Protos.Tile.MapgenRoom)
                    {
                        var dpos = new Vector2i(_rand.Next(baseParams.MaxRoomSize) + baseParams.MinRoomSize,
                                                _rand.Next(baseParams.MaxRoomSize) + baseParams.MinRoomSize);
                        roomSize = _rand.NextVec2iInVec(dpos);
                        if (roomSize.X > 1 && roomSize.Y > 1 && roomSize.X + dpos.X < map.Width - 2 && roomSize.Y + dpos.Y < map.Height - 2)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (found)
                {
                    var dy = roomPos.Y;
                    for (var ry = 0; ry < roomSize.Y; ry++)
                    {
                        var dx = roomPos.X;
                        for (var rx = 0; rx < roomSize.X; rx++)
                        {
                            map.SetTile(new Vector2i(dx, dy), Protos.Tile.MapgenRoom);
                            dx++;
                        }
                        dy++;
                    }
                }
            }

            return map;
        }
    }
}