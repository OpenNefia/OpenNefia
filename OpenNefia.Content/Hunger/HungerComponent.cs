using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hunger
{
    [RegisterComponent]
    public sealed class HungerComponent : Component
    {
        public override string Name => "Hunger";

        [DataField]
        public int Nutrition { get; set; } = 0;
    }
}
