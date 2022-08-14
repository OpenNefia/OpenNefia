using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using static OpenNefia.Core.Prototypes.EntityPrototype;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;

namespace OpenNefia.Content.Areas
{
    public interface IAreaEntranceSystem : IEntitySystem
    {
        AreaFloorId? GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaDefEntrance = null);

        WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null);
    }

    public sealed class AreaEntranceSystem : EntitySystem, IAreaEntranceSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapEntranceComponent, EntitySteppedOnEvent>(DisplayAreaEntranceMessage);
            SubscribeEntity<GetAreaEntranceMessageEvent>(GetDefaultEntranceMessage, priority: EventPriorities.Highest);
        }

        private void DisplayAreaEntranceMessage(EntityUid uid, WorldMapEntranceComponent component, EntitySteppedOnEvent args)
        {
            var ev = new GetAreaEntranceMessageEvent();
            RaiseEvent(uid, ev);
            if (!string.IsNullOrEmpty(ev.OutMessage))
                _mes.Display(ev.OutMessage);
        }

        private void GetDefaultEntranceMessage(EntityUid uid, GetAreaEntranceMessageEvent args)
        {
            if (!TryComp<WorldMapEntranceComponent>(uid, out var worldMapEntrance))
                return;

            var areaId = worldMapEntrance.Entrance.MapIdSpecifier.GetAreaId();
            if (areaId == null || !TryArea(areaId.Value, out var area)
                || !TryComp<AreaEntranceComponent>(area.AreaEntityUid, out var areaEntrance)
                || areaEntrance.EntranceMessage == null)
                return;

            args.OutMessage = areaEntrance.EntranceMessage;
        }

        public AreaFloorId? GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            if (!Resolve(area.AreaEntityUid, ref areaEntranceComp))
                return floorId;

            return floorId != null ? floorId.Value : areaEntranceComp.StartingFloor;
        }

        public WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            PrototypeId<EntityPrototype>? protoId = null;
            IMapStartLocation? startLocation = null;
            if (Resolve(area.AreaEntityUid, ref areaEntranceComp, logMissing: false))
            {
                protoId = areaEntranceComp.EntranceEntity;
                startLocation = areaEntranceComp.StartLocation;
            }

            var entranceEnt = EntityManager.SpawnEntity(protoId, coords);

            var worldMapEntrance = EntityManager.EnsureComponent<WorldMapEntranceComponent>(entranceEnt);
            worldMapEntrance.Entrance.MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id);
            if (startLocation != null)
                worldMapEntrance.Entrance.StartLocation = startLocation;

            return worldMapEntrance;
        }
    }

    public sealed class GetAreaEntranceMessageEvent : EntityEventArgs
    {
        public string OutMessage { get; set; } = string.Empty;

        public GetAreaEntranceMessageEvent()
        {
        }
    }
}
