using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class PlayerComponent : Component
    {
        public override string Name => "Player";
    }
}
