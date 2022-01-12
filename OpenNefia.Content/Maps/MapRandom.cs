using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Maps
{
    public interface IMapRandom : IEntitySystem
    {
        /// <summary>
        /// Picks a random tile in the map.
        /// </summary>
        TileRef? PickRandomTile(IMap map);

        /// <summary>
        /// Picks a random tile in the map within the given tile radius.
        /// </summary>
        TileRef? PickRandomTileInRadius(TileRef tile, int radius);

        /// <summary>
        /// Picks a random tile in the map within the given tile radius.
        /// </summary>
        TileRef? PickRandomTileInRadius(MapCoordinates coords, int radius);

        /// <summary>
        ///     Gets tiles in random directions from the given one.
        /// </summary>
        /// <param name="onlyAccessible">If true, only return tiles where <see cref="IMap.CanAccess"/> returns true.</param>
        /// <returns>An enumerable of the adjacent tiles.</returns>
        IEnumerable<TileRef> GetRandomAdjacentTiles(TileRef tile, bool onlyAccessible = false);

        /// <summary>
        ///     Gets tiles in random directions from the given one.
        /// </summary>
        /// <param name="onlyAccessible">If true, only return tiles where <see cref="IMap.CanAccess"/> returns true.</param>
        /// <returns>An enumerable of the adjacent tiles.</returns>
        IEnumerable<TileRef> GetRandomAdjacentTiles(MapCoordinates coords, bool onlyAccessible = false);
    }

    public class MapRandom : EntitySystem, IMapRandom
    {
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        /// <inheritdoc/>
        public TileRef? PickRandomTile(IMap map)
        {
            return map.GetTile(_random.NextVec2iInBounds(map.Bounds));
        }

        public TileRef? PickRandomTileInRadius(TileRef tile, int radius)
        {
            return PickRandomTileInRadius(tile.MapPosition, radius);
        }

        public TileRef? PickRandomTileInRadius(MapCoordinates coords, int radius)
{
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return null;

            return map.GetTile(_random.NextVec2iInRadius(radius).BoundWithin(map.Bounds));
        }

        /// <inheritdoc/>
        public IEnumerable<TileRef> GetRandomAdjacentTiles(TileRef tile, bool onlyAccessible = false)
        {
            return GetRandomAdjacentTiles(tile.MapPosition, onlyAccessible);
        }

        /// <inheritdoc/>
        public IEnumerable<TileRef> GetRandomAdjacentTiles(MapCoordinates coords, bool onlyAccessible = false)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                yield break;

            foreach (var direction in DirectionUtility.RandomDirections())
            {
                var newCoords = coords.Offset(direction);
                var adjacent = map.GetTile(newCoords);

                if (adjacent == null)
                {
                    continue;
                }

                if (onlyAccessible && !map.CanAccess(newCoords))
                {
                    continue;
                }

                yield return adjacent.Value;
            }
        }
    }
}
