using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Arena
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaArenaComponent : Component
    {
        public override string Name => "AreaArena";

        [DataField]
        public GameDateTime SeedRenewDate { get; set; } = GameDateTime.Zero;

        [DataField]
        public int Seed { get; set; } = 0;
    }
}
