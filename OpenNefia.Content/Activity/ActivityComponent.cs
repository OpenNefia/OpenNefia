using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Activity
{
    [RegisterComponent]
    public sealed class ActivityComponent : Component
    {
        public override string Name => "Activity";

        /// <summary>
        /// Slot holding the component(s) of the current activity.
        /// </summary>
        [DataField]
        public SlotId? SlotID { get; set; }

        /// <summary>
        /// Prototype used to create the activity.
        /// </summary>
        [DataField]
        public PrototypeId<ActivityPrototype> PrototypeID { get; set; }

        /// <summary>
        /// Turns remaining for the activity.
        /// </summary>
        [DataField]
        public int TurnsRemaining { get; set; } = 0;
    }
}
