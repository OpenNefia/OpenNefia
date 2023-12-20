using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Game;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Areas
{
    public interface IAreaEntranceSystem : IEntitySystem
    {
        AreaFloorId GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaDefEntrance = null);

        WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null);
    }

    public sealed class AreaEntranceSystem : EntitySystem, IAreaEntranceSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapEntranceComponent, AfterEntitySteppedOnEvent>(DisplayAreaEntranceMessage);
            SubscribeEntity<GetAreaEntranceMessageEvent>(GetDefaultEntranceMessage, priority: EventPriorities.Highest);
        }

        private void DisplayAreaEntranceMessage(EntityUid uid, WorldMapEntranceComponent component, AfterEntitySteppedOnEvent args)
        {
            if (!_gameSession.IsPlayer(args.Stepper))
                return;

            var areaId = component.Entrance.MapIdSpecifier.GetOrGenerateAreaId();
            if (areaId == null || !TryArea(areaId.Value, out var area))
                return;

            var ev = new GetAreaEntranceMessageEvent();
            RaiseEvent(area.AreaEntityUid, ev);
            if (!string.IsNullOrEmpty(ev.OutMessage))
                _mes.Display(ev.OutMessage);
        }

        private void GetDefaultEntranceMessage(EntityUid uid, GetAreaEntranceMessageEvent args)
        {
            if (!TryComp<AreaEntranceComponent>(uid, out var areaEntrance)
                || areaEntrance.EntranceMessage == null)
                return;

            args.OutMessage = areaEntrance.EntranceMessage;
        }

        public AreaFloorId GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            if (!Resolve(area.AreaEntityUid, ref areaEntranceComp))
                return AreaFloorId.Default;

            return floorId != null ? floorId.Value : areaEntranceComp.StartingFloor;
        }

        public WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            PrototypeId<EntityPrototype> protoId = Protos.MObj.MapEntrance;
            IMapStartLocation? startLocation = null;
            if (Resolve(area.AreaEntityUid, ref areaEntranceComp, logMissing: false))
            {
                protoId = areaEntranceComp.EntranceEntity;
                startLocation = areaEntranceComp.StartLocation;
            }

            var entranceEnt = _entityGen.SpawnEntity(protoId, coords)!;

            var worldMapEntrance = EntityManager.EnsureComponent<WorldMapEntranceComponent>(entranceEnt.Value);
            worldMapEntrance.Entrance.MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id);
            if (startLocation != null)
                worldMapEntrance.Entrance.StartLocation = startLocation;

            if (areaEntranceComp != null && areaEntranceComp.ChipID != null && TryComp<ChipComponent>(worldMapEntrance.Owner, out var chip))
                chip.ChipID = areaEntranceComp.ChipID.Value;

            var ev = new AreaEntranceCreatedEvent(area, coords);
            RaiseEvent(entranceEnt.Value, ev);

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

    public sealed class AreaEntranceCreatedEvent : EntityEventArgs
    {
        public IArea Area { get; }
        public MapCoordinates MapCoordinates { get; }

        public AreaEntranceCreatedEvent(IArea area, MapCoordinates mapCoordinates)
        {
            Area = area;
            MapCoordinates = mapCoordinates;
        }
    }
}
