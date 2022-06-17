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

namespace OpenNefia.Content.Nefia.Generator
{
    public sealed class NefiaFloorStandard : INefiaFloorType
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

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

        public static readonly IReadOnlySet<PrototypeId<EntityPrototype>> RoomItems = new HashSet<PrototypeId<EntityPrototype>>()
            {
                Protos.Item.Cabinet,
                Protos.Item.NeatBarTable,
                Protos.Item.PachisuroMachine,
                Protos.Item.GreenPlant
            };

        private bool TryDigRoom(IMap map, List<Room> rooms, RoomType kind, int minSize, int maxSize, [NotNullWhen(true)] out Room? room)
        {
            var template = RoomTemplates[kind];

            room = CalcValidRoom(map, rooms, template.roomType, minSize, maxSize);

            if (room == null)
                return false;

            rooms.Add(room.Value);

            var tile1 = 0;
            if (_rand.OneIn(2))
                tile1 = 1 + _rand.Next(2);
            var tile2 = 0;
            if (_rand.OneIn(2))
                tile2 = 1 + _rand.Next(2);

            var bounds = room.Value.Bounds;

            for (int j = 0; j < bounds.Height - 1; j++)
            {
                for (int i = 0; i < bounds.Width - 1; i++)
                {
                    var x = bounds.Left + i;
                    var y = bounds.Top + j;
                    var tile = Protos.Tile.MapgenRoom;
                    if (template.wallType != WallType.None)
                    {
                        if (i == 0 || j == 0 || i == bounds.Width - 1 || j == bounds.Height - 1)
                        {
                            switch (template.wallType)
                            {
                                case WallType.None:
                                default:
                                    break;
                                case WallType.Wall:
                                    tile = Protos.Tile.MapgenWall;
                                    break;
                                case WallType.Room:
                                    tile = Protos.Tile.MapgenRoom;
                                    if (tile1 == 1 && i == 0)
                                    {
                                        tile = Protos.Tile.MapgenWall;
                                    }
                                    if (tile2 == 1 && j == 0)
                                    {
                                        tile = Protos.Tile.MapgenWall;
                                        if (i != 0 && i != bounds.Width - 1)
                                        {
                                            var pos = map.AtPos(x, y + 1);
                                            if (_rand.OneIn(3))
                                            {
                                                var id = _rand.Pick(RoomItems);
                                                _entityGen.SpawnEntity(id, pos);
                                            }
                                            else if (i % 2 == 1)
                                            {
                                                _entityGen.SpawnEntity(Protos.Item.Candle, pos);
                                            }
                                        }
                                    }
                                    if (tile1 == 2 && i == bounds.Width - 1)
                                    {
                                        tile = Protos.Tile.MapgenWall;
                                    }
                                    if (tile2 == 2 && j == bounds.Height - 1)
                                    {
                                        tile = Protos.Tile.MapgenWall;
                                        var pos = map.AtPos(x, y + 1);
                                        if (_rand.OneIn(3))
                                        {
                                            var id = _rand.Pick(RoomItems);
                                            _entityGen.SpawnEntity(id, pos);
                                        }
                                        else if (i % 2 == 1)
                                        {
                                            _entityGen.SpawnEntity(Protos.Item.Candle, pos);
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    map.SetTile(new Vector2i(x, y), tile);
                }
            }

            if (template.doorType == DoorType.Room)
            {
                CreateRoomDoor(map, room.Value, true, null);
            }
            else if (template.doorType == DoorType.Random || template.doorType == DoorType.RandomNoDoor)
            {
                var placeDoor = template.doorType != DoorType.RandomNoDoor;
                foreach (var dir in DirectionUtility.RandomCardinalDirections())
                {
                    CreateRoomDoor(map, room.Value, placeDoor, dir);
                }
            }

            return true;
        }

        private bool CreateRoomDoor(IMap map, Room room, bool placeDoor, Direction? dir)
        {
            if (dir == null)
                dir = room.Alignment;
            if (dir == null)
                dir = Direction.South;

            int posOffset;
            if (dir == Direction.North || dir == Direction.South)
            {
                posOffset = room.Bounds.Width;
            }
            else
            {
                posOffset = room.Bounds.Height;
            }

            var offsets = new Vector2i[2];
            var pos = Vector2i.Zero;

            foreach (var doorPos in DirectionUtility.RandomCardinalDirections())
            {
                switch(doorPos)
                {
                    case Direction.North:
                        pos = (posOffset + room.Bounds.Left + 1, room.Bounds.Bottom - 1);
                        offsets[0] = (0, -1);
                        offsets[1] = (0, 1);
                        break;
                    case Direction.South:
                    default:
                        pos = (posOffset + room.Bounds.Left + 1, room.Bounds.Top);
                        offsets[0] = (0, -1);
                        offsets[1] = (0, 1);
                        break;
                    case Direction.West:
                        pos = (room.Bounds.Right - 1, posOffset + room.Bounds.Top + 1);
                        offsets[0] = (-1, 0);
                        offsets[1] = (1, 0);
                        break;
                    case Direction.East:
                        pos = (room.Bounds.Left, posOffset + room.Bounds.Top + 1);
                        offsets[0] = (-1, 0);
                        offsets[1] = (1, 0);
                        break;
                }
            }

            foreach (var offset in offsets)
            {
                var dpos = pos + offset;
                if (!map.Bounds.IsInBounds(dpos))
                {
                    return false;
                }
                if (map.GetTile(dpos)!.Value.Tile.GetStrongID() == Protos.Tile.MapgenWall)
                {
                    return false;
                }
            }

            map.SetTile(pos, Protos.Tile.MapgenRoom);
            if (placeDoor)
            {
                // TODO
                // var difficulty = CalcDoorDifficulty(map);
                _entityGen.SpawnEntity(Protos.MObj.DoorWooden, map.AtPos(pos));
            }

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

        private EntityUid? PlaceStairsUp(IMap map, Room room)
        {
            var x = _rand.Next(room.Bounds.Width - 2) + room.Bounds.Left + 1;
            var y = _rand.Next(room.Bounds.Height - 2) + room.Bounds.Top + 1;
            return _entityGen.SpawnEntity(Protos.MObj.StairsUp, map.AtPos(x, y));
        }

        private EntityUid? PlaceStairsDown(IMap map, Room room)
        {
            var x = _rand.Next(room.Bounds.Width - 2) + room.Bounds.Left + 1;
            var y = _rand.Next(room.Bounds.Height - 2) + room.Bounds.Top + 1;
            return _entityGen.SpawnEntity(Protos.MObj.StairsDown, map.AtPos(x, y));
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

        private (Direction, Direction) GetNextDigDir(IMap map, Direction curDir, Vector2i start, Vector2i end)
        {
            var dest = Direction.Invalid;

            if (start.X >= end.X - 4 && start.X <= end.X + 4 && start.Y >= end.Y - 4 && start.Y <= end.Y + 4)
            {
                if (start.X < end.X)
                {
                    curDir = Direction.East;
                    if (start.Y > end.Y)
                        dest = Direction.North;
                    else
                        dest = Direction.South;
                }
                if (start.X > end.X)
                {
                    curDir = Direction.West;
                    if (start.Y > end.Y)
                        dest = Direction.North;
                    else
                        dest = Direction.South;
                }
                if (start.Y < end.Y)
                {
                    curDir = Direction.South;
                    if (start.X > end.X)
                        dest = Direction.West;
                    else
                        dest = Direction.East;
                }
                if (start.Y > end.Y)
                {
                    curDir = Direction.North;
                    if (start.X > end.X)
                        dest = Direction.West;
                    else
                        dest = Direction.East;
                }
            }

            if (curDir == Direction.West || curDir == Direction.East)
            {
                if (start.Y > end.Y)
                {
                    curDir = Direction.North;
                }
                else
                {
                    curDir = Direction.South;
                }
                if (start.X > end.X)
                {
                    dest = Direction.West;
                }
                else
                {
                    dest = Direction.East;
                }
            }
            else
            {
                if (start.X > end.X)
                {
                    curDir = Direction.West;
                }
                else
                {
                    curDir = Direction.East;
                }
                if (start.Y > end.Y)
                {
                    dest = Direction.North;
                }
                else
                {
                    dest = Direction.South;
                }
            }

            return (curDir, dest);
        }

        private bool CanDig(IMap map, Vector2i pos)
        {
            if (pos.X < 1 || pos.Y < 1 || pos.X > map.Width - 2 || pos.Y > map.Height - 2)
            {
                return false;
            }

            var tileId = map.GetTile(pos)!.Value.Tile.GetStrongID();
            return tileId == Protos.Tile.MapgenTunnel || tileId == Protos.Tile.MapgenDefault;
        }

        private (Direction, Direction?) GetNextDigDir2(IMap map, Direction curDir, Direction? lastDir, Vector2i start, Vector2i end)
        {
            var swap = (bool do_swap) =>
            {
                if (do_swap)
                {
                    var temp = lastDir;
                    lastDir = curDir;
                    curDir = temp!.Value;
                }
                else
                {
                    curDir = lastDir!.Value;
                    lastDir = null;
                }
            };

            if (lastDir != null)
            {
                if (lastDir.Value == Direction.West && CanDig(map, start + (-1, 0)))
                    swap(start.X == end.X);
                else if (lastDir.Value == Direction.East && CanDig(map, start + (1, 0)))
                    swap(start.X == end.X);
                else if (lastDir.Value == Direction.North && CanDig(map, start + (0, -1)))
                    swap(start.Y == end.Y);
                else if (lastDir.Value == Direction.South && CanDig(map, start + (0, 1)))
                    swap(start.Y == end.Y);
            }

            if (curDir == Direction.West || curDir == Direction.East)
            {
                if (start.X == end.X)
                {
                    if (start.Y > end.Y && CanDig(map, start + (0, -1)))
                    {
                        lastDir = curDir;
                        curDir = Direction.North;
                    }
                    else if (start.Y < end.Y && CanDig(map, start + (0, 1)))
                    {
                        lastDir = curDir;
                        curDir = Direction.South;
                    }
                }
            }
            else
            {
                if (start.Y == end.Y)
                {
                    if (start.X > end.X && CanDig(map, start + (-1, 0)))
                    {
                        lastDir = curDir;
                        curDir = Direction.West;
                    }
                    else if (start.X < end.X && CanDig(map, start + (1, 0)))
                    {
                        lastDir = curDir;
                        curDir = Direction.East;
                    }
                }
            }

            return (curDir, lastDir);
        }

        private bool DigPath(IMap map, Vector2i start, Vector2i end, bool straight, float hiddenPathChance)
        {
            var curDir = Direction.North;
            var nextDir = Direction.Invalid;
            Direction? lastDir = null;
            var success = false;

            if (straight)
            {
                (curDir, nextDir) = GetNextDigDir(map, curDir, start, end);
            }

            for (var i = 0; i < 2000; i++)
            {
                if (start.X == end.X)
                {
                    if (start.Y + 1 == end.Y || start.Y - 1 == end.Y)
                    {
                        success = true;
                        break;
                    }
                }
                if (start.Y == end.Y)
                {
                    if (start.X + 1 == end.X || start.X - 1 == end.X)
                    {
                        success = true;
                        break;
                    }
                }
                if (straight)
                {
                    (curDir, lastDir) = GetNextDigDir2(map, curDir, lastDir, start, end);
                }

                var digPos = start + curDir.ToIntVec();

                if (CanDig(map, digPos))
                {
                    start = digPos;
                    map.SetTile(digPos, Protos.Tile.MapgenTunnel);
                    if (_rand.Prob(hiddenPathChance))
                    {
                        map.SetTile(digPos, Protos.Tile.MapgenFog);
                        _entityGen.SpawnEntity(Protos.MObj.HiddenPath, map.AtPos(digPos));
                    }
                }
                else
                {
                    var found = false;
                    switch (nextDir)
                    {
                        case Direction.West:
                            if (CanDig(map, start + (-1, 0)))
                            {
                                curDir = Direction.West;
                                nextDir = Direction.Invalid;
                                found = true;
                            }
                            break;
                        case Direction.East:
                            if (CanDig(map, start + (1, 0)))
                            {
                                curDir = Direction.East;
                                nextDir = Direction.Invalid;
                                found = true;
                            }
                            break;
                        case Direction.North:
                            if (CanDig(map, start + (0, -1)))
                            {
                                curDir = Direction.North;
                                nextDir = Direction.Invalid;
                                found = true;
                            }
                            break;
                        case Direction.South:
                            if (CanDig(map, start + (0, 1)))
                            {
                                curDir = Direction.South;
                                nextDir = Direction.Invalid;
                                found = true;
                            }
                            break;
                    }

                    if (!found)
                    {
                        (curDir, nextDir) = GetNextDigDir(map, curDir, start, end);
                    }
                }
            }

            return success;
        }

        private bool TryConnectRooms(IMap map, List<Room> rooms, bool placeDoors, BaseNefiaGenParams baseParams)
        {
            for (int roomIdx = 0; roomIdx < rooms.Count - 1; roomIdx++)
            {
                var success = false;
                var entranceCount = _rand.Next(baseParams.RoomEntranceCount + 1);

                for (int i = 0; i < entranceCount; i++)
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

                    success = success || DigPath(map, startPos, endPos, true, baseParams.HiddenPathChance) || true;
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
            EntitySystem.InjectDependencies(this);

            var baseParams = data.Get<BaseNefiaGenParams>();
            var map = CreateMap(mapId, baseParams);

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            if (!TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var upstairsRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for upstairs");
                return null;
            }

            PlaceStairsUp(map, upstairsRoom.Value);

            if (!TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var downstairsRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for downstairs");
                return null;
            }

            PlaceStairsDown(map, downstairsRoom.Value);

            for (int i = 0; i < baseParams.RoomCount; i++)
            {
                TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out _);
            }

            if (!TryConnectRooms(map, rooms, true, baseParams))
            {
                Logger.ErrorS("nefia.gen.floor", $"Could not connect {rooms.Count} rooms");
                return null;
            }

            return map;
        }
    }
}