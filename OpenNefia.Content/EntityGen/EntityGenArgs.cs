using OpenNefia.Content.Pickable;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EntityGen
{
    public abstract class EntityGenArgs
    {
    }

    public sealed class EntityGenArgSet : Blackboard<EntityGenArgs>
    {
        public static EntityGenArgSet Make(params EntityGenArgs[] rest)
        {
            var result = new EntityGenArgSet();

            foreach (var param in rest)
                result.Add(param);

            result.Ensure<EntityGenCommonArgs>();

            return result;
        }
    }

    public sealed class EntityGenCommonArgs : EntityGenArgs
    {
        /// <summary>
        /// Amount of this entity to spawn. If null, a random amount is chosen.
        /// </summary>
        public int? Amount { get; set; }

        /// <summary>
        /// Level to set the generated entity to.
        /// </summary>
        public int? LevelOverride { get; set; }

        /// <summary>
        /// Quality of the entity to spawn.
        /// </summary>
        public Quality? Quality { get; set; }

        /// <summary>
        /// If true, the entity will not be stacked with anything on the same tile.
        /// </summary>
        public bool NoStack { get; set; } = false;

        /// <summary>
        /// If true, the entity's type will not be randomly modified. Applies to things like Shade generation.
        /// </summary>
        public bool NoRandomModify { get; set; } = false;

        /// <summary>
        /// If true, the <see cref="EntityGeneratedEvent"/> will not be fired when spawning this entity.
        /// </summary>
        public bool NoFireGeneratedEvent { get; set; } = false;
    }

    public sealed class ItemGenArgs : EntityGenArgs
    {
        /// <summary>
        /// If true, this item is being generated in a shopkeeper's inventory.
        /// </summary>
        public bool IsShop { get; set; } = false;

        /// <summary>
        /// If true, do not log this item in the oracle artifacts log if it is unique.
        /// </summary>
        public bool NoOracle { get; set; } = false;

        /// <summary>
        /// Own state to generate this item/pickable with.
        /// </summary>
        public OwnState OwnState { get; set; } = OwnState.None;
    }

    public sealed class MefGenArgs : EntityGenArgs
    {
        /// <summary>
        /// Number of turns this mef will last for.
        /// </summary>
        public int TurnDuration { get; set; } = 10;

        /// <summary>
        /// Power of this mef.
        /// </summary>
        public int Power { get; set; } = 10;

        /// <summary>
        /// The entity responsible for creating this mef.
        /// </summary>
        public EntityUid? Origin { get; set; } = null;
    }
}
