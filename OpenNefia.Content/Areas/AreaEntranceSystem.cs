using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using static OpenNefia.Core.Prototypes.EntityPrototype;

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
            IEntityLoadContext? context = null;
            if (Resolve(area.AreaEntityUid, ref areaEntranceComp, logMissing: false))
            {
                context = new BasicComponentLoadContext(areaEntranceComp.Components);
            }

            var entranceEnt = EntityManager.SpawnEntity(Protos.Feat.MapEntrance, coords, context);

            var worldMapEntrance = EntityManager.EnsureComponent<WorldMapEntranceComponent>(entranceEnt);
            worldMapEntrance.Entrance.MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id);

            var areaEnt = area.AreaEntityUid;

            return worldMapEntrance;
        }
    }

    public class BasicComponentLoadContext : IEntityLoadContext
    {
        private ComponentRegistry _components;

        public BasicComponentLoadContext(ComponentRegistry components)
        {
            _components = components;
        }

        public IComponent GetComponentData(string componentName, IComponent? protoData)
        {
            return _components[componentName];
        }

        public IEnumerable<string> GetExtraComponentTypes()
        {
            return _components.Keys;
        }

        public bool ShouldLoadComponent(string componentName)
        {
            return _components.ContainsKey(componentName);
        }
    }
}
