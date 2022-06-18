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
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects.Pickable;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Large cavern with walls interspersed throughout.
    /// </summary>
    public class NefiaLayoutPuppyCave : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var klass = 5 + _rand.Next(4);
            var bold = 2;

            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MapSize = (klass * (bold * 2) - bold + 8, baseParams.MapSize.X);

            var map = _nefiaLayout.CreateMap(mapId, baseParams);
            baseParams.MaxCharaCount = map.Width * map.Height / 12;

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            _nefiaLayout.DigMaze(map, rooms, data, klass, bold);
            if (!_nefiaLayout.PlaceStairsInMaze(map))
                return null;

            var tunnels = new int[map.Width, map.Height];

            for (var cnt = 0; cnt < 50; cnt++)
            {
                var tunnel = 100 + cnt + 1;
                var pos = Vector2i.Zero;
                var found = false;

                for (var i = 0; i < 1000; i++)
                {
                    pos = _rand.NextVec2iInBounds(map.Bounds);
                    if (map.GetTile(pos)!.Value.Tile.GetStrongID() == Protos.Tile.MapgenTunnel)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Logger.ErrorS("nefia.gen.floor", "Could not find tunnel");
                    return null;
                }

                void TryDig(Vector2i dpos, int width)
                {
                    if (dpos.X < 1 || dpos.Y < 1 || dpos.X >= map!.Width - 1 || dpos.Y >= map.Height - 1)
                    {
                        return;
                    }

                    if ((pos - dpos).Length >= width / 2)
                    {
                        return;
                    }

                    bool Check(Vector2i tpos)
                    {
                        var tileId = map!.GetTile(tpos)!.Value.Tile.GetStrongID();
                        var tunnelAt = tunnels![tpos.X, tpos.Y];
                        if (tunnelAt != 0)
                        {
                            if (tunnelAt != tunnel && tileId != Protos.Tile.MapgenDefault)
                                return false;
                        }
                        else
                        {
                            if (tileId != Protos.Tile.MapgenTunnel && tileId != Protos.Tile.MapgenDefault)
                                return false;
                        }

                        return true;
                    }

                    if (!Check(dpos + (-1, 0))) return;
                    if (!Check(dpos + (1, 0))) return;
                    if (!Check(dpos + (0, -1))) return;
                    if (!Check(dpos + (0, 1))) return;
                    if (!Check(dpos + (-1, -1))) return;
                    if (!Check(dpos + (1, -1))) return;
                    if (!Check(dpos + (-1, 1))) return;
                    if (!Check(dpos + (1, 1))) return;

                    map.SetTile(dpos, Protos.Tile.MapgenTunnel);
                    tunnels[dpos.X, dpos.Y] = tunnel;
                }

                var width = 10 + _rand.Next(4);
                for (var j = 0; j < width; j++)
                {
                    for (var i = 0; i < width; i++)
                    {
                        var dpos = (i + pos.X - (width / 2), j + pos.Y - (width / 2));
                        TryDig(dpos, width);
                    }
                }
            }

            void TryPlaceDoor(Vector2i pos)
            {
                if (!map!.IsInBounds(pos))
                    return;

                if (map.GetTile(pos)?.Tile.GetStrongID() != Protos.Tile.MapgenTunnel)
                    return;

                if (_lookup.GetLiveEntitiesAtCoords(map.AtPos(pos)).Count() > 0)
                    return;

                if (map.GetTile(pos + (-1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenTunnel
                    && map.GetTile(pos + (1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenTunnel)
                {
                    if (map.GetTile(pos + (0, -1))?.Tile.GetStrongID() == Protos.Tile.MapgenDefault
                        && map.GetTile(pos + (0, 1))?.Tile.GetStrongID() == Protos.Tile.MapgenDefault)
                    {
                        var door = _entityGen.SpawnEntity(Protos.MObj.DoorWooden, map.AtPos(pos));
                        if (door != null && _entityManager.TryGetComponent<DoorComponent>(door.Value, out var doorComp))
                        {
                            doorComp.UnlockDifficulty = _nefiaLayout.CalculateDoorDifficulty(map);
                        }
                    }

                    return;
                }

                if (map.GetTile(pos + (0, -1))?.Tile.GetStrongID() == Protos.Tile.MapgenTunnel
                    && map.GetTile(pos + (0, 1))?.Tile.GetStrongID() == Protos.Tile.MapgenTunnel)
                {
                    if (map.GetTile(pos + (-1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenDefault
                        && map.GetTile(pos + (1, 0))?.Tile.GetStrongID() == Protos.Tile.MapgenDefault)
                    {
                        var door = _entityGen.SpawnEntity(Protos.MObj.DoorWooden, map.AtPos(pos));
                        if (door != null && _entityManager.TryGetComponent<DoorComponent>(door.Value, out var doorComp))
                        {
                            doorComp.UnlockDifficulty = _nefiaLayout.CalculateDoorDifficulty(map);
                        }
                    }

                    return;
                }
            }

            for (var j = 0; j < map.Height / 2 - 2; j++)
            {
                for (var i = 0; i < map.Width / 2 - 2; i++)
                {
                    TryPlaceDoor((i * 2, j * 2));
                }
            }

            return map;
        }
    }
}