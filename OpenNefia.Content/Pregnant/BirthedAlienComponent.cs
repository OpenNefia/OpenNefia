using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Pregnant
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class BirthedAlienComponent : Component
    {
        /// <summary>
        /// Entity that gave birth to this alien.
        /// </summary>
        [DataField]
        public EntityUid? ParentEntity { get; set; }

        /// <summary>
        /// Name of the entity that gave birth to this alien.
        /// </summary>
        [DataField]
        public string? ParentName { get; set; }
    }
}