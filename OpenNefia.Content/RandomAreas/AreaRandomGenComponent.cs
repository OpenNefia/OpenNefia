using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomAreas
{
    /// <summary>
    /// When attached to an aera entity prototype, indicates that this area can 
    /// be generated randomly in a map with a <see cref="MapRandomAreaManagerComponent"/>.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaRandomGenComponent : Component
    {
        public override string Name => "AreaRandomGen";
    }
}
