using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Pregnancy
{
    [RegisterComponent]
    public class PregnancyComponent : Component
    {
        [DataField]
        public bool IsPregnant { get; set; }
    }
}