using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Feats
{
    /// <summary>
    /// The level associated with a feat.
    /// </summary>
    [DataDefinition]
    public class FeatLevel : IEquatable<FeatLevel>
    {
        /// <summary>
        /// Level of the feat.
        /// </summary>
        [DataField]
        public Stat<int> Level { get; set; } = new(0);

        public bool Equals(FeatLevel? other)
        {
            if (other == null) 
                return false;

            return other.Level == Level;
        }
    }
}
