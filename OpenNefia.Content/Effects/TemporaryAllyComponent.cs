﻿using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects
{
    /// <summary>
    /// Indicates this character is a temporary ally, either through an escort quest
    /// or from being added as part of a sidequest (Poppy, etc). This will prevent certain interact actions
    /// ("Inventory", "Give", etc.) and also prevent using them as Riding mounts.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TemporaryAllyComponent : Component
    {
        /// <summary>
        /// If <c>true</c>, the player can cast Return with this ally in their party.
        /// </summary>
        /// <remarks>
        /// In Elona+, Belphat the Cosmic Sword would have this flag set to <c>true</c>.
        /// </remarks>
        [DataField]
        public bool AllowsReturning { get; set; } = false;
    }
}