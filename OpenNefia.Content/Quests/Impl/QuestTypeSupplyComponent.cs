using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests.Impl
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeSupplyComponent : Component
    {
        /// <summary>
        /// ID of the item the client desires.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> TargetItemID { get; set; } = Protos.Item.Bug;
    }
}