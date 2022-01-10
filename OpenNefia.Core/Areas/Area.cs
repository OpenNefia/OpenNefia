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
        [DataField(required: true)]
        /// <inheritdoc/>
        public AreaId Id { get; internal set; }

        [DataField(required: true)]
        /// <inheritdoc/>
        public EntityUid AreaEntityUid { get; internal set; }

        [DataField("containedMaps", required: true)]
        internal Dictionary<AreaFloorId, AreaFloor> _containedMaps = new();

        /// <inheritdoc/>
        public IReadOnlyDictionary<AreaFloorId, AreaFloor> ContainedMaps => _containedMaps;
    }
}
