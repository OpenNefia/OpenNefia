using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dependency = OpenNefia.Core.IoC.DependencyAttribute;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Defines an entity system for querying a list of entities in a map.
    /// </summary>
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

        /// <summary>
        ///     Returns ALL component instances of a specified type in the given map.
        /// </summary>
        /// <typeparam name="TComp">A trait or type of a component to retrieve.</typeparam>
        /// <param name="mapId">Map to query in.</param>
        /// <param name="includeChildren">If true, also query the children of entities in the map.</param>
        /// <returns>All components in the map that have the specified type.</returns>
        IEnumerable<TComp> EntityQueryInMap<TComp>(MapId mapId, bool includeChildren = false)
            where TComp : IComponent;

        /// <summary>
        /// Returns the relevant components from all entities in the map that contain the two required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <param name="mapId">Map to query in.</param>
        /// <param name="includeChildren">If true, also query the children of entities in the map.</param>
        /// <returns>The pairs of components from each entity directly in the map that has the two required components.</returns>
        IEnumerable<(TComp1, TComp2)> EntityQueryInMap<TComp1, TComp2>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent;

        /// <summary>
        /// Returns the relevant components from all entities in the map that contain the three required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <typeparam name="TComp3">Third required component.</typeparam>
        /// <param name="mapId">Map to query in.</param>
        /// <param name="includeChildren">If true, also query the children of entities in the map.</param>
        /// <returns>The pairs of components from each entity in the map that has the three required components.</returns>
        IEnumerable<(TComp1, TComp2, TComp3)> EntityQueryInMap<TComp1, TComp2, TComp3>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent;

        /// <summary>
        /// Returns the relevant components from all entities in the map that contain the four required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <typeparam name="TComp3">Third required component.</typeparam>
        /// <typeparam name="TComp4">Fourth required component.</typeparam>
        /// <param name="mapId">Map to query in.</param>
        /// <param name="includeChildren">If true, also query the children of entities in the map.</param>
        /// <returns>The pairs of components from each entity in the map that has the four required components.</returns>
        IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQueryInMap<TComp1, TComp2, TComp3, TComp4>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent;
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

            return _entityManager.GetEntities().Where(ent => ent.Spatial.MapID == mapId && ent.Uid != mapEntity.Uid)
                .Append(mapEntity);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetEntitiesDirectlyInMap(MapId mapId)
        {
            if (!_mapManager.TryGetMapEntity(mapId, out var mapEntity))
                return Enumerable.Empty<Entity>();

            return mapEntity.Spatial.Children.Select(spatial => spatial.Owner)
                .Append(mapEntity);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntitiesAtPos(MapCoordinates coords)
        {
            return GetEntitiesDirectlyInMap(coords.MapId)
                 .Where(entity => (entity.Spatial.MapPosition == coords 
                                   || entity.Spatial.Parent == null) // is map entity?
                               && entity.MetaData.IsAlive);
        }

        /// <inheritdoc />
        public Entity? GetPrimaryEntity(MapCoordinates coords)
        {
            return GetLiveEntitiesAtPos(coords)
                .Where(ent => ent.MetaData.IsAlive)
                .FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsInMap(MapId mapId, SpatialComponent spatial, bool includeChildren)
        {
            return spatial.MapID == mapId
                && spatial.Parent != null // is this a non-map entity?
                && (includeChildren || spatial.Parent?.Parent == null);
        }

        /// <inheritdoc />
        public IEnumerable<TComp> EntityQueryInMap<TComp>(MapId mapId, bool includeChildren = false)
            where TComp : IComponent
        {
            foreach (var ent in _entityManager.EntityQuery<TComp>())
            {
                if (_entityManager.TryGetComponent(ent.OwnerUid, out SpatialComponent? spatial) 
                    && IsInMap(mapId, spatial, includeChildren))
                {
                    yield return ent;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2)> EntityQueryInMap<TComp1, TComp2>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent
        {
            foreach (var ent in _entityManager.EntityQuery<TComp1, TComp2>())
            {
                if (_entityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && IsInMap(mapId, spatial, includeChildren))
                {
                    yield return ent;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2, TComp3)> EntityQueryInMap<TComp1, TComp2, TComp3>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
        {
            foreach (var ent in _entityManager.EntityQuery<TComp1, TComp2, TComp3>())
            {
                if (_entityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && IsInMap(mapId, spatial, includeChildren))
                {
                    yield return ent;
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQueryInMap<TComp1, TComp2, TComp3, TComp4>(MapId mapId, bool includeChildren = false)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent
        {
            foreach (var ent in _entityManager.EntityQuery<TComp1, TComp2, TComp3, TComp4>())
            {
                if (_entityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && IsInMap(mapId, spatial, includeChildren))
                {
                    yield return ent;
                }
            }
        }
    }
}
