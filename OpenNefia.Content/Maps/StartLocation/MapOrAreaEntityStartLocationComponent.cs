using OpenNefia.Content.Areas;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Defers placing the player to either:
    /// - the map entrance found in a <see cref="MapStartLocationComponent"/> on the map
    /// - the map entrance found in the <see cref="AreaEntranceComponent"/> on the map's area
    /// If either of these are missing, the player is placed at the center of the map (by default).
    /// </summary>
    /// <remarks>
    /// According to <see cref="MapComponent"/>, this is the default.
    /// </remarks>
    public class MapOrAreaEntityStartLocation : IMapStartLocation
    {
        [DataField]
        public IMapStartLocation FallbackLocation { get; set; } = new CenterMapLocation();

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var areaMan = IoCManager.Resolve<IAreaManager>();

            if (entMan.TryGetComponent<MapStartLocationComponent>(map.MapEntityUid, out var startLoc))
            {
                if (startLoc.StartLocation is not MapOrAreaEntityStartLocation)
                    return startLoc.StartLocation.GetStartPosition(ent, map);
            }

            if (areaMan.TryGetAreaOfMap(map, out var area)
                && entMan.TryGetComponent<AreaEntranceComponent>(area.AreaEntityUid, out var areaEntrance))
            {
                if (areaEntrance.StartLocation != null
                    && areaEntrance.StartLocation is not MapOrAreaEntityStartLocation)
                    return areaEntrance.StartLocation.GetStartPosition(ent, map);
            }

            if (FallbackLocation is not MapOrAreaEntityStartLocation)
                return FallbackLocation.GetStartPosition(ent, map);

            return new CenterMapLocation().GetStartPosition(ent, map);
        }
    }
}