using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Chests
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ChestComponent : Component
    {
        [DataField]
        public int LockpickDifficulty { get; set; }

        [DataField]
        public bool HasItems { get; set; } = true;

        [DataField]
        public int? ItemCount { get; set; }

        [DataField]
        public int ItemLevel { get; set; }

        [DataField]
        public bool DisplayLevelInName { get; set; } = false;

        /// <summary>
        /// Random seed to prevent save-scumming.
        /// </summary>
        [DataField]
        public int RandomSeed { get; set; }

        /// <summary>
        /// Chance that one (1) small medal will be generated when this chest is opened.
        /// </summary>
        [DataField]
        public float? SmallMedalProb { get; set; }
    }

    #region Special Chests
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SmallGambleChestComponent : Component
    {    }
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SafeComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class WalletComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SuitcaseComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TreasureBallComponent : Component
    {
        [DataField]
        public Quality ItemQuality { get; set; }
    }
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class BejeweledChestComponent : Component
    {    }
    
    #endregion
}