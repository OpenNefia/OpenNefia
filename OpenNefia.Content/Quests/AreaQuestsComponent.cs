using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// <para>
    /// Tracks maps in this area that can provide quests. This is useful for being able to refer to
    /// other maps that can act as quest destinations without needing to load the map itself.
    /// </para>
    /// <para>
    /// An example is the escort quest. When the quest is generated, the engine needs to know what
    /// other towns can act as the destination for the quest. With this setup it becomes as easy as
    /// enumerating all areas with an <see cref="AreaQuestsComponent"/> and picking a map within one
    /// appropriately.
    /// </para>
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaQuestsComponent : Component
    {
        /// <summary>
        /// Set of maps in this area where quests can be generated (e.g. they should have quest
        /// boards).
        /// </summary>
        [DataField]
        public Dictionary<MapId, QuestHubData> QuestHubs { get; set; } = new();
    }

    [DataDefinition]
    public sealed class QuestHubData
    {
        /// <summary>
        /// Name of the map associated with this quest hub data.
        /// </summary>
        /// <remarks>
        /// $"We have this client secretly heading to {<see cref="MapName"/>} for certain reasons. [...]"
        /// </remarks>
        [DataField]
        public string MapName { get; set; } = string.Empty;

        /// <summary>
        /// List of characters in this quest hub that can act as quest clients.
        /// </summary>
        /// <remarks>
        /// Used by the delivery quest to find a delivery target in an unloaded map.
        /// </remarks>
        [DataField]
        public Dictionary<EntityUid, QuestClient> Clients { get; set; } = new();
    }

    [DataDefinition]
    public sealed class QuestClient
    {
        public QuestClient() { }

        public QuestClient(EntityUid clientEntity, string clientName)
        {
            ClientEntityUid = clientEntity;
            ClientName = clientName;
        }

        [DataField]
        public EntityUid ClientEntityUid { get; set; }

        [DataField]
        public string ClientName { get; set; } = string.Empty;
    }
}