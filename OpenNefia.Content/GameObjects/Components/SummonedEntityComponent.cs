using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public sealed class SummonedEntityComponent : Component
    {
        public override string Name => "SummonedEntity";
    }
}
