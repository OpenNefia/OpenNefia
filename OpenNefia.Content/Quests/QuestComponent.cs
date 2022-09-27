using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Component found on all quest entities, which tracks essential quest data.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestComponent : Component
    {
        /// <summary>
        /// Current state of this quest (accepted/completed/failed/etc.)
        /// </summary>
        [DataField]
        public QuestState State { get; set; } = QuestState.NotAccepted;

        /// <summary>
        /// Difficulty of this quest.
        /// </summary>
        [DataField]
        public int Difficulty { get; set; } = 0;

        /// <summary>
        /// Date when this quest is removed from the town board.
        /// </summary>
        [DataField]
        public GameDateTime TownBoardExpirationDate { get; set; } = GameDateTime.MaxValue;

        /// <summary>
        /// Locale key root of the quest pointing to a list of tables. They should all have "Name"
        /// and "Description" fields.
        /// </summary>
        [DataField]
        public LocaleKey LocaleKeyRoot { get; set; }

        /// <summary>
        /// Random seed of this quest.
        /// </summary>
        [DataField]
        public int RandomSeed { get; set; }

        /// <summary>
        /// Time from when the quest is accepted until its deadline.
        /// </summary>
        [DataField]
        public GameTimeSpan? TimeUntilDeadline { get; set; }

        /// <summary>
        /// Date when this quest is automatically failed.
        /// </summary>
        [DataField]
        public GameDateTime? Deadline { get; set; }

        /// <summary>
        /// Entity UID of the client who gave this quest.
        /// </summary>
        [DataField]
        public EntityUid ClientEntity { get; set; }

        /// <summary>
        /// Name of the client who gave this quest.
        /// </summary>
        [DataField]
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Map containing the client who gave this quest.
        /// </summary>
        [DataField]
        public MapId ClientOriginatingMap { get; set; }

        /// <summary>
        /// Name of the map containing the client who gave this quest.
        /// </summary>
        [DataField]
        public string ClientOriginatingMapName { get; set; } = string.Empty;
    }

    public enum QuestState
    {
        NotAccepted,
        Accepted,
        Completed,
    }

    /// <summary>
    /// Specifies a deadline for a quest.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestDeadlinesComponent : Component
    {
        [DataField]
        public IntRange? TownBoardExpirationDays { get; set; } = new IntRange(3, 4);

        [DataField]
        public IntRange? DeadlineDays { get; set; }
    }

    /// <summary>
    /// Specifies that a quest can only be generated if the player's fame is high enough.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestMinimumFameComponent : Component
    {
        /// <summary>
        /// Minimum fame required for this quest to be generated.
        /// </summary>
        [DataField(required: true)]
        public int MinimumFame { get; set; } = 0;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestRewardGoldComponent : Component
    {
        [DataField]
        public int GoldModifier { get; set; } = 0;

        /// <summary>
        /// If true, adjust the amount of gold awarded based on the player's level.
        /// </summary>
        /// <remarks>
        /// In Elona variants such as omake_overhaul, this behavior is changed to adjust the amount
        /// of gold based on the player's fame instead.
        /// </remarks>
        [DataField]
        public bool ModifyGoldBasedOnPlayerLevel { get; set; } = true;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestRewardRandomCategoryComponent : Component
    {
        [DataField(required: true)]
        public PrototypeId<TagSetPrototype> ItemCategories { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestRewardSingleCategoryComponent : Component
    {
        [DataField(required: true)]
        public PrototypeId<TagPrototype> ItemCategory { get; set; }
    }

    /// <summary>
    /// Specifies that this quest is an "immediate quest", which takes place within a designated set
    /// of maps. Leaving this set of maps causes the quest to be failed.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestImmediateRegionsComponent : Component
    {
        /// <summary>
        /// Maps this quest takes place in.
        /// </summary>
        [DataField(required: true)]
        public HashSet<MapId> QuestMaps { get; set; } = new();

        /// <summary>
        /// Areas this quest takes place in.
        /// </summary>
        [DataField(required: true)]
        public HashSet<AreaId> QuestAreas { get; set; } = new();
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestImmediateTimeLimitComponent : Component
    {
        [DataField(required: true)]
        public GameDateTime ExpirationDate { get; set; } = GameDateTime.MaxValue;

        [DataField]
        public GameTimeSpan ExpirationNotifyInterval { get; set; } = GameTimeSpan.MaxValue;
    }
}