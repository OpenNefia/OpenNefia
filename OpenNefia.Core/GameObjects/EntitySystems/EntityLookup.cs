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
        /// <param name="includeMapEntity">If true, include the map entity as the last in the enumerable.</param>
        IEnumerable<Entity> GetAllEntitiesIn(MapId mapId, bool includeMapEntity = false);

        /// <summary>
        /// Returns all entities that are direct children of the map,
        /// excluding entities in containers.
        /// </summary>
        /// <param name="mapId">The map to query in.</param>
        /// <param name="includeMapEntity">If true, include the map entity as the last in the enumerable.</param>
        IEnumerable<Entity> GetEntitiesDirectlyIn(MapId mapId, bool includeMapEntity = false);

        /// <summary>
        /// Returns all entities that are direct children of the entity.
        /// </summary>
        /// <param name="entity">Entity to query in.</param>
        /// <param name="includeParent">If true, include the passed entity as the last in the enumerable.</param>
        IEnumerable<Entity> GetEntitiesDirectlyIn(Entity entity, bool includeParent = false);

        /// <summary>
        /// Gets the live entities at the position in the map, excluding
        /// entities in containers.
        /// </summary>
        /// <param name="coords">The coordinates to query at.</param>
        /// <param name="includeMapEntity">If true, include the map entity as the last in the enumerable.</param>
        IEnumerable<Entity> GetLiveEntitiesAtCoords(MapCoordinates coords, bool includeMapEntity = false);

        /// <summary>
        /// Gets the live entities at the position. This is used in case
        /// the entity is in a container or similar.
        /// </summary>
        /// <param name="coords">The coordinates to query at.</param>
        /// <param name="includeParent">If true, include the parent entity as the last in the enumerable.</param>
        IEnumerable<Entity> GetLiveEntitiesAtCoords(EntityCoordinates coords, bool includeParent = false);

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
        Entity? GetBlockingEntity(MapCoordinates coords);

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

        IEnumerable<Entity> EntitiesUnderneath(EntityUid player, bool includeMapEntity = false, SpatialComponent? spatial = null);
    }

    public class EntityLookup : EntitySystem, IEntityLookup
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MapComponent, MapInitializeEvent>(HandleMapInitialize, nameof(HandleMapInitialize));
        }

        private void HandleMapInitialize(EntityUid uid, MapComponent component, MapInitializeEvent args)
        {
            var lookup = EntityManager.EnsureComponent<MapEntityLookupComponent>(uid);
            var map = _mapManager.GetMap(component.MapId);
            lookup.InitializeFromMap(map);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetAllEntitiesIn(MapId mapId, bool includeMapEntity = false)
        {
            if (!_mapManager.TryGetMapEntity(mapId, out var mapEntity))
                return Enumerable.Empty<Entity>();

            var ents = EntityManager.GetEntities()
                .Where(ent => ent.Spatial.MapID == mapId && ent.Uid != mapEntity.Uid);

            if (includeMapEntity)
            {
                ents = ents.Append(mapEntity);
            }

            return ents;
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetEntitiesDirectlyIn(Entity entity, bool includeParent = false)
        {
            var ents = entity.Spatial.Children.Select(spatial => spatial.Owner);

            if (includeParent)
            {
                ents = ents.Append(entity);
            }

            return ents;
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetEntitiesDirectlyIn(MapId mapId, bool includeParent = false)
        {
            if (!_mapManager.TryGetMapEntity(mapId, out var mapEntity))
                return Enumerable.Empty<Entity>();

            return GetEntitiesDirectlyIn(mapEntity, includeParent);
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntitiesAtCoords(MapCoordinates coords, bool includeMapEntity = false)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return Enumerable.Empty<Entity>();

            if (!map.IsInBounds(coords))
                return Enumerable.Empty<Entity>();

            var mapLookupComp = EntityManager.EnsureComponent<MapEntityLookupComponent>(map.MapEntityUid);

            var ents = mapLookupComp.EntitySpatial[coords.X, coords.Y]
                .Where(uid => EntityManager.IsAlive(uid));
        
            if (includeMapEntity)
            {
                ents = ents.Append(map.MapEntityUid);
            }

            return ents.Select(uid => EntityManager.GetEntity(uid));
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntitiesAtCoords(EntityCoordinates coords, bool includeParent = false)
        {
            return GetLiveEntitiesAtCoords(coords.ToMap(EntityManager), includeParent);
        }

        /// <inheritdoc />
        public Entity? GetBlockingEntity(MapCoordinates coords)
        {
            return GetLiveEntitiesAtCoords(coords)
                .Where(ent => ent.Spatial.IsSolid)
                .FirstOrDefault();
        }
        
        /// <inheritdoc/>
        public IEnumerable<Entity> EntitiesUnderneath(EntityUid player, bool includeMapEntity = false, SpatialComponent? spatial = null)
        {
            if (!Resolve(player, ref spatial))
                return Enumerable.Empty<Entity>();

            return GetLiveEntitiesAtCoords(spatial.MapPosition, includeMapEntity: includeMapEntity)
                .Where(e => e.Uid != player);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EntityIsInMap(MapId mapId, SpatialComponent spatial, bool includeChildren)
        {
            return spatial.MapID == mapId
                && spatial.Parent != null // is this a non-map entity?
                && (includeChildren || spatial.Parent?.Parent == null);
        }

        /// <inheritdoc />
        public IEnumerable<TComp> EntityQueryInMap<TComp>(MapId mapId, bool includeChildren = false)
            where TComp : IComponent
        {
            foreach (var ent in EntityManager.EntityQuery<TComp>())
            {
                if (EntityManager.TryGetComponent(ent.OwnerUid, out SpatialComponent? spatial) 
                    && EntityIsInMap(mapId, spatial, includeChildren))
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
            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2>())
            {
                if (EntityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && EntityIsInMap(mapId, spatial, includeChildren))
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
            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2, TComp3>())
            {
                if (EntityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && EntityIsInMap(mapId, spatial, includeChildren))
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
            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2, TComp3, TComp4>())
            {
                if (EntityManager.TryGetComponent(ent.Item1.OwnerUid, out SpatialComponent? spatial)
                    && EntityIsInMap(mapId, spatial, includeChildren))
                {
                    yield return ent;
                }
            }
        }
    }
}
