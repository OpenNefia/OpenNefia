using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Indicates this map is part of an immediate quest (party, hunting, etc).
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MapImmediateQuestComponent : Component
    {
        [DataField]
        public EntityUid QuestUid { get; set; }

        /// <summary>
        /// How much time should pass before the "time remaining for quest" message is shown.
        /// </summary>
        [DataField]
        public GameTimeSpan TimeToNextNotify { get; set; }

        /// <summary>
        /// Location to return the player to after the quest is finished.
        /// </summary>
        [DataField]
        public MapEntrance PreviousLocation { get; set; } = new MapEntrance();
    }
}