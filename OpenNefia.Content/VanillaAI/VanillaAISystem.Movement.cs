using OpenNefia.Content.Factions;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Book;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.VanillaAI
{
    public sealed partial class VanillaAISystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapTilesetSystem _mapTilesets = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;

        public bool StayNearPosition(EntityUid entity, MapCoordinates anchor, VanillaAIComponent ai,
            int maxDistance = 2,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref spatial))
                return false;

            if (anchor.MapId != spatial.MapID)
                return false;

            if (!_mapManager.TryGetMap(anchor.MapId, out var map))
                return false;

            MapCoordinates newPos;

            if (spatial.MapPosition.TryDistanceTiled(anchor, out var distance) && distance > maxDistance)
            {
                newPos = map.AtPos(DriftTowardsPos(spatial.WorldPosition, anchor.Position));
            }
            else
            {
                var dir = DirectionUtility.RandomDirections().First();
                newPos = spatial.MapPosition.Offset(dir);
            }

            if (map.CanAccess(newPos))
            {
                _movement.MoveEntity(entity, newPos);
                return true;
            }

            return false;
        }

        private bool GoToPresetAnchor(EntityUid entity, VanillaAIComponent ai,
            SpatialComponent? spatial = null,
            AIAnchorComponent? aiAnchor = null)
        {
            if (!Resolve(entity, ref spatial, ref aiAnchor, logMissing: false))
                return false;

            return StayNearPosition(entity, new MapCoordinates(spatial.MapID, aiAnchor.Anchor), ai);
        }

        private void Wander(EntityUid entity, VanillaAIComponent ai,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref spatial))
                return;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return;

            if (ai.CalmAction == VanillaAICalmAction.Roam)
            {
                DoWander(entity, map, ai, spatial);
            }
            else if (ai.CalmAction == VanillaAICalmAction.Dull)
            {
                if (EntityManager.TryGetComponent(entity, out AIAnchorComponent aiAnchor))
                {
                    GoToPresetAnchor(entity, ai, spatial, aiAnchor);
                }
                else
                {
                    DoWander(entity, map, ai, spatial);
                }
            }
        }

        private void DoWander(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            foreach (var tile in _mapRandom.GetRandomAdjacentTiles(spatial.MapPosition))
            {
                if (tile != TileRef.Empty && map.CanAccess(tile.MapPosition))
                {
                    _movement.MoveEntity(entity, tile.MapPosition, spatial: spatial);
                    return;
                }
            }
        }

        private Vector2i DriftTowardsPos(Vector2i pos, Vector2i towards)
        {
            var offset = pos.DirectionTowards(towards).ToIntVec();

            if (_rand.OneIn(2))
                offset.X = 0;
            if (_rand.OneIn(2))
                offset.Y = 0;

            return pos + offset;
        }

        /// <summary>
        /// Tries to move towards the current target.
        /// </summary>
        /// <returns>True if the entity moved towards its target.</returns>
        public bool MoveTowardsTarget(EntityUid entity, VanillaAIComponent ai, SpatialComponent? spatial = null, bool retreat = false)
        {
            if (!EntityManager.IsAlive(ai.CurrentTarget))
                return false;

            if (ai.CurrentTarget == entity)
            {
                SetTarget(entity, GetDefaultTarget(entity), 0, ai);
                return true;
            }

            var target = ai.CurrentTarget!.Value;

            if (!Resolve(entity, ref spatial))
                return false;

            if (!EntityManager.TryGetComponent(target, out SpatialComponent targetSpatial))
                return false;

            if (spatial.MapID != targetSpatial.MapID)
                return false;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return false;

            if (ai.TurnsUntilMovement <= 0)
            {
                ai.DestinationCoords = targetSpatial.WorldPosition;

                if (spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                {
                    if (retreat || dist < ai.TargetDistance)
                    {
                        // Move away from the target.
                        var dir = spatial.WorldPosition.DirectionTowards(targetSpatial.WorldPosition).GetOpposite();
                        ai.DestinationCoords = spatial.WorldPosition.Offset(dir);
                    }
                }
            }
            else
            {
                ai.TurnsUntilMovement--;
            }

            var direction = spatial.WorldPosition.DirectionTowards(ai.DestinationCoords);
            var newCoords = spatial.MapPosition.Offset(direction);

            var onCellSpatial = _lookup.GetBlockingEntity(newCoords);
            if (onCellSpatial != null)
            {
                var onCell = onCellSpatial.Owner;

                if (_factions.GetRelationTowards(entity, onCell) <= Relation.Enemy)
                {
                    SetTarget(entity, onCell, ai.Aggro + 4);
                    DoBasicAction(entity, ai);
                    return false;
                }
                else
                {
                    LevelComponent? targetLevel = null;
                    LevelComponent? onCellLevel = null;
                    QualityComponent? onCellQuality = null;
                    VanillaAIComponent? onCellAi = null;

                    if (Resolve(target, ref targetLevel) && Resolve(onCell, ref onCellLevel, ref onCellQuality, ref onCellAi, logMissing: false))
                    {
                        if (onCellQuality.Quality.Buffed > Quality.Good
                            && onCellLevel.Level > targetLevel.Level
                            && onCellAi.CurrentTarget != ai.CurrentTarget)
                        {
                            if (_movement.SwapPlaces(entity, onCell))
                            {
                                _mes.Display(Loc.GetString("Elona.AI.Swap.Displaces", ("chara", entity), ("onCell", onCell)), entity: entity);
                                // TODO activity
                            }
                        }
                    }
                }
            }

            if (!_parties.IsInPlayerParty(entity))
            {
                if (EntityManager.TryGetComponent(entity, out QualityComponent? quality)
                    && quality.Quality.Buffed > Quality.Good
                    && _factions.GetRelationTowards(entity, ai.CurrentTarget!.Value) <= Relation.Hate
                    && map.IsInBounds(newCoords))
                {
                    if (_rand.OneIn(4))
                    {
                        CrushWall(entity, map, newCoords);
                        return true;
                    }
                }
            }

            var result = FindPositionForMovement(entity, map, ai, spatial);

            if (result.Coords != null)
            {
                if (map.CanAccess(result.Coords.Value))
                {
                    _movement.MoveEntity(entity, result.Coords.Value, spatial: spatial);
                    return true;
                }
            }

            if (ai.TurnsUntilMovement > 0)
            {
                Logger.DebugS("ai.vanilla", $"wait until movement {ai.TurnsUntilMovement}");
                var dir = DirectionUtility.RandomDirections().First();
                var randCoords = spatial.MapPosition.Offset(dir);
                if (map.CanAccess(randCoords))
                {
                    _movement.MoveEntity(entity, randCoords, spatial: spatial);
                    return true;
                }
            }
            else
            {
                if (result.BlockedByChara)
                {
                    ai.TurnsUntilMovement = 3;
                }
                else
                {
                    ai.TurnsUntilMovement = 6;
                }

                var dir = _rand.Pick(result.AvailableDirs);
                var offset = dir.ToIntVec() * 6;
                ai.DestinationCoords = spatial.WorldPosition + offset;
            }

            return false;
        }

        private void CrushWall(EntityUid entity, IMap map, MapCoordinates newCoords)
        {
            // >>>>>>>> elona122/shade2/ai.hsp:411 			map(x,y,0)=tile_tunnel ...
            var tileset = _mapTilesets.GetTileset(map);
            var tile = _mapTilesets.GetTile(Protos.Tile.MapgenTunnel, tileset);
            if (tile == null)
                return;

            map.SetTile(newCoords.Position, tile.Value);
            _mapDebris.SpillFragments(newCoords, 2);
            _audio.Play(Protos.Sound.Crush1, newCoords);
            var drawable = new BreakingFragmentsMapDrawable();
            _mapDrawables.Enqueue(drawable, newCoords);
            _mes.Display(Loc.GetString("Elona.AI.CrushesWall", ("entity", entity)), entity: entity);
            // <<<<<<<< elona122/shade2/ai.hsp:415 			goto *turn_end ...
        }

        private MovementResult FindPositionForMovement(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var dir = spatial.WorldPosition.DirectionTowards(ai.DestinationCoords);
            var dirVec = dir.ToIntVec();

            MovementResult result;
            if (dirVec.X >= dirVec.Y)
            {
                result = DirCheckEastWest(entity, map, ai, spatial);
                if (result.Coords != null)
                {
                    return result;
                }
                result = DirCheckNorthSouth(entity, map, ai, spatial);
                if (result.Coords != null)
                {
                    return result;
                }
            }
            else
            {
                result = DirCheckNorthSouth(entity, map, ai, spatial);
                if (result.Coords != null)
                {
                    return result;
                }
                result = DirCheckEastWest(entity, map, ai, spatial);
                if (result.Coords != null)
                {
                    return result;
                }
            }

            return result;
        }

        private readonly Direction[] EastWest = new[] { Direction.East, Direction.West };
        private readonly Direction[] SouthNorth = new[] { Direction.South, Direction.North };

        private MovementResult DirCheckEastWest(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var reverse = false;
            Direction dir = Direction.East;
            var desired = ai.DestinationCoords;
            if (desired.X > spatial.WorldPosition.X)
            {
                if (desired.Y > spatial.WorldPosition.Y)
                {
                    reverse = true;
                }
                dir = Direction.East;
            }
            else if (desired.X < spatial.WorldPosition.X)
            {
                if (desired.Y < spatial.WorldPosition.Y)
                {
                    reverse = true;
                }
                dir = Direction.West;
            }

            var (pos, blocked) = DirCheck(entity, map, dir, reverse, ai, spatial);
            return new MovementResult(pos, blocked, EastWest);
        }

        private MovementResult DirCheckNorthSouth(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var reverse = false;
            Direction dir = Direction.South;
            var desired = ai.DestinationCoords;
            if (desired.Y > spatial.WorldPosition.Y)
            {
                if (desired.X > spatial.WorldPosition.X)
                {
                    reverse = true;
                }
                dir = Direction.South;
            }
            else if (desired.Y < spatial.WorldPosition.Y)
            {
                if (desired.X < spatial.WorldPosition.X)
                {
                    reverse = true;
                }
                dir = Direction.North;
            }

            var (pos, blocked) = DirCheck(entity, map, dir, reverse, ai, spatial);
            return new MovementResult(pos, blocked, SouthNorth);
        }

        private (MapCoordinates?, bool) DirCheck(EntityUid entity, IMap map, Direction dir, bool reverse,
            VanillaAIComponent ai,
            SpatialComponent spatial)
        {
            var pos = Vector2i.Zero;
            var blocked = false;

            int start;
            int finish;
            if (reverse)
            {
                start = 1;
                finish = -1;
            }
            else
            {
                start = -1;
                finish = 1;
            }

            for (int step = start; step != finish; step += finish)
            {
                switch (dir)
                {
                    case Direction.East:
                        pos = spatial.WorldPosition + (1, step);
                        break;
                    case Direction.West:
                        pos = spatial.WorldPosition + (-1, step);
                        break;
                    case Direction.North:
                        pos = spatial.WorldPosition + (step, -1);
                        break;
                    case Direction.South:
                        pos = spatial.WorldPosition + (step, 1);
                        break;
                }

                var coords = map.AtPos(pos);

                if (map.CanAccess(coords))
                {
                    return (coords, blocked);
                }

                var onCellSpatial = _lookup.GetBlockingEntity(coords);

                if (onCellSpatial != null)
                {
                    if (_factions.GetRelationTowards(entity, onCellSpatial.Owner) <= Relation.Enemy)
                    {
                        ai.CurrentTarget = onCellSpatial.Owner;
                    }
                    else
                    {
                        blocked = true;
                    }
                }

                if (CanAccessInDirCheck(map, pos, onCellSpatial?.Owner, ai))
                {
                    return (map.AtPos(pos), blocked);
                }
            }

            return (null, blocked);
        }

        private bool CanAccessInDirCheck(IMap map, Vector2i pos, EntityUid? onCell, VanillaAIComponent ai)
        {
            // If the tile is solid, it's a no-go.
            var tile = map.GetTile(pos);
            if (tile == null || tile.Value.Tile.ResolvePrototype().IsSolid)
                return false;

            // If the thing on the cell is the target, then move towards it.
            if (onCell != null && ai.CurrentTarget == onCell)
                return true;

            // Count things like doors as passable even though they're solid.
            if (onCell != null && EntityManager.HasComponent<AICanPassThroughComponent>(onCell.Value))
                return true;

            return false;
        }

        private struct MovementResult
        {
            public readonly MapCoordinates? Coords;
            public readonly bool BlockedByChara;
            public readonly Direction[] AvailableDirs;

            public MovementResult(MapCoordinates? coords, bool blockedByChara, Direction[] availableDirs)
            {
                Coords = coords;
                BlockedByChara = blockedByChara;
                AvailableDirs = availableDirs;
            }
        }
    }
}
