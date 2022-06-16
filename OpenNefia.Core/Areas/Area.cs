using OpenNefia.Core.GameObjects;
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
    [DataDefinition]
    internal sealed class Area : IArea
    {
        /// <inheritdoc/>
        [DataField(required: true)]
        public AreaId Id { get; internal set; }

        /// <inheritdoc/>
        [DataField(required: true)]
        public GlobalAreaId? GlobalId { get; internal set; }

        /// <inheritdoc/>
        [DataField(required: true)]
        public EntityUid AreaEntityUid { get; internal set; }

        [DataField("maps", required: true)]
        internal Dictionary<AreaFloorId, AreaFloor> _containedMaps = new();

        /// <inheritdoc/>
        public IReadOnlyDictionary<AreaFloorId, AreaFloor> ContainedMaps => _containedMaps;

        /// <inheritdoc/>
        [DataField]
        public AreaFloorId? StartingFloor { get; set; }

        public override string ToString()
        {
            return $"Area(id={Id}, entUid={AreaEntityUid}, globalId={GlobalId})";
        }
    }
}
