using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    ///     Represents an area inside the ECS system.
    /// </summary>
    public class AreaComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "Area";

        [DataField("areaId")]
        private AreaId _areaId = Areas.AreaId.Nullspace;

        public AreaId AreaId
        {
            get => _areaId;
            internal set => _areaId = value;
        }

        /// <summary>
        /// Initial floor to place the player in when generating area entrances.
        /// </summary>
        public AreaFloorId? StartingFloor { get; set; }
    }
}