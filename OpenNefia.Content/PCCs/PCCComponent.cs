using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.PCCs
{
    [RegisterComponent]
    public sealed class PCCComponent : Component
    {
        public override string Name => "PCC";

        [DataField("pccParts")]
        public List<PCCPart> PCCParts { get; } = new();
    }
}
