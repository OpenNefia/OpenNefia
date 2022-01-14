using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Feats
{
    /// <summary>
    /// Holds the current Feats of an entity as well as the current amount of learnable Feats.
    /// </summary>
    [RegisterComponent]
    public class FeatsComponent : Component
    {
        public override string Name => "Feats";
        
        /// <summary>
        /// Current amount of learnable Feats.
        /// </summary>
        [DataField]
        public int NumberOfFeatsAcquirable { get; set; } = 0;

        /// <summary>
        /// Current Feats and the respective Levels
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<FeatPrototype>, int> Feats { get; } = new();

        public int Level(PrototypeId<FeatPrototype> id)
        {
            if (Feats.TryGetValue(id, out var val))
                return val;
            return 0;
        }
    }
}
