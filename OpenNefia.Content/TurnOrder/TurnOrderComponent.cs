using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.TurnOrder
{
    /// <summary>
    /// Indicates an entity that can receive turn order events.
    /// </summary>
    [RegisterComponent]
    public class TurnOrderComponent : Component
    {
        public override string Name => "TurnOrder";

        /// <summary>
        /// How much time this entity has stockpiled to take actions.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is incremented every turn based on speed, regardless of whether the entity moves,
        /// and is subtracted from each time the entity takes an action.
        /// </para>
        /// <para>
        /// According to the HSP source, 5 time units equal one in-game second.
        /// </para>
        /// </remarks>
        [DataField]
        public int TimeThisTurn { get; set; } = 0;
        
        /// <summary>
        /// How many in-game turns this entity has taken across its entire lifetime.
        /// </summary>
        [DataField]
        public int TurnsAlive { get; set; }

        /// <summary>
        /// The entity's current speed.
        /// </summary>
        /// <remarks>
        /// This should not be set to a value below 10.
        /// </remarks>
        [DataField]
        public int CurrentSpeed { get; set; } = 10;

        /// <summary>
        /// Speed percentage to apply to this character. Default is 1.0 (100%).
        /// </summary>
        [DataField]
        public float SpeedPercentage { get; set; } = 1f;

        [DataField]
        public int SpeedCorrection { get; set; } = 0;
    }
}
