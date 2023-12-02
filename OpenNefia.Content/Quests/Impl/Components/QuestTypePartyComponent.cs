using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must earn enough points by performing at a party. Gives
    /// music tickets if the score is high enough.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1009</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypePartyComponent : Component
    {
        /// <summary>
        /// Points needed for quest success.
        /// </summary>
        [DataField]
        public int RequiredPoints { get; set; } = 0;

        /// <summary>
        /// Points the player has earned so far.
        /// </summary>
        [DataField]
        public int CurrentPoints { get; set; } = 0;
    }
}