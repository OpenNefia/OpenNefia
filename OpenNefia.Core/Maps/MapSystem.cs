using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Maps
{
    internal sealed class MapSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            base.Initialize();

            _mapManager.OnActiveMapChanged += OnActiveMapChanged;

            SubscribeComponent<MapComponent, ComponentAdd>(OnMapAdd);
            SubscribeComponent<MapComponent, ComponentInit>(OnMapInit);
            SubscribeComponent<MapComponent, ComponentStartup>(OnMapStartup);
        }

        private void OnActiveMapChanged(IMap map, IMap? oldMap, MapLoadType loadType)
        {
            var ev = new ActiveMapChangedEvent(map, oldMap, loadType);
            RaiseLocalEvent(map.MapEntityUid, ev);
        }

        private void OnMapAdd(EntityUid uid, MapComponent component, ComponentAdd args)
        {
            var msg = new MapComponentAddEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }

        private void OnMapInit(EntityUid uid, MapComponent component, ComponentInit args)
        {
            var msg = new MapComponentInitializeEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }

        private void OnMapStartup(EntityUid uid, MapComponent component, ComponentStartup args)
        {
            var msg = new MapComponentStartupEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseEvent(uid, msg);
        }
    }

    /// <summary>
    /// Raised whenever a map component starts up.
    /// </summary>
    public sealed class MapComponentStartupEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapComponentStartupEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever a map is being initialized.
    /// </summary>
    public sealed class MapComponentInitializeEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapComponentInitializeEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever a map is added.
    /// </summary>
    public sealed class MapComponentAddEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapComponentAddEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever the current map changes.
    /// </summary>
    public sealed class ActiveMapChangedEvent : EntityEventArgs
    {
        public IMap NewMap { get; }
        public IMap? OldMap { get; }
        public MapLoadType LoadType { get; }

        public ActiveMapChangedEvent(IMap newMap, IMap? oldMap = null, MapLoadType loadType = MapLoadType.Full)
        {
            NewMap = newMap;
            OldMap = oldMap;
            LoadType = loadType;
        }
    }
}
