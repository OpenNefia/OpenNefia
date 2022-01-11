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

        /// <summary>
        /// If true, automatically register a global area with the same ID as 
        /// this prototype's ID when starting a new save.
        /// </summary>
        /// <remarks>
        /// TODO: Needs to be compatible with scenarios having their own area sets.
        /// This will probably become a HashSet of PrototypeIds in a future ScenarioPrototype.
        /// </remarks>
        [DataField]
        public bool CreateGlobal { get; } = false;

        // TODO map generators/extra initialization things instead of prototype ID
        [DataField("floors")]
        private readonly Dictionary<AreaFloorId, PrototypeId<MapPrototype>> _initialFloors = new();

        public IReadOnlyDictionary<AreaFloorId, PrototypeId<MapPrototype>> InitialFloors => _initialFloors;

        [DataField]
        public AreaFloorId? StartingFloor { get; }
    }
}
