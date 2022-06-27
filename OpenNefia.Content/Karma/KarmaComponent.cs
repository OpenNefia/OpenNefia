using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Karma
{
    [RegisterComponent]
    public sealed class KarmaComponent : Component
    {
        public override string Name => "Karma";

        [DataField]
        public Stat<int> Karma { get; set; } = new(0);
    }

    public static class KarmaLevels
    {
        public const int Bad = -30;
        public const int Good = 20;
    }
}
