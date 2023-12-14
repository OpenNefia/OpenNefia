using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Upon entering a floor specified in this component that hasn't been generated yet,
    /// the provided map will be generated and used as the floor.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaStaticFloorsComponent : Component
    {
        [DataField("floors", required: true)]
        private readonly Dictionary<AreaFloorId, PrototypeId<MapPrototype>> _floors = new();
        public IReadOnlyDictionary<AreaFloorId, PrototypeId<MapPrototype>> Floors => _floors;
    }
}
