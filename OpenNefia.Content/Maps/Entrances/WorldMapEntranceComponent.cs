using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class WorldMapEntranceComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "WorldMapEntrance";
    }
}
