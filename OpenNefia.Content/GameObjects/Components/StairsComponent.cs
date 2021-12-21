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
        /// <inheritdoc />
        public override string Name => "Stairs";

        [DataField(required: true)]
        public StairsDirection Direction { get; set; }
    }

    public enum StairsDirection
    {
        Up,
        Down
    }
}
