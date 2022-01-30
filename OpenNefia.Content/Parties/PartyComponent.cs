using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Parties
{
    /// <summary>
    /// Handles grouping entities together as allies.
    /// </summary>
    [RegisterComponent]
    public class PartyComponent : Component
    {
        public override string Name => "Party";

        /// <summary>
        /// Allied entities that this entity is leading.
        /// </summary>
        /// <remarks>
        /// The leader themselves are also a member.
        /// </remarks>
        [DataField]
        public SortedSet<EntityUid> Members { get; } = new();

        /// <summary>
        /// Party leader entity that is leading this entity, if any.
        /// </summary>
        /// <remarks>
        /// The leader counts as leading themselves.
        /// </remarks>
        [DataField]
        public EntityUid? Leader { get; set; }
    }
}
