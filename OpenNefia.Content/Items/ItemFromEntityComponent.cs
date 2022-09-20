using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    /// <summary>
    /// Indicates this entity originated from some other entity that had
    /// a prototype ID attached. Examples include milk and corpses.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EntityProtoSourceComponent : Component
    {
        /// <summary>
        /// Entity prototype ID of the entity that this item originated from.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> EntityID { get; set; } = Protos.Chara.Bug;
    }
}