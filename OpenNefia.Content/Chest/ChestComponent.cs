using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Chest
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ChestComponent : Component
    {
        public override string Name => "Chest";

        [DataField]
        public int LockpickDifficulty { get; set; }

        [DataField]
        public bool HasItems { get; set; } = true;

        [DataField]
        public int ItemLevel { get; set; }

        [DataField]
        public bool DisplayLevelInName { get; set; } = false;

        /// <summary>
        /// Random seed to prevent save-scumming.
        /// </summary>
        [DataField]
        public int RandomSeed { get; set; }
    }
}