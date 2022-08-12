using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
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
    {
        public override string Name => "SmallGambleChest";
    }
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SafeComponent : Component
    {
        public override string Name => "Safe";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class WalletComponent : Component
    {
        public override string Name => "Wallet";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class SuitcaseComponent : Component
    {
        public override string Name => "Suitcase";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TreasureBallComponent : Component
    {
        public override string Name => "TreasureBall";

        [DataField]
        public Quality ItemQuality { get; set; }
    }
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class BejeweledChestComponent : Component
    {
        public override string Name => "BejeweledChest";
    }
    
    #endregion
}