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

namespace OpenNefia.Content.Maps
{
    public interface IMapRandom : IEntitySystem
    {
        /// <summary>
        ///     Gets tiles in random directions from the given one.
        /// </summary>
        /// <returns>An enumerable of the adjacent tiles.</returns>
        IEnumerable<TileRef> GetRandomAdjacentTiles(TileRef tile);

        /// <summary>
        ///     Gets tiles in random directions from the given one.
        /// </summary>
        /// <returns>An enumerable of the adjacent tiles.</returns>
        IEnumerable<TileRef> GetRandomAdjacentTiles(MapCoordinates coords);
    }

    public class MapRandom : EntitySystem, IMapRandom
    {
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        /// <inheritdoc/>
        public TileRef? PickRandomTile(IMap map)
        {
            return map.GetTile(_random.NextPoint(map.Bounds));
        }

        /// <inheritdoc/>
        public IEnumerable<TileRef> GetRandomAdjacentTiles(TileRef tile)
        {
            if (!_mapManager.TryGetMap(tile.MapId, out var map))
                return Enumerable.Empty<TileRef>();

            return GetRandomAdjacentTiles(map.AtPos(tile.Position));
        }

        /// <inheritdoc/>
        public IEnumerable<TileRef> GetRandomAdjacentTiles(MapCoordinates coords)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                yield break;

            foreach (var direction in DirectionUtility.RandomDirections())
            {
                var adjacent = map.GetTile(coords.Offset(direction));

                if (adjacent == null)
                {
                    continue;
                }

                yield return adjacent.Value;
            }
        }
    }

}
