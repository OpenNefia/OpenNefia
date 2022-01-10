using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class GrammarComponent : Component
    {
        public override string Name => "Grammar";

        [DataField("attributes")]
        public Dictionary<string, string> Attributes { get; } = new();
    }
}
