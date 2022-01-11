using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    /// <para>
    /// Interface for an area.
    /// </para>
    /// <para>
    /// Areas are collections of maps that exist globally, and are available for access
    /// no matter which map the player is currently in. Areas are implemented like maps,
    /// by attaching an entity to them and putting them in the "nullspace" map (ID 0).
    /// </para>
    /// <para>
    /// Areas are used for random Nefia and collections of related maps.
    /// (think Vernis/The Mine/Robber's Hideout/Test Ground)
    /// </para>
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IArea
    {
        /// <summary>
        /// ID of this area.
        /// </summary>
        public AreaId Id { get; }
       
        /// <summary>
        /// ID used for referencing this specific area in prototype data.
        /// </summary>
        /// <remarks>
        /// Problem this is intending to solve: placing entrances to areas in prototype
        /// data/map blueprints. In prototypes, you won't know what <see cref="Id"/> an 
        /// area is going to end up with yet.
        /// </remarks>
        public GlobalAreaId? GlobalId { get; }

        /// <summary>
        /// Entity that is associated with this area.
        /// </summary>
        public EntityUid AreaEntityUid { get; }

        /// <summary>
        /// Maps contained in this area.
        /// </summary>
        IReadOnlyDictionary<AreaFloorId, AreaFloor> ContainedMaps { get; }

        /// <summary>
        /// Initial floor to place the player in when generating area entrances.
        /// </summary>
        public AreaFloorId? StartingFloor { get; set; }
    }
}