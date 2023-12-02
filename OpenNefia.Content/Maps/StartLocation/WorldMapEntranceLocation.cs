using OpenNefia.Content.Areas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Places the player on a world map entrance that leads to the global area of the previous map.
    /// The entrance entity in question must have a <see cref="WorldMapEntranceComponent"/>.
    /// </summary>
    public class WorldMapEntranceLocation : IMapStartLocation
    {
        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var prevMap = mapMan.GetMapOfEntity(ent);

            if (!areaMan.TryGetAreaOfMap(prevMap, out var prevArea))
            {
                Logger.Error($"Previous map is not in a global area! {prevMap}");
                return new CenterMapLocation().GetStartPosition(ent, map);
            }

            var lookup = EntitySystem.Get<IEntityLookup>();
            var entrance = lookup.EntityQueryInMap<WorldMapEntranceComponent>(map)
                .Where(e => e.Entrance.MapIdSpecifier.GetOrGenerateAreaId() == prevArea.Id)
                .FirstOrDefault();

            if (entrance == null)
            {
                Logger.Error($"Could not find {nameof(WorldMapEntranceComponent)} leading to area {prevArea}!");
                return new CenterMapLocation().GetStartPosition(ent, map);
            }

            return entMan.GetComponent<SpatialComponent>(entrance.Owner).LocalPosition;
        }
    }
}
