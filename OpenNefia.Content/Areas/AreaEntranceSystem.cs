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
        /// <summary>
        /// Returns the default floor that should be used when entering this area
        /// from a map entrance.
        /// </summary>
        /// <param name="area">Area to query.</param>
        /// <param name="floorId">Floor ID that overrides the area's starting floor.</param>
        AreaFloorId? GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaDefEntrance = null);

        /// <summary>
        /// Creates a map entrance entity according to the properties in the
        /// area entity's <see cref="AreaEntranceComponent"/>.
        /// </summary>
        /// <param name="area">Area to create the entrance for.</param>
        /// <param name="coords">Coordinates to place the map entrance entity at.</param>
        /// <returns>The <see cref="WorldMapEntranceComponent"/> of the created map entrance entity.</returns>
        WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null);
    }

    public sealed class AreaEntranceSystem : EntitySystem, IAreaEntranceSystem
    {
        /// <inheritdoc/>
        public AreaFloorId? GetStartingFloor(IArea area, AreaFloorId? floorId,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            if (!Resolve(area.AreaEntityUid, ref areaEntranceComp))
                return floorId;

            return floorId != null ? floorId.Value : areaEntranceComp.StartingFloor;
        }

        /// <inheritdoc/>
        public WorldMapEntranceComponent CreateAreaEntrance(IArea area, MapCoordinates coords,
            AreaEntranceComponent? areaEntranceComp = null)
        {
            IEntityLoadContext? context = null;
            IMapStartLocation? startLocation = null;
            if (Resolve(area.AreaEntityUid, ref areaEntranceComp, logMissing: false))
            {
                context = new BasicComponentLoadContext(areaEntranceComp.Components);
                startLocation = areaEntranceComp.StartLocation;
            }

            var entranceEnt = EntityManager.SpawnEntity(Protos.Feat.MapEntrance, coords, context);

            var worldMapEntrance = EntityManager.EnsureComponent<WorldMapEntranceComponent>(entranceEnt);
            worldMapEntrance.Entrance.MapIdSpecifier = new AreaFloorMapIdSpecifier(area.Id);
            if (startLocation != null)
                worldMapEntrance.Entrance.StartLocation = startLocation;

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
