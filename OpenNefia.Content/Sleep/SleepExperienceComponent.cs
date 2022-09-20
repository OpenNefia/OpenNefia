using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Sleep
{
    [RegisterComponent]
    public sealed class SleepExperienceComponent : Component
    {
        [DataField]
        public int SleepExperience { get; set; } = 0;
    }
}
