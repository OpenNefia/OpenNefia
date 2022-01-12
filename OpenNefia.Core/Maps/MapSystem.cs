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

            SubscribeLocalEvent<MapComponent, ComponentAdd>(OnMapAdd, nameof(OnMapAdd));
            SubscribeLocalEvent<MapComponent, ComponentInit>(OnMapInit, nameof(OnMapInit));
            SubscribeLocalEvent<MapComponent, ComponentStartup>(OnMapStartup, nameof(OnMapStartup));
        }

        private void OnActiveMapChanged(IMap map, IMap? oldMap)
        {
            var ev = new MapEnteredEvent(map, oldMap);
            RaiseLocalEvent(map.MapEntityUid, ev);
        }

        private void OnMapAdd(EntityUid uid, MapComponent component, ComponentAdd args)
        {
            var msg = new MapAddEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseLocalEvent(uid, msg);
        }

        private void OnMapInit(EntityUid uid, MapComponent component, ComponentInit args)
        {
            var msg = new MapInitializeEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseLocalEvent(uid, msg);
        }

        private void OnMapStartup(EntityUid uid, MapComponent component, ComponentStartup args)
        {
            var msg = new MapStartupEvent(uid, component.MapId);
            EntityManager.EventBus.RaiseLocalEvent(uid, msg);
        }
    }

    /// <summary>
    /// Raised whenever a map component starts up.
    /// </summary>
    public sealed class MapStartupEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapStartupEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever a map is being initialized.
    /// </summary>
    public sealed class MapInitializeEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapInitializeEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever a map is added.
    /// </summary>
    public sealed class MapAddEvent : EntityEventArgs
    {
        public EntityUid EntityUid { get; }
        public MapId MapId { get; }

        public MapAddEvent(EntityUid uid, MapId mapId)
        {
            EntityUid = uid;
            MapId = mapId;
        }
    }

    /// <summary>
    /// Raised whenever the current map changes.
    /// </summary>
    public sealed class MapEnteredEvent : EntityEventArgs
    {
        public IMap NewMap { get; }
        public IMap? OldMap { get; }

        public MapEnteredEvent(IMap newMap, IMap? oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }
}
