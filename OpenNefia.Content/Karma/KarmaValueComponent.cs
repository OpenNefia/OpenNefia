using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Karma
{
    /// <summary>
    /// Indicates that this character carries a karma penalty for killing them.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class KarmaValueComponent : Component
    {
        public override string Name => "KarmaValue";

        /// <summary>
        /// How much karma should be lost when this entity is killed.
        /// </summary>
        [DataField]
        public int KarmaValue { get; set; }
    }
}