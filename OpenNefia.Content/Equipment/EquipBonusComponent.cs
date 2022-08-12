using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Equipment
{
    /// <summary>
    /// Represents a bonus attached to an item (+2, -9, etc.)
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class BonusComponent : Component
    {
        public override string Name => "Bonus";

        [DataField]
        public int Bonus { get; set; } = 0;
    }
}