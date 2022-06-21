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
        /// Party ID of this entity.
        /// </summary>
        [DataField]
        public int? PartyID { get; set; } = null;
    }
}
