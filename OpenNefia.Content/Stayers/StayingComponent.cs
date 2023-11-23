using OpenNefia.Content.Maps;
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

namespace OpenNefia.Content.Stayers
{
    [DataDefinition]
    public sealed class StayingLocation
    {
        public StayingLocation(IStayerCriteria criteria, IMapStartLocation location, string areaName, string tag)
        {
            Criteria = criteria;
            Location = location;
            AreaName = areaName;
            Tag = tag;
        }

        /// <summary>
        /// Criteria for the entity to show up again.
        /// </summary>
        [DataField]
        public IStayerCriteria Criteria { get; set; }

        /// <summary>
        /// Position to respawn the entity at when the map is entered.
        /// </summary>
        [DataField]
        public IMapStartLocation Location { get; set; } = new MapAIAnchorLocation();

        /// <summary>
        /// Name of the area this entity is staying in.
        /// </summary>
        [DataField]
        public string AreaName { get; set; }

        /// <summary>
        /// Tag indicating which staying "group" this character belongs to,
        /// such that they can be enumerated/addressed separately from others.
        /// This is important since in vanilla Elona there are two groups
        /// of "global" characters, allies (1-16) and adventurers (17-56).
        /// </summary>
        [DataField]
        public string Tag { get; set; } = StayingTags.Ally;
    }

    /// <summary>
    /// A character that is staying in a player-owned building,
    /// but should still be available in memory while the player
    /// is adventuring elsewhere. When the player changes maps,
    /// this character will be moved into the staying entities
    /// container of the <see cref="IStayersSystem"/> and moved
    /// back when the map is re-entered.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class StayingComponent : Component
    {
        /// <summary>
        /// Criteria for the entity to show up again.
        /// </summary>
        [DataField]
        public StayingLocation? StayingLocation { get; set; } = null;
    }

    public static class StayingTags
    {
        public const string Ally = $"Elona.{nameof(Ally)}";
        public const string Adventurer = $"Elona.{nameof(Adventurer)}";
    }

    /// <summary>
    /// Determines when a stayer can show up in a map.
    /// There are two main cases:
    /// <list type="bullet">
    /// <item>When a map with a matching ID is entered (map has been generated)</item>
    /// <item>When the given floor of an area is entered (map has not been generated/dungeon adventurers)</item>
    /// </list>
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IStayerCriteria
    {
        bool CanAppear(EntityUid ent, IMap map);
    }

    public sealed class MapIdStayerCriteria : IStayerCriteria
    {
        public MapIdStayerCriteria(MapId mapID)
        {
            MapID = mapID;
        }

        [DataField(required: true)]
        public MapId MapID { get; }

        public bool CanAppear(EntityUid ent, IMap map)
        {
            return map.Id == MapID;
        }
    }

    public sealed class AreaFloorStayerCriteria : IStayerCriteria
    {
        public AreaFloorStayerCriteria(AreaId areaID, AreaFloorId areaFloorID)
        {
            AreaID = areaID;
            AreaFloorID = areaFloorID;
        }

        [DataField(required: true)]
        public AreaId AreaID { get; }

        [DataField(required: true)]
        public AreaFloorId AreaFloorID { get; }

        public bool CanAppear(EntityUid ent, IMap map)
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            if (!areaMan.TryGetAreaAndFloorOfMap(map.Id, out var area, out var floor))
                return false;

            return area.Id == AreaID && floor == AreaFloorID;
        }
    }
}