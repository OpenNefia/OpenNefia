using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    public sealed partial class VanillaAISystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        private (SpatialComponent, MoveableComponent, FactionComponent)? GetBlockingEntity(IMap map, Vector2i pos)
        {
            return _lookup.EntityQueryInMap<SpatialComponent, MoveableComponent, FactionComponent>(map.Id)
                .Where(t => t.Item1.WorldPosition == pos)
                .FirstOrDefault();
        }

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

            if (spatial.MapPosition.TryDistance(anchor, out var distance) && distance > maxDistance)
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

        private Vector2i DriftTowardsPos(Vector2i pos, Vector2i initialPos)
        {
            var offset = Vector2i.Zero;

            if (_random.OneIn(2))
            {
                if (pos.X > initialPos.X)
                    offset.X = -1;
                else if (pos.X < initialPos.X)
                    offset.X = 1;
            }

            if (_random.OneIn(2))
            {
                if (pos.Y > initialPos.Y)
                    offset.Y = -1;
                else if (pos.Y < initialPos.Y)
                    offset.Y = 1;
            }

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
                ai.DesiredMovePosition = targetSpatial.WorldPosition;

                if (spatial.MapPosition.TryDistance(targetSpatial.MapPosition, out var dist))
                {
                    if (retreat || dist > ai.TargetDistance)
                    {
                        // Move away from the target.
                        var dir = spatial.WorldPosition.DirectionTowards(targetSpatial.WorldPosition).GetOpposite();
                        ai.DesiredMovePosition = spatial.WorldPosition.Offset(dir);
                    }
                }
            }
            else
            {
                ai.TurnsUntilMovement--;
            }

            if (map.CanAccess(ai.DesiredMovePosition))
            {
                _movement.MoveEntity(entity, map.AtPos(ai.DesiredMovePosition), spatial: spatial);
                return true;
            }

            var onCell = GetBlockingEntity(map, ai.DesiredMovePosition);

            if (onCell != null)
            {
                var (onCellSpatial, onCellMoveable, onCellFaction) = onCell.Value;
                var onCellUid = onCellSpatial.OwnerUid;

                if (_factions.GetRelationTowards(entity, onCellUid) <= Relation.Enemy)
                {
                    SetTarget(entity, onCellUid, ai.Aggro + 4);
                    DoBasicAction(entity, ai);
                    return false;
                }
                else
                {
                    LevelComponent? targetLevel = null;
                    LevelComponent? onCellLevel = null;
                    QualityComponent? onCellQuality = null;
                    VanillaAIComponent? onCellAi = null;

                    if (Resolve(target, ref targetLevel) && Resolve(onCellUid, ref onCellLevel, ref onCellQuality, ref onCellAi))
                    {
                        if (onCellQuality.Quality.Buffed > Quality.Good 
                            && onCellLevel.Level > targetLevel.Level
                            && onCellAi.CurrentTarget != ai.CurrentTarget)
                        {
                            if (_movement.SwapPlaces(entity, onCellUid))
                            {
                                Mes.DisplayIfLos(entity, Loc.GetString("Elona.AI.Swap.Displace", ("chara", entity), ("onCell", onCell)));
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
                    && map.IsInBounds(ai.DesiredMovePosition))
                {
                    if (_random.OneIn(4))
                    {
                        // TODO crush wall
                        return true;
                    }
                }
            }

            var result = FindPositionForMovement(entity, map, ai, spatial);

            if (result.Coords != null)
            {
                _movement.MoveEntity(entity, result.Coords.Value, spatial: spatial);
                return true;
            }

            if (ai.TurnsUntilMovement > 0)
            {
                var dir = DirectionUtility.RandomDirections().First();
                var newPos = spatial.MapPosition.Offset(dir);
                if (map.CanAccess(newPos))
                {
                    _movement.MoveEntity(entity, newPos, spatial: spatial);
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

                var dir = _random.Pick(result.AvailableDirs);
                var offset = dir.ToIntVec() * 6;
                ai.DesiredMovePosition = spatial.WorldPosition + offset;
            }

            return false;
        }

        private MovementResult FindPositionForMovement(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var dir = spatial.WorldPosition.DirectionTowards(ai.DesiredMovePosition);
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

        private MovementResult DirCheckEastWest(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var reverse = false;
            Direction dir = Direction.East;
            var desired = ai.DesiredMovePosition;
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
            return new MovementResult(pos, blocked, new List<Direction> { Direction.East, Direction.West });
        }

        private MovementResult DirCheckNorthSouth(EntityUid entity, IMap map, VanillaAIComponent ai, SpatialComponent spatial)
        {
            var reverse = false;
            Direction dir = Direction.South;
            var desired = ai.DesiredMovePosition;
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
            return new MovementResult(pos, blocked, new List<Direction> { Direction.South, Direction.North });
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

            for (int step = start; step <= finish; step += finish)
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
                if (map.CanAccess(pos))
                {
                    return (map.AtPos(pos), blocked);
                }

                var onCell = GetBlockingEntity(map, pos);
                var onCellSpatial = onCell?.Item1;

                if (onCell != null)
                {
                    if (_factions.GetRelationTowards(entity, onCellSpatial!.OwnerUid) <= Relation.Enemy)
                    {
                        ai.CurrentTarget = onCellSpatial.OwnerUid;
                    }
                    else
                    {
                        blocked = true;
                    }
                }

                if (CanAccessInDirCheck(map, pos, onCellSpatial?.OwnerUid))
                {
                    return (map.AtPos(pos), blocked);
                }
            }

            return (null, blocked);
        }

        private bool CanAccessInDirCheck(IMap map, Vector2i pos, EntityUid? onCell)
        {
            // If the tile is solid, it's a no-go.
            var tile = map.GetTile(pos);
            if (tile == null || tile.Value.Tile.ResolvePrototype().IsSolid)
                return false;

            // Count things like doors as passable even though they're solid.
            if (onCell != null && EntityManager.HasComponent<AICanPassThroughComponent>(onCell.Value))
                return true;

            return false;
        }

        private struct MovementResult
        {
            public readonly MapCoordinates? Coords;
            public readonly bool BlockedByChara;
            public readonly IReadOnlyList<Direction> AvailableDirs;

            public MovementResult(MapCoordinates? coords, bool blockedByChara, IReadOnlyList<Direction> availableDirs)
            {
                Coords = coords;
                BlockedByChara = blockedByChara;
                AvailableDirs = availableDirs;
            }
        }
    }
}
