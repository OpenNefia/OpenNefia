using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.InUse
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ItemUserComponent : Component
    {
        /// <summary>
        /// Entities that this entity is currently using.
        /// </summary>
        [DataField]
        public HashSet<EntityUid> InUse { get; set; } = new();
    }
}