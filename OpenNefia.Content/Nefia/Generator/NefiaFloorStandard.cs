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

        /// <summary>
        /// Maximum room count per nefia.
        /// </summary>
        private const int MAX_ROOMS = 30;

        public enum RoomType
        {
            /// <summary>
            /// Generates rooms anywhere.
            /// </summary>
            Anywhere,

            /// <summary>
            /// Generates rooms anywhere, away from the edges of the map.
            /// </summary>
            NonEdge,

            /// <summary>
            /// Generates rooms on the edges of the map.
            /// </summary>
            Edge,

            /// <summary>
            /// Generates small 3x3 rooms.
            /// </summary>
            Small,

            /// <summary>
            /// Generates rooms at least 3 tiles away from the edges of the map.
            /// </summary>
            Inner
        }

        public enum WallType
        {
            None,
            Wall,
            Floor,
            Room
        }

        public enum DoorType
        {
            None,
            Room,
            Random,
            RandomNoDoor
        }

        public record RoomTemplate(RoomType roomType, WallType wallType, DoorType doorType);

        public static readonly IReadOnlyDictionary<RoomType, RoomTemplate> RoomTemplates = new Dictionary<RoomType, RoomTemplate>()
        {
            { RoomType.Anywhere, new(RoomType.Anywhere, WallType.None, DoorType.None) },
            { RoomType.NonEdge, new(RoomType.NonEdge, WallType.Wall, DoorType.None) },
            { RoomType.Edge, new(RoomType.Edge, WallType.Wall, DoorType.Room) },
            { RoomType.Small, new(RoomType.Small, WallType.Floor, DoorType.RandomNoDoor) },
            { RoomType.Inner, new(RoomType.Inner, WallType.Room, DoorType.None) },
        };

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

        public Room? CalcRoomSize(RoomType roomType, int minSize, int maxSize, Vector2i mapSize)
        {
            var x = 0;
            var y = 0;
            var w = 0;
            var h = 0;
            var dir = Direction.Invalid;

            switch (roomType)
            {
                case RoomType.Anywhere:
                default:
                    w = _rand.Next(maxSize) + minSize;
                    h = _rand.Next(maxSize) + minSize;
                    x = _rand.Next(mapSize.X) + 2;
                    y = _rand.Next(mapSize.Y) + 2;
                    break;
                case RoomType.NonEdge:
                    w = ((_rand.Next(maxSize) + minSize) / 3 * 3) + 5;
                    h = ((_rand.Next(maxSize) + minSize) / 3 * 3) + 5;
                    x = (_rand.Next(mapSize.X) / 3 * 3) + 2;
                    y = (_rand.Next(mapSize.Y) / 3 * 3) + 2;
                    break;
                case RoomType.Edge:
                    dir = DirectionUtility.RandomCardinalDirections().First();
                    if (dir == Direction.North || dir == Direction.South)
                    {
                        x = _rand.Next(mapSize.X - minSize * 3 / 2 - 2) + minSize / 2;
                        w = _rand.Next(minSize) + minSize / 2 + 3;
                        if (dir == Direction.North)
                            y = 0;
                        else
                            y = mapSize.Y - h;
                    }
                    else
                    {
                        y = _rand.Next(mapSize.Y - minSize * 3 / 2 - 2) + minSize / 2;
                        h = _rand.Next(minSize) + minSize / 2 + 3;
                        if (dir == Direction.West)
                            x = 0;
                        else
                            x = mapSize.X - w;
                    }
                    break;
                case RoomType.Small:
                    w = h = 3;
                    var xRange = mapSize.X - minSize * 2 - w - 2 + 1;
                    if (xRange < 1)
                        return null;
                    var yRange = mapSize.Y - minSize * 2 - h - 2 + 1;
                    if (yRange < 1)
                        return null;
                    x = minSize + 1 + _rand.Next(xRange);
                    y = minSize + 1 + _rand.Next(yRange);
                    break;
                case RoomType.Inner:
                    w = _rand.Next(maxSize) + minSize;
                    h = _rand.Next(maxSize) + minSize;
                    x = _rand.Next(mapSize.X - maxSize - 8) + 3;
                    y = _rand.Next(mapSize.Y - maxSize - 8) + 3;
                    break;
            }

            return new Room(UIBox2i.FromDimensions(x, y, w, h), dir);
        }

        private Room? CalcValidRoom(IMap map, List<Room> rooms, RoomType roomType, int minSize, int maxSize)
        {
            for (var i = 0; i < 100; i++)
            {
                Room? room;
                var success = false;

                while (true)
                {
                    room = CalcRoomSize(roomType, minSize, maxSize, map.Size);
                    if (room == null)
                        // Calculation failed
                        return null;

                    var bounds = room.Value.Bounds;
                    var bottomRight = bounds.BottomRight - Vector2i.One;

                    // Check if map contains room
                    if (!map.Bounds.IsInBounds(bounds))
                        break;

                    if (roomType == RoomType.NonEdge)
                    {
                        if (bottomRight.X >= map.Size.X + 1 && bottomRight.Y >= map.Size.Y + 1)
                            break;
                    }
                    else if (roomType == RoomType.Small)
                    {
                        if (map.GetTile(map.AtPos(bottomRight))!.Value.Tile.GetStrongID() == Protos.Tile.MapgenRoom)
                            break;
                    }

                    // Check if room intersects other rooms
                    var doContinue = false;
                    foreach (var other in rooms)
                    {
                        var x1 = other.Bounds.Left - 1 - bounds.Left;
                        var y1 = other.Bounds.Top - 1 - bounds.Top;
                        var x2 = -other.Bounds.Width - 2 < x1 && x1 < bounds.Width;
                        var y2 = -other.Bounds.Height - 2 < y1 && y1 < bounds.Height;
                        if (x2 && y2)
                        {
                            doContinue = true;
                            break;
                        }
                    }
                    if (doContinue)
                        break;

                    success = true;
                    break;
                }

                if (success)
                    return room;
            }

            return null;
        }

        private bool TryDigRoom(IMap map, List<Room> rooms, RoomType kind, int minSize, int maxSize, [NotNullWhen(true)] out Room? room)
        {
            var template = RoomTemplates[kind];

            room = CalcValidRoom(map, rooms, template.roomType, minSize, maxSize);

            if (room == null)
                return false;

            return true;
        }

        private bool TryDigRoomIfBelowMax(IMap map, List<Room> rooms, RoomType kind, int minSize, int maxSize, [NotNullWhen(true)] out Room? room)
        {
            if (rooms.Count > MAX_ROOMS)
            {
                room = null;
                return false;
            }

            return TryDigRoom(map, rooms, kind, minSize, maxSize, out room);
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

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            if (!TryDigRoom(map, rooms, 1, out var upstairsRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for upstairs");
                return null;
            }

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

            map.Clear(Protos.Tile.Brick1);

            return map;
        }
    }
}