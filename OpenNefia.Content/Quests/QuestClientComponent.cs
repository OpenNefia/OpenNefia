using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Marks this entity as able to give quests.
    /// </summary>
    /// <remarks>
    /// This shouldn't be declared in prototypes since it is added automatically
    /// when the map is renewed.
    /// </remarks>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class QuestClientComponent : Component
    {
        /// <summary>
        /// True if this character was passed a quest item and a karma penalty
        /// should be applied if the item kills the character.
        /// </summary>
        [DataField]
        public bool WasPassedQuestItem { get; set; } = false;
    }
}