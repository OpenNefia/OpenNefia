using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    ///     Represents an area inside the ECS system.
    /// </summary>
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaComponent : Component
    {
        [DataField("areaId")]
        private AreaId _areaId = Areas.AreaId.Nullspace;

        public AreaId AreaId
        {
            get => _areaId;
            internal set => _areaId = value;
        }
    }
}