using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dungeons
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaDungeonComponent : Component
    {
        public override string Name => "AreaDungeon";

        public int DeepestFloor { get; set; } = 1;
    }
}
