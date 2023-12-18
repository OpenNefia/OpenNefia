using OpenNefia.Content.Charas;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Roles;

namespace OpenNefia.Content.Maps
{
    public enum CharaPlaceType
    {
        /// <summary>
        /// Find a random position nearby. Give up after 100 tries.
        /// </summary>
        Npc,

        /// <summary>
        /// Same as <see cref="Npc"/>, but keep looking across every tile in the entire map for an open space.
        /// Give up if not found.
        /// </summary>
        Ally
    }

    public interface IMapPlacement : IEntitySystem
    {
        /// <summary>
        /// Finds a free position to place something on a map.
        /// The tile must have no items on it and must not be blocked by another entity.
        /// </summary>
        /// <param name="desiredPos">Raw desired position.</param>
        /// <param name="allowStacking">If true, ignore entities with a <see cref="StackComponent"/> on the ground when checking for tile openness.</param>
        /// <param name="mapOnly">If true, only check wall/floor status for tile accessibility.</param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        MapCoordinates? FindFreePosition(IMap map, bool allowStacking = false, bool mapOnly = false, bool forceClear = false);

        /// <summary>
        /// Finds a free position to place something on a map.
        /// The tile must not be blocked by another entity.
        /// </summary>
        /// <param name="desiredPos">Raw desired position.</param>
        /// <param name="allowStacking">If true, ignore entities with a <see cref="StackComponent"/> on the ground when checking for tile openness.</param>
        /// <param name="mapOnly">If true, only check wall/floor status for tile accessibility.</param>
        /// <param name="forceClear"></param>
        /// <returns></returns>
        MapCoordinates? FindFreePosition(MapCoordinates desiredPos, bool mapOnly = false, bool forceClear = false);

        /// <summary>
        /// Tries to find an open tile to place a character.
        /// </summary>
        /// <param name="desiredPos">Raw desired position.</param>
        /// <param name="type">How hard to look.</param>
        /// <returns></returns>
        MapCoordinates? FindFreePositionForChara(MapCoordinates desiredPos, CharaPlaceType type = CharaPlaceType.Npc);

        /// <summary>
        /// Clears out a map tile so that it can be accessed.
        /// </summary>
        /// <remarks>
        /// Don't use this recklessly, as it will delete all solid entities on the tile.
        /// </remarks>
        /// <param name="coords"></param>
        /// <param name="deleteNonSolid">Also delete all non-solid entities on the tile.</param>
        void ForceClearPosition(MapCoordinates coords, bool deleteNonSolid = false);

        /// <summary>
        /// Tries to place a character near a position, moving it somewhere close if it isn't available.
        /// If the tile is blocked, the character's state is set accordingly (dead, pet dead, etc.)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coords"></param>
        /// <returns>True if the placement was successful.</returns>
        bool TryPlaceChara(EntityUid entity, MapCoordinates coords);

        /// <summary>
        /// Tries to place a character near a position, moving it somewhere close if it isn't available.
        /// If the tile is blocked, the character's state is set accordingly (dead, pet dead, etc.)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coords"></param>
        /// <param name="actualCoords">Actual coordinates the entity was placed at after adjustment.</param>
        /// <returns>True if the placement was successful.</returns>
        bool TryPlaceChara(EntityUid entity, MapCoordinates coords, out EntityCoordinates actualCoords);
    }

    public sealed class MapPlacementSystem : EntitySystem, IMapPlacement
    {
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMapTilesetSystem _tilesets = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _session = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;

        public MapCoordinates? FindFreePosition(IMap map, bool allowStacking = false, bool mapOnly = false, bool forceClear = false)
        {
            var tries = 0;
            Vector2i resultPos;

            do
            {
                resultPos = (_random.Next(map.Width - 2) + 2, _random.Next(map.Height - 2) + 2);

                bool ok;
                if (mapOnly)
                    ok = !map.GetTile(resultPos)?.Tile.ResolvePrototype().IsSolid ?? false;
                else
                    ok = map.CanAccess(resultPos);

                if (!allowStacking)
                {
                    var items = _lookup.GetLiveEntitiesAtCoords(map.AtPos(resultPos))
                        .Where(ent => EntityManager.HasComponent<StackComponent>(ent.Owner));
                    if (items.Any())
                        ok = false;
                }

                if (ok)
                    return map.AtPos(resultPos);

                tries++;
            }
            while (tries < 100);

            if (forceClear)
            {
                var coords = map.AtPos(resultPos);
                ForceClearPosition(coords);
                return coords;
            }

            return null;
        }

        public MapCoordinates? FindFreePosition(MapCoordinates desiredPos, bool mapOnly = false, bool forceClear = false)
        {
            if (!_mapManager.TryGetMap(desiredPos.MapId, out var map))
                return null;

            var tries = 0;
            Vector2i resultPos;

            do
            {
                if (tries == 0)
                {
                    resultPos = desiredPos.Position;
                }
                else
                {
                    resultPos = desiredPos.Position + (_random.Next(tries + 1) - _random.Next(tries + 1),
                                                       _random.Next(tries + 1) - _random.Next(tries + 1));
                }

                bool ok;
                if (mapOnly)
                    ok = map.IsFloor(resultPos);
                else
                    ok = map.CanAccess(resultPos);

                if (ok)
                    return map.AtPos(resultPos);

                tries++;
            }
            while (tries < 100);

            if (forceClear)
            {
                var coords = map.AtPos(resultPos);
                ForceClearPosition(coords);
                return coords;
            }

            return null;
        }

        public MapCoordinates? FindFreePositionForChara(IMap map, CharaPlaceType type = CharaPlaceType.Npc)
        {
            var pos = _random.NextVec2iInBounds(map.Bounds.Scale(-2));
            return FindFreePositionForChara(map.AtPos(pos), type);
        }

        public MapCoordinates? FindFreePositionForChara(MapCoordinates desiredPos, CharaPlaceType type = CharaPlaceType.Npc)
        {
            if (!_mapManager.TryGetMap(desiredPos.MapId, out var map))
                return null;

            var tries = 0;
            Vector2i resultPos;

            do
            {
                resultPos = desiredPos.Position + (_random.Next(tries + 1) - _random.Next(tries + 1),
                                                   _random.Next(tries + 1) - _random.Next(tries + 1));

                if (map.CanAccess(resultPos))
                    return map.AtPos(resultPos);

                tries++;
            }
            while (tries < 100);

            if (type == CharaPlaceType.Npc)
                return null;

            foreach (var tile in map.AllTiles)
            {
                if (map.CanAccess(tile.Position))
                    return map.AtPos(tile.Position);
            }

            return null;
        }

        public void ForceClearPosition(MapCoordinates coords, bool deleteNonSolid = false)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return;

            if (!map.IsFloor(coords.Position))
            {
                var tileset = _tilesets.GetTileset(map);
                _tilesets.TryGetTile(Protos.Tile.MapgenTunnel, tileset, out var tile);
                map.SetTile(coords.Position, tile!.Value);
            }

            foreach (var spatial in _lookup.GetLiveEntitiesAtCoords(coords).ToList())
            {
                if (spatial.IsSolid || deleteNonSolid)
                    EntityManager.DeleteEntity(spatial.Owner);
            }
        }

        public bool TryPlaceChara(EntityUid entity, MapCoordinates coords)
            => TryPlaceChara(entity, coords, out _);

        public bool TryPlaceChara(EntityUid entity, MapCoordinates coords, out EntityCoordinates realCoordinates)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
            {
                realCoordinates = EntityCoordinates.Invalid;
                return false;
            }

            var type = CharaPlaceType.Npc;
            if (_parties.IsInPlayerParty(entity))
                type = CharaPlaceType.Ally;

            var result = FindFreePositionForChara(coords, type);

            if (result == null && _session.IsPlayer(entity))
            {
                result = map.AtPos(_random.NextVec2iInBounds(map.Bounds));
                ForceClearPosition(result.Value);
            }

            var spatial = Spatial(entity);

            if (result != null)
            {
                Logger.DebugS("map.placement", $"Place {entity} {coords} --> {result}");

                realCoordinates = new EntityCoordinates(map.MapEntityUid, result.Value.Position);
                spatial.Coordinates = realCoordinates;
                return true;
            }

            Logger.WarningS("map.placement", $"Failed to place {entity} near {coords}");
            realCoordinates = new EntityCoordinates(map.MapEntityUid, Vector2i.Zero);
            spatial.Coordinates = realCoordinates;
            FailedToPlaceChara(entity);
            return false;
        }

        private void FailedToPlaceChara(EntityUid entity)
        {
            if (_session.IsPlayer(entity))
                throw new InvalidOperationException("Failed to place player character");

            CharaLivenessState liveness;

            if (_parties.IsInPlayerParty(entity))
            {
                liveness = CharaLivenessState.PetWait;
                _mes.Display(Loc.GetString("Elona.Chara.PlaceFailure.Ally", ("target", entity)));
            }
            else
            {
                liveness = CharaLivenessState.Dead;
                _mes.Display(Loc.GetString("Elona.Chara.PlaceFailure.Other", ("target", entity)));
            }

            if (_charas.IsVillager(entity))
            {
                liveness = CharaLivenessState.VillagerDead;
            }

            if (EntityManager.TryGetComponent<CharaComponent>(entity, out var charaComp))
                charaComp.Liveness = liveness;

            var ev = new CharaPlaceFailureEvent(entity);
            RaiseEvent(entity, ev);
        }
    }

    public sealed class CharaPlaceFailureEvent
    {
        public EntityUid Entity { get; }

        public CharaPlaceFailureEvent(EntityUid entity)
        {
            Entity = entity;
        }
    }
}
