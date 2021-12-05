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
        IEnumerable<Entity> GetAllEntitiesInMap(MapId mapId);

        IEnumerable<Entity> GetLiveEntitiesAtPos(MapCoordinates coords);
    }

    public class EntityLookup : EntitySystem, IEntityLookup
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public IEnumerable<Entity> GetAllEntitiesInMap(MapId mapId)
        {
            if (!_mapManager.MapExists(mapId))
                return Enumerable.Empty<Entity>();

            return _entityManager.GetEntities().Where(e => e.Spatial.Coords.Map?.Id == mapId);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntitiesAtPos(MapCoordinates coords)
        {
            if (coords.Map == null)
                return Enumerable.Empty<Entity>();

            return GetAllEntitiesInMap(coords.Map.Id)
                 .Where(entity => entity.Spatial.Coords == coords
                               && entity.MetaData.IsAlive);
        }
    }
}
