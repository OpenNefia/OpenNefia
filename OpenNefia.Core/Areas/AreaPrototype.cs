using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    [Prototype("Area")]
    public sealed class AreaPrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("initialFloors")]
        private readonly Dictionary<AreaFloorId, PrototypeId<MapPrototype>> _initialFloors = new();

        public IReadOnlyDictionary<AreaFloorId, PrototypeId<MapPrototype>> InitialFloors => _initialFloors;

        [DataField]
        public AreaFloorId? StartingFloor { get; }
    }
}
