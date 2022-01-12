using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomAreas
{
    /// <summary>
    /// When attached to an entity prototype, indicates that this entity is an area entity
    /// that can be generated randomly on the world map.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class RandomAreaComponent : Component
    {
        public override string Name => "RandomArea";
    }
}
