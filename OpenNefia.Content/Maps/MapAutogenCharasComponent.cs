using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapAutogenCharasComponent : Component
    {
        public override string Name => "MapAutogenCharas";

        /// <summary>
        /// How many randomly generated characters to spawn when this map
        /// is first generated.
        /// </summary>
        [DataField]
        public int Amount { get; set; }
    }

    [RegisterComponent]
    public class MapAutogenEntitiesComponent : Component
    {
        public override string Name => "MapAutogenEntities";

        /// <summary>
        /// Extra entity types to generate multiple of when this map
        /// is first generated.
        /// </summary>
        [DataField]
        public List<EntityAutogenSpec> Specs { get; } = new();
    }

    [DataDefinition]
    public class EntityAutogenSpec
    {
        [DataField]
        public PrototypeId<EntityPrototype>? ProtoId { get; set; }

        [DataField(required: true)]
        public int Amount { get; set; }

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}
