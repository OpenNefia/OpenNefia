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
using OpenNefia.Core.SaveGames;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Areas
{
    public interface IAreaEntranceSystem : IEntitySystem
    {
        /// <summary>
        /// The topmost parent area of the last visited area
        /// that is marked "global" (has a <see cref="AreaTypeGlobalComponent"/>).
        /// </summary>
        AreaId? CurrentGlobalAreaID { get; }

        /// <summary>
        /// Returns the topmost parent area of the last visited area
        /// that is marked "global" (has a <see cref="AreaTypeGlobalComponent"/>).
        /// </summary>
        /// <remarks>
        /// Why this is needed: You're inside a town and want to know what continent
        /// you're in (North or South Tyris, Lost Irva, etc.) so you can gather the
        /// Return locations in just that continent.
        /// </remarks>
        /// <param name="globalArea"></param>
        /// <returns></returns>
        bool TryGetCurrentGlobalArea([NotNullWhen(true)] out IArea? globalArea);

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
        [Dependency] private readonly IAreaManager _areaManager = default!;

        [RegisterSaveData("Elona.AreaEntranceSystem.CurrentGlobalAreaID")]
        public AreaId? CurrentGlobalAreaID { get; set; } = null;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapEntranceComponent, AfterEntitySteppedOnEvent>(DisplayAreaEntranceMessage);
            SubscribeEntity<GetAreaEntranceMessageEvent>(GetDefaultEntranceMessage, priority: EventPriorities.Highest);
            SubscribeBroadcast<ActiveMapChangedEvent>(UpdateCurrentGlobalAreaID);
        }

        private void UpdateCurrentGlobalAreaID(ActiveMapChangedEvent args)
        {
            // NOTE: The global area is updated such that entering a map *without*
            // an area preserves the current global area. This is so that the player
            // will not be stranded and can't cast Return out of those maps (because
            // Return depends on the global area, and there's no way to get the global
            // area through parent-child relationships if the map has no area, and thus
            // no parent area).
            if (TryArea(args.NewMap, out var area))
            {
                IArea? found = null;
                if (HasComp<AreaTypeGlobalComponent>(area.AreaEntityUid))
                    found = area;

                foreach (var parent in _areaManager.EnumerateParentAreas(area))
                {
                    if (HasComp<AreaTypeGlobalComponent>(parent.AreaEntityUid))
                        found = parent;
                }

                CurrentGlobalAreaID = found?.Id;
            
                if (CurrentGlobalAreaID == null)
                {
                    Logger.ErrorS("areaEntrances", $"No current global area while inside map {args.NewMap}, area {area}.");
                }
            }
        }

        public bool TryGetCurrentGlobalArea([NotNullWhen(true)] out IArea? area)
        {
            if (CurrentGlobalAreaID == null)
            {
                area = null;
                return false;
            }
            return _areaManager.TryGetArea(CurrentGlobalAreaID.Value, out area);
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
