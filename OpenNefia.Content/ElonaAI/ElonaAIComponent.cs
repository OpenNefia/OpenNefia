using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ElonaAI
{
    /// <summary>
    /// State relevant to vanilla Elona's AI logic.
    /// </summary>
    [RegisterComponent]
    public class ElonaAIComponent : Component
    {
        public override string Name => "ElonaAI";

        [DataField]
        public EntityUid? CurrentTarget { get; set; }
    }
}
