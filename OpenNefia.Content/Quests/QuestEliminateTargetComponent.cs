using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Marks this quest target as an entity that must be eliminated for a quest.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class QuestEliminateTargetComponent : Component
    {
        [DataField]
        public string Tag { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reports the quest targets remaining in the map when one of them is killed.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapReportQuestEliminateTargetsComponent : Component
    {
    }   
}