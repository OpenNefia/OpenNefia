using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Areas
{
    /*
     * Keeps track of entrances into global areas from other maps.
     * 
     * This is used to calculate distances between major towns for suitability when generating delivery quests.
     * In vanilla, every global area had a single map and (X, Y) coordinate the entrance was located at.
     * In OpenNefia, there is no such restriction and there can be any number of entrances into a global area.
     * 
     * I think there is a better way of doing this but I haven't figured it out yet.
     */
    public interface IAreaKnownEntrancesSystem : IEntitySystem
    {
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(MapId mapID);
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IMap map);
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(AreaId areaID);
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IArea area);
        IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(GlobalAreaId globalAreaID);

        bool TryGetClosestEntranceInMap(MapCoordinates parentMapCoords, MapId destMapID, [NotNullWhen(true)] out AreaEntranceMetadata? result);
        bool TryDistanceTiled(MapCoordinates parentMapCoords, MapId destMapID, [NotNullWhen(true)] out int result);
    }

    public sealed class AreaKnownEntrancesSystem : EntitySystem, IAreaKnownEntrancesSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapCreatedEvent>(HandleMapCreated);
            SubscribeBroadcast<MapLoadedFromSaveEvent>(HandleMapLoadedFromSave);
            SubscribeBroadcast<AfterMapEnterEventArgs>(HandleMapEnter, priority: EventPriorities.VeryHigh);
            SubscribeComponent<WorldMapEntranceComponent, AreaEntranceCreatedEvent>(WorldMapEntrance_Generated);
            SubscribeComponent<WorldMapEntranceComponent, EntityPositionChangedEvent>(WorldMapEntrance_PositionChanged);
            SubscribeComponent<WorldMapEntranceComponent, BeforeEntityDeletedEvent>(WorldMapEntrance_BeforeDeleted);
            SubscribeEntity<BeforeMapDeletedEvent>(Map_Deleted);
        }

        private void HandleMapCreated(MapCreatedEvent args)
        {
            UpdateKnownEntrances(args.Map);
        }

        private void HandleMapLoadedFromSave(MapLoadedFromSaveEvent args)
        {
            UpdateKnownEntrances(args.Map);
        }

        private void HandleMapEnter(AfterMapEnterEventArgs ev)
        {
            UpdateKnownEntrances(ev.NewMap);
        }

        private void WorldMapEntrance_Generated(EntityUid uid, WorldMapEntranceComponent component, AreaEntranceCreatedEvent args)
        {
            UpdateKnownEntrance(Spatial(uid), component);
        }

        private void WorldMapEntrance_PositionChanged(EntityUid uid, WorldMapEntranceComponent entrance, ref EntityPositionChangedEvent args)
        {
            UpdateKnownEntrance(Spatial(uid), entrance);
        }

        private void WorldMapEntrance_BeforeDeleted(EntityUid uid, WorldMapEntranceComponent component, ref BeforeEntityDeletedEvent args)
        {
            DeleteKnownEntrance(component);
        }

        private void Map_Deleted(EntityUid uid, BeforeMapDeletedEvent args)
        {
            if (_areaManager.TryGetAreaOfMap(args.Map.Id, out var area) && TryComp<AreaKnownEntrancesComponent>(area.AreaEntityUid, out var knownComp))
            {
                foreach (var (globalAreaId, entrances) in knownComp.KnownEntrances)
                {
                    foreach (var (entranceEntity, metadata) in entrances.ToList())
                    {
                        if (metadata.MapCoordinates.MapId == args.Map.Id)
                        {
                            entrances.Remove(entranceEntity);
                        }
                    }
                }
            }
        }

        private void UpdateKnownEntrances(IMap map)
        {
            // Remove missing entrances.
            if (_areaManager.TryGetAreaOfMap(map.Id, out var area) && area.GlobalId != null)
            {
                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(area.AreaEntityUid);
                var known = knownComp.KnownEntrances.GetOrInsertNew(area.GlobalId.Value);

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

        public bool TryGetClosestEntranceInMap(MapCoordinates parentMapCoords, MapId destMapID, [NotNullWhen(true)] out AreaEntranceMetadata? result)
        {
            if (!TryArea(destMapID, out var area) || area.GlobalId == null)
            {
                result = null;
                return false;
            }

            result = EnumerateKnownEntrancesTo(area.GlobalId.Value)
                .Where(e => e.MapCoordinates.MapId == parentMapCoords.MapId)
                .MinBy(e =>
                {
                    if (!parentMapCoords.TryDistanceFractional(e.MapCoordinates, out var dist))
                        return float.MaxValue;

                    return dist;
                });
            return result != null;
        }

        public bool TryDistanceTiled(MapCoordinates parentMapCoords, MapId destMapID, [NotNullWhen(true)] out int result)
        {
            if (TryGetClosestEntranceInMap(parentMapCoords, destMapID, out var entrance) && entrance.MapCoordinates.TryDistanceTiled(parentMapCoords, out var resultDist))
            {
                result = resultDist;
                return true;
            }

            result = 0;
            return false;
        }

        private bool TryGetGlobalAreaIdOfArea(AreaId areaID, [NotNullWhen(true)] out GlobalAreaId? globalAreaId)
        {
            if (_areaManager.TryGetArea(areaID, out var area) && area.GlobalId != null)
            {
                globalAreaId = area.GlobalId;
                return true;
            }

            globalAreaId = null;
            return false;
        }

        private void UpdateKnownEntrance(SpatialComponent spatial, WorldMapEntranceComponent entrance)
        {
            var destAreaId = entrance.Entrance.MapIdSpecifier.GetOrGenerateAreaId();

            if (destAreaId != null && TryGetGlobalAreaIdOfArea(destAreaId.Value, out var globalAreaId))
            {
                var destArea = _areaManager.GetArea(destAreaId.Value);
                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(destArea.AreaEntityUid);
                var known = knownComp.KnownEntrances.GetOrInsertNew(globalAreaId.Value);

                if (known.TryGetValue(spatial.Owner, out var meta))
                {
                    meta.MapCoordinates = spatial.MapPosition;
                    meta.EntranceEntity = spatial.Owner;
                }
                else
                {
                    known.Add(spatial.Owner, new AreaEntranceMetadata(spatial.MapPosition, spatial.Owner));
                }

                Logger.DebugS("sys.knownEntrances", $"Found area entrance: {globalAreaId} ({destAreaId}) <- {spatial.MapPosition}");
            }
        }

        private void DeleteKnownEntrance(WorldMapEntranceComponent entrance)
        {
            var destAreaId = entrance.Entrance.MapIdSpecifier.GetOrGenerateAreaId();

            if (destAreaId != null && TryGetGlobalAreaIdOfArea(destAreaId.Value, out var globalAreaId))
            {
                var destArea = _areaManager.GetArea(destAreaId.Value);
                var knownComp = EnsureComp<AreaKnownEntrancesComponent>(destArea.AreaEntityUid);
                if (knownComp.KnownEntrances.TryGetValue(globalAreaId.Value, out var known))
                    known.Remove(entrance.Owner);
            }
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(MapId mapId)
        {
            if (!TryArea(mapId, out var map))
                return Enumerable.Empty<AreaEntranceMetadata>();

            return EnumerateKnownEntrancesTo(map);
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IMap map)
        {
            if (!TryArea(map, out var area))
                return Enumerable.Empty<AreaEntranceMetadata>();

            return EnumerateKnownEntrancesTo(area);
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(AreaId areaId)
        {
            if (!TryArea(areaId, out var area))
                return Enumerable.Empty<AreaEntranceMetadata>();

            return EnumerateKnownEntrancesTo(area);
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(IArea area)
        {
            if (area.GlobalId == null)
                return Enumerable.Empty<AreaEntranceMetadata>();

            return EnumerateKnownEntrancesTo(area.GlobalId.Value);
        }

        public IEnumerable<AreaEntranceMetadata> EnumerateKnownEntrancesTo(GlobalAreaId globalAreaID)
        {
            if (!_areaManager.TryGetGlobalArea(globalAreaID, out var area)
                || !TryComp<AreaKnownEntrancesComponent>(area.AreaEntityUid, out var knownComp)
                || !knownComp.KnownEntrances.TryGetValue(globalAreaID, out var entrances))
                return Enumerable.Empty<AreaEntranceMetadata>();

            return entrances.Values;
        }
    }
}