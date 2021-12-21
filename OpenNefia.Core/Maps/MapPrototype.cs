using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Core.Maps
{
    [Prototype("Map")]
    public class MapPrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        public ResourcePath BlueprintPath { get; } = default!;
    }
}