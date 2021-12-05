using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public interface IEntityLookup : IEntitySystem
    {
        /// <summary>
        /// Gets all entities in this map, including children nested in containers.
        /// </summary>
        IEnumerable<Entity> GetAllEntitiesInMap(MapId mapId);

        /// <summary>
        /// Returns all entities that are direct children of the map,
        /// excluding entities in containers.
        /// </summary>
        IEnumerable<Entity> GetEntitiesDirectlyInMap(MapId mapId);

        /// <summary>
        /// Gets the live entities at the position in the map, excluding
        /// entities in containers.
        /// </summary>
        IEnumerable<Entity> GetLiveEntitiesAtPos(MapCoordinates coords);

        /// <summary>
        /// Gets the primary character on this tile.
        /// 
        /// In Elona, traditionally only one character is allowed on each tile. However, extra features
        /// such as the Riding mechanic or the Tag Teams mechanic added in Elona+ allow multiple characters to
        /// occupy the same tile.
        /// 
        /// This function retrieves the "primary" character used for things like
        /// damage calculation, spell effects, and so on, which should exclude the riding mount, tag team
        /// partner, etc.
        /// 
        /// It's necessary to keep track of the non-primary characters on the same tile because they are 
        /// still affected by things like area of effect magic.
        /// </summary>
        Entity? GetPrimaryEntity(MapCoordinates coords);
    }

    public class EntityLookup : EntitySystem, IEntityLookup
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        /// <inheritdoc />
        public IEnumerable<Entity> GetAllEntitiesInMap(MapId mapId)
        {
            if (!_mapManager.TryGetMapEntity(mapId, out var mapEntity))
                return Enumerable.Empty<Entity>();

            return _entityManager.GetEntities().Where(ent => ent.Spatial.MapID == mapId && ent.Uid != mapEntity.Uid);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetEntitiesDirectlyInMap(MapId mapId)
        {
            if (!_mapManager.TryGetMapEntity(mapId, out var mapEntity))
                return Enumerable.Empty<Entity>();

            return mapEntity.Spatial.Children.Select(spatial => spatial.Owner);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntitiesAtPos(MapCoordinates coords)
        {
            return GetEntitiesDirectlyInMap(coords.MapId)
                 .Where(entity => entity.Spatial.MapPosition == coords
                               && entity.MetaData.IsAlive);
        }

        /// <inheritdoc />
        public Entity? GetPrimaryEntity(MapCoordinates coords)
        {
            return GetLiveEntitiesAtPos(coords)
                .Where(ent => ent.MetaData.IsAlive)
                .FirstOrDefault();
        }
    }
}
