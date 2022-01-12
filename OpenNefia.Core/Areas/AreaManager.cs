using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Areas
{
    public delegate void ActiveAreaChangedDelegate(IArea newArea, IArea? oldArea);

    /// <summary>
    /// Manages areas.
    /// </summary>
    public interface IAreaManager
    {
        IReadOnlyDictionary<AreaId, IArea> LoadedAreas { get; }
        IArea? ActiveArea { get; }

        event ActiveAreaChangedDelegate? ActiveAreaChanged;

        bool AreaExists(AreaId areaId);
        IArea CreateArea(PrototypeId<AreaPrototype>? prototypeId = null, GlobalAreaId? globalId = null);
        bool TryGetArea(AreaId areaId, [NotNullWhen(true)] out IArea? area);
        IArea GetArea(AreaId areaId);
        void DeleteArea(AreaId areaId);

        void RegisterAreaFloor(IArea area, AreaFloorId floorId, IMap map);
        void RegisterAreaFloor(IArea area, AreaFloorId floorId, MapId mapId);
        void RegisterAreaFloor(IArea area, AreaFloorId floorId, AreaFloor areaFloor);
        void UnregisterAreaFloor(IArea area, AreaFloorId floorId);

        bool TryGetAreaOfMap(MapId map, [NotNullWhen(true)] out IArea? area);
        bool TryGetAreaAndFloorOfMap(MapId map, [NotNullWhen(true)] out IArea? area, [NotNullWhen(true)] out AreaFloorId floorId);

        // TODO: This is probably going into an IMapGenerator interface later.
        public MapId GenerateMapForFloor(AreaId areaId, AreaFloorId floorId);

        bool GlobalAreaExists(GlobalAreaId globalId);
        IArea GetGlobalArea(GlobalAreaId globalId);
        bool TryGetGlobalArea(GlobalAreaId globalId, [NotNullWhen(true)] out IArea? area);
    }

    internal interface IAreaManagerInternal : IAreaManager
    {
        /// <summary>
        /// All loaded areas. Used for serialization.
        /// </summary>
        new Dictionary<AreaId, IArea> LoadedAreas { get; }

        /// <summary>
        /// The next free area ID to use when generating new areas.
        /// </summary>
        /// <remarks>
        /// This should **only** be set when handling game saving/loading.
        /// </remarks>
        int NextAreaId { get; set; }

        /// <summary>
        /// Registers a area loaded from a save.
        /// </summary>
        /// <remarks>
        /// Do **NOT** use this function to manually register new areas! This bypasses 
        /// the tracking for free area IDs, since the assumption is that the passed area
        /// previously existed in memory at some point but was unloaded.
        /// </remarks>
        /// <param name="area">The area loaded from a save.</param>
        /// <param name="areaId">The area ID this area was registered with at the time of saving.</param>
        /// <param name="areaEntityUid">The area entity UID to associate with this area, also loaded from a save.</param>
        void RegisterArea(IArea area, AreaId areaId, EntityUid areaEntityUid);

        /// <summary>
        /// Clears the active area list.
        /// </summary>
        /// <remarks>
        /// This should **only** be called when handling game saving/loading.
        /// </remarks>
        void FlushAreas();

        /// <summary>
        /// Allocates a new AreaID, incrementing the highest ID counter.
        /// </summary>
        AreaId GenerateAreaId();
    }

    public sealed partial class AreaManager : IAreaManagerInternal
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        private readonly Dictionary<AreaId, IArea> _areas = new();
        private readonly Dictionary<AreaId, EntityUid> _areaEntities = new();
        private readonly Dictionary<MapId, (IArea, AreaFloorId)> _mapsToAreas = new();

        /// <inheritdoc/>
        public event ActiveAreaChangedDelegate? ActiveAreaChanged;

        /// <inheritdoc/>
        IReadOnlyDictionary<AreaId, IArea> IAreaManager.LoadedAreas => _areas;

        /// <inheritdoc/>
        Dictionary<AreaId, IArea> IAreaManagerInternal.LoadedAreas => _areas;

        /// <inheritdoc/>
        public IArea? ActiveArea => null;

        /// <inheritdoc/>
        public int NextAreaId { get; set; } = (int)AreaId.FirstId;

        /// <inheritdoc/>
        public AreaId GenerateAreaId()
        {
            return new(NextAreaId++);
        }

        /// <inheritdoc/>
        public bool AreaExists(AreaId areaId)
        {
            return _areas.ContainsKey(areaId);
        }

        /// <inheritdoc/>
        public void FlushAreas()
        {
            _areas.Clear();
            _areaEntities.Clear();
            _mapsToAreas.Clear();
            NextAreaId = (int)AreaId.FirstId;
        }

        /// <inheritdoc/>
        public IArea CreateArea(PrototypeId<AreaPrototype>? prototypeId = null, GlobalAreaId? globalId = null)
        {
            if (prototypeId != null && !_prototypeManager.HasIndex(prototypeId.Value))
                throw new ArgumentException($"Prototype with ID '{prototypeId}' does not exist.", nameof(prototypeId));
            if (globalId != null && GlobalAreaExists(globalId.Value))
                throw new ArgumentException($"Area with global ID '{globalId}' already exists.", nameof(globalId));

            var actualID = GenerateAreaId();

            var area = new Area();
            _areas.Add(actualID, area);
            area.Id = actualID;
            RebindAreaEntity(actualID, area);

            if (prototypeId != null)
            {
                var proto = _prototypeManager.Index(prototypeId.Value);
                foreach (var (floorId, initialMap) in proto.InitialFloors)
                {
                    RegisterAreaFloor(area, floorId, new AreaFloor(initialMap));
                }
                area.StartingFloor = proto.StartingFloor;
            }

            if (globalId != null)
            {
                area.GlobalId = globalId.Value;
            }

            return area;
        }

        private static void SetAreaAndEntityIds(IArea area, AreaId areaId, EntityUid areaEntityUid)
        {
            var areaInternal = (Area)area;
            areaInternal.Id = areaId;
            areaInternal.AreaEntityUid = areaEntityUid;
        }

        /// <inheritdoc/>
        public void RegisterArea(IArea area, AreaId areaId, EntityUid areaEntityUid)
        {
            if (areaId == AreaId.Nullspace)
            {
                throw new ArgumentException("Can't register null area.", nameof(areaId));
            }
            if (!_entityManager.EntityExists(areaEntityUid))
            {
                throw new ArgumentException($"Area entity {areaEntityUid} doesn't exist.", nameof(areaEntityUid));
            }

            _areas[areaId] = area;
            foreach (var (floorId, floor) in area.ContainedMaps)
            {
                if (floor.MapId != null)
                    _mapsToAreas.Add(floor.MapId.Value, (area, floorId));
            }

            SetAreaEntity(areaId, areaEntityUid);
            SetAreaAndEntityIds(area, areaId, areaEntityUid);
        }

        private EntityUid RebindAreaEntity(AreaId actualID, IArea area)
        {
            var areaComps = _entityManager.EntityQuery<AreaComponent>();

            AreaComponent? result = null;
            foreach (var areaComp in areaComps)
            {
                if (areaComp.AreaId != actualID)
                    continue;

                result = areaComp;
                break;
            }
            
            if (result != null)
            {
                _areaEntities.Add(actualID, result.Owner);
                Logger.DebugS("area", $"Rebinding area {actualID} to entity {result.Owner}");
                return result.Owner;
            }
            else
            {
                // We assume the global map is always loaded.
                var globalMap = _mapManager.GetMap(MapId.Global);
                var globalMapSpatial = _entityManager.GetComponent<SpatialComponent>(globalMap.MapEntityUid);

                var newEnt = _entityManager.CreateEntityUninitialized(null);
                _areaEntities.Add(actualID, newEnt);

                // Make sure the area IDs are set on the area object before area component
                // events are fired.
                SetAreaAndEntityIds(area, actualID, newEnt);

                var areaComp = _entityManager.AddComponent<AreaComponent>(newEnt);
                areaComp.AreaId = actualID;
                
                // Area entities will always live in the global map.
                var areaSpatial = _entityManager.GetComponent<SpatialComponent>(newEnt);
                areaSpatial.AttachParent(globalMapSpatial);

                _entityManager.InitializeComponents(newEnt);
                _entityManager.StartComponents(newEnt);
                Logger.DebugS("area", $"Binding area {actualID} to entity {newEnt}");
                return newEnt;
            }
        }

        /// <inheritdoc/>
        public void SetAreaEntity(AreaId areaId, EntityUid newAreaEntity)
        {
            if (!_areas.TryGetValue(areaId, out var areaInstance))
                throw new InvalidOperationException($"Area {areaId} does not exist.");

            foreach (var kvEntity in _areaEntities)
            {
                if (kvEntity.Value == newAreaEntity)
                {
                    throw new InvalidOperationException(
                        $"Entity {newAreaEntity} is already the root node of area {kvEntity.Key}.");
                }
            }

            // remove existing graph
            if (_areaEntities.TryGetValue(areaId, out var oldEntId))
            {
                //Note: This prevents setting a subgraph as the root, since the subgraph will be deleted
                _entityManager.DeleteEntity(oldEntId);
            }
            else
            {
                _areaEntities.Add(areaId, EntityUid.Invalid);
            }

            // re-use or add area component
            if (!_entityManager.TryGetComponent(newAreaEntity, out AreaComponent? areaComp))
            {
                areaComp = _entityManager.AddComponent<AreaComponent>(newAreaEntity);
            }
            else
            {
                if (areaComp.AreaId != areaId)
                    Logger.WarningS("area",
                        $"Setting area {areaId} root to entity {newAreaEntity}, but entity thinks it is root node of area {areaComp.AreaId}.");
            }

            Logger.DebugS("area", $"Setting area {areaId} entity to {newAreaEntity}");

            // set as new area entity
            areaComp.AreaId = areaId;
            _areaEntities[areaId] = newAreaEntity;
            SetAreaAndEntityIds(areaInstance, areaId, newAreaEntity);
        }

        /// <inheritdoc/>
        public void DeleteArea(AreaId areaID)
        {
            if (areaID == ActiveArea?.Id)
            {
                Logger.WarningS("area", $"Deleting active area {areaID}");
            }

            if (!_areas.ContainsKey(areaID))
            {
                Logger.WarningS("area", $"Attempted to delete nonexistent area '{areaID}'");
                return;
            }

            Logger.InfoS("area", $"Deleting area {areaID}.");

            _areas.Remove(areaID);

            var areaEnt = _areaEntities[areaID];
            _entityManager.DeleteEntity(areaEnt);
            _areaEntities.Remove(areaID);
        }

        /// <inheritdoc/>
        public bool TryGetArea(AreaId areaId, [NotNullWhen(true)] out IArea? area)
        {
            return _areas.TryGetValue(areaId, out area);
        }

        /// <inheritdoc/>
        public IArea GetArea(AreaId areasId)
        {
            return _areas[areasId];
        }
    }
}
