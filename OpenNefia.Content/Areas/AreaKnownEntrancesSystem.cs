using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Areas
{
    public interface IAreaKnownEntrancesSystem : IEntitySystem
    {
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IMap map);
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(MapId mapID);
    }

    public sealed class AreaKnownEntrancesSystem : EntitySystem, IAreaKnownEntrancesSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapCreatedEvent>(HandleMapCreated);
            SubscribeBroadcast<MapLoadedFromSaveEvent>(HandleMapLoadedFromSave);
            SubscribeBroadcast<MapInitializeEvent>(HandleMapInitialize);
            SubscribeComponent<WorldMapEntranceComponent, AreaEntranceCreatedEvent>(WorldMapEntrance_Generated);
            SubscribeComponent<WorldMapEntranceComponent, EntityPositionChangedEvent>(WorldMapEntrance_PositionChanged);
            SubscribeComponent<WorldMapEntranceComponent, EntityTerminatingEvent>(WorldMapEntrance_Terminating);
            SubscribeComponent<MapComponent, EntityTerminatingEvent>(Map_Terminating);
        }

        private void HandleMapLoadedFromSave(MapLoadedFromSaveEvent args)
        {
            UpdateKnownEntrances(args.Map);
        }

        private void HandleMapCreated(MapCreatedEvent args)
        {
            UpdateKnownEntrances(args.Map);
        }

        private void HandleMapInitialize(MapInitializeEvent ev)
        {
            UpdateKnownEntrances(ev.Map);
        }

        private void WorldMapEntrance_Generated(EntityUid uid, WorldMapEntranceComponent component, AreaEntranceCreatedEvent args)
        {
            UpdateKnownEntrance(Spatial(uid), component);
        }

        private void WorldMapEntrance_PositionChanged(EntityUid uid, WorldMapEntranceComponent entrance, ref EntityPositionChangedEvent args)
        {
            UpdateKnownEntrance(Spatial(uid), entrance);
        }

        private void WorldMapEntrance_Terminating(EntityUid uid, WorldMapEntranceComponent component, ref EntityTerminatingEvent args)
        {
            DeleteKnownEntrance(component);
        }

        private void Map_Terminating(EntityUid uid, MapComponent component, ref EntityTerminatingEvent args)
        {
            if (_areaManager.TryGetAreaOfMap(component.MapId, out var area) && TryComp<AreaKnownEntrancesComponent>(area.AreaEntityUid, out var knownComp))
            {
                knownComp.KnownEntrances.Remove(component.MapId);
            }
        }

        private void UpdateKnownEntrances(IMap map)
        {
            // Remove missing entrances.
            if (_areaManager.TryGetAreaOfMap(map.Id, out var area))
            {
                DebugTools.Assert(_mapManager.MapIsLoaded(map.Id), "Map was not loaded!");

                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(area.AreaEntityUid);
                var known = knownComp.KnownEntrances.GetOrInsertNew(map.Id);

                foreach (var existingEntity in known.Keys.ToList())
                {
                    if (!IsAlive(existingEntity))
                        known.Remove(existingEntity);
                }
            }

            // Add new entrances.
            foreach (var (spatial, entrance) in _lookup.EntityQueryInMap<SpatialComponent, WorldMapEntranceComponent>(map).ToList())
            {
                UpdateKnownEntrance(spatial, entrance);
            }
        }

        private void UpdateKnownEntrance(SpatialComponent spatial, WorldMapEntranceComponent entrance)
        {
            var destMapId = entrance.Entrance.MapIdSpecifier.GetMapId();
            var destAreaId = entrance.Entrance.MapIdSpecifier.GetAreaId();

            if (destMapId != null && destAreaId != null)
            {
                var destArea = _areaManager.GetArea(destAreaId.Value);
                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(destArea.AreaEntityUid);
                var known = knownComp.KnownEntrances.GetOrInsertNew(destMapId.Value);

                if (known.TryGetValue(spatial.Owner, out var meta))
                {
                    meta.MapCoordinates = spatial.MapPosition;
                    meta.EntranceEntity = spatial.Owner;
                }
                else
                {
                    known.Add(spatial.Owner, new AreaEntranceMetadata(spatial.MapPosition, spatial.Owner));
                }
            }
        }

        private void DeleteKnownEntrance(WorldMapEntranceComponent entrance)
        {
            var destMapId = entrance.Entrance.MapIdSpecifier.GetMapId();
            var destAreaId = entrance.Entrance.MapIdSpecifier.GetAreaId();

            if (destMapId != null && destAreaId != null)
            {
                var destArea = _areaManager.GetArea(destAreaId.Value);
                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(destArea.AreaEntityUid);
                if (knownComp.KnownEntrances.TryGetValue(destMapId.Value, out var known))
                    known.Remove(entrance.Owner);
            }
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IMap map)
            => EnumerateKnownEntrancesTo(map.Id);

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(MapId mapID)
        {
            if (!_areaManager.TryGetAreaOfMap(mapID, out var area) 
                || !TryComp<AreaKnownEntrancesComponent>(area.AreaEntityUid, out var knownComp)
                || !knownComp.KnownEntrances.TryGetValue(mapID, out var entrances))
                return Enumerable.Empty<AreaEntranceMetadata>();

            return entrances.Values;
        }
    }
}