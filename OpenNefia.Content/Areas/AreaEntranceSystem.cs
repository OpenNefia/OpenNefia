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
}
