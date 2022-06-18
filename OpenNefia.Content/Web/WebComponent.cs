using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Web
{
    [RegisterComponent]
    public sealed class WebComponent : Component
    {
        public override string Name => "Web";

        [DataField]
        public int UntangleDifficulty { get; set; }
    }
}
