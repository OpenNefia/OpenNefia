using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    internal sealed class Area : IArea
    {
        /// <inheritdoc/>
        public AreaId Id { get; internal set; }

        /// <inheritdoc/>
        public EntityUid AreaEntityUid { get; internal set; }

        internal Dictionary<AreaFloorId, AreaFloor> _containedMaps = new();

        /// <inheritdoc/>
        public IReadOnlyDictionary<AreaFloorId, AreaFloor> ContainedMaps => _containedMaps;
    }
}
