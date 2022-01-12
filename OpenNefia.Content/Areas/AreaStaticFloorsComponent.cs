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
    [RegisterComponent]
    public sealed class AreaStaticFloorsComponent : Component
    {
        public override string Name => "AreaStaticFloors";

        [DataField]
        private readonly Dictionary<AreaFloorId, PrototypeId<MapPrototype>> _floors = new();

        public IReadOnlyDictionary<AreaFloorId, PrototypeId<MapPrototype>> Floors => _floors;
    }
}
