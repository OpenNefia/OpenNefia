using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Maps
{
    internal sealed class MapSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<MapComponent, ComponentAdd>(OnMapAdd, nameof(OnMapAdd));
            SubscribeLocalEvent<MapComponent, ComponentInit>(OnMapInit, nameof(OnMapInit));
            SubscribeLocalEvent<MapComponent, ComponentStartup>(OnMapStartup, nameof(OnMapStartup));
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
}
