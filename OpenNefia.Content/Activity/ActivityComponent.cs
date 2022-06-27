using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
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
        /// Slot holding the component with the current activity.
        /// </summary>
        [DataField]
        public SlotId? ActivityComponentSlotId { get; set; }

        [DataField]
        public GameTimeSpan TimeRemaining { get; set; } = GameTimeSpan.Zero;
    }
}
