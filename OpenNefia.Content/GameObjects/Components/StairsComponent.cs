using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class StairsComponent : Component
    {
        [DataField(required: true)]
        public StairsDirection Direction { get; set; }

        /// <summary>
        /// Entrance to use.
        /// </summary>
        [DataField]
        public MapEntrance Entrance { get; set; } = new();
    }

    public enum StairsDirection
    {
        Up,
        Down
    }
}
