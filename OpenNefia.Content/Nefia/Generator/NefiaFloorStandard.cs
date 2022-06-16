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

namespace OpenNefia.Content.Nefia.Generator
{
    public sealed class NefiaFloorStandard : INefiaFloorType
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap CreateMap(MapId mapId, BaseNefiaGenParams baseParams, Vector2i mapSize)
        {
            var map = _mapManager.CreateMap(mapSize.X, mapSize.Y, mapId);
            map.Clear(Protos.Tile.MapgenDefault);

            var level = _entityManager.EnsureComponent<LevelComponent>(map.MapEntityUid);
            level.Level = baseParams.DangerLevel;

            var mapCharaGen = _entityManager.EnsureComponent<MapCharaGenComponent>(map.MapEntityUid);
            mapCharaGen.MaxCharaCount = baseParams.MaxCharaCount;

            return map;
        }

        public IMap CreateMap(MapId mapId, BaseNefiaGenParams baseParams)
            => CreateMap(mapId, baseParams, baseParams.MapSize);

        private bool TryDigRoom(IMap map, List<Room> rooms, int kind, [NotNullWhen(true)] out Room? room)
        {
            throw new NotImplementedException();
        }

        private void PlaceStairsDown(IMap map, Room upstairsRoom)
        {
            throw new NotImplementedException();
        }

        private void PlaceStairsUp(IMap map, Room upstairsRoom)
        {
            throw new NotImplementedException();
        }

        private (Vector2i, Direction) CalcRoomEntrance(IMap map, Room room)
        {
            var found = false;
            Vector2i pos = Vector2i.Zero;
            Direction direction = Direction.Invalid;

            while (!found)
            {
                found = true;
                direction = DirectionUtility.RandomCardinalDirections().First();

                switch (direction)
                {
                    case Direction.West:
                        pos = new(room.Bounds.Left, room.Bounds.Top + _rand.Next(room.Bounds.Height - 2) + 1);
                        break;
                    case Direction.East:
                        pos = new(room.Bounds.Right - 1, room.Bounds.Top + _rand.Next(room.Bounds.Height - 2) + 1);
                        break;
                    case Direction.North:
                        pos = new(room.Bounds.Left + _rand.Next(room.Bounds.Width - 2) + 1, room.Bounds.Top);
                        break;
                    case Direction.South:
                        pos = new(room.Bounds.Left + _rand.Next(room.Bounds.Width - 2) + 1, room.Bounds.Bottom - 1);
                        break;
                }

                var (dx, dy) = direction.ToIntVec();

                if (dx != 0)
                {
                    if (map.GetTile(pos + (0, -1))?.Tile.GetStrongID() == Protos.Tile.MapgenRoom
                        || map.GetTile(pos + (0, 1))?.Tile.GetStrongID() == Protos.Tile.MapgenRoom)
                    {
                        found = false;
                    }
                }
                if (dy != 0)
                {
                    if (map.GetTile(pos + (-1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenRoom
                        || map.GetTile(pos + (1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenRoom)
                    {
                        found = false;
                    }
                }
            }

            return (pos, direction);
        }

        private bool DigPath(IMap map, Vector2i startPos, Vector2i endPos, bool straight, float hiddenPathChance)
        {
            throw new NotImplementedException();
        }

        private bool TryConnectRooms(IMap map, List<Room> rooms, bool placeDoors, BaseNefiaGenParams baseParams)
        {
            for (int roomIdx = 0; roomIdx < rooms.Count; roomIdx++)
            {
                var success = false;
                var entranceCount = _rand.Next(baseParams.RoomEntranceCount + 1) + 1;

                for (int i = 1; i < entranceCount; i++)
                {
                    var startPos = Vector2i.Zero;
                    var endPos = Vector2i.Zero;

                    for (int j = roomIdx; j <= roomIdx + 1; j++)
                    {
                        var room = rooms[j];
                        var (pos, direction) = CalcRoomEntrance(map, room);
                        var adjacent = pos + direction.ToIntVec();

                        map.SetTile(pos, Protos.Tile.MapgenRoom);
                        map.SetTile(adjacent, Protos.Tile.MapgenTunnel);

                        if (j == roomIdx)
                            startPos = adjacent;
                        else
                            endPos = adjacent;
                    }

                    success = success || DigPath(map, startPos, endPos, true, baseParams.HiddenPathChance);
                    if (success)
                    {
                        break;
                    }
                }

                if (!success)
                    return false;
            }

            return true;
        }

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            IoCManager.InjectDependencies(this);
            var baseParams = data.Get<BaseNefiaGenParams>();
            var map = CreateMap(mapId, baseParams);

            //var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            //if (!TryDigRoom(map, rooms, 1, out var upstairsRoom))
            //{
            //    Logger.ErrorS("nefia.gen.floor", "Could not dig room for upstairs");
            //    return null;
            //}

            //PlaceStairsUp(map, upstairsRoom.Value);

            //if (!TryDigRoom(map, rooms, 1, out var downstairsRoom))
            //{
            //    Logger.ErrorS("nefia.gen.floor", "Could not dig room for downstairs");
            //    return null;
            //}

            //PlaceStairsDown(map, downstairsRoom.Value);

            //for (int i = 0; i < baseParams.RoomCount; i++)
            //{
            //    TryDigRoom(map, rooms, 1, out _);
            //}

            //if (!TryConnectRooms(map, rooms, true, baseParams))
            //{
            //    Logger.ErrorS("nefia.gen.floor", "Could not connect rooms");
            //    return null;
            //}

            return map;
        }
    }
}