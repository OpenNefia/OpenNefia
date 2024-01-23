using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items.Impl
{
    /// <summary>
    /// When combined with an <see cref="ItemContainerComponent"/>, allows
    /// the player to open and take out items from the container.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TrunkComponent : Component
    {
        /// <summary>
        /// Karma penalty for opening this trunk.
        /// </summary>
        [DataField]
        public int KarmaPenalty { get; set; }
    }
}