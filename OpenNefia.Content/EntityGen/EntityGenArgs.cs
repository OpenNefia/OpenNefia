﻿using OpenNefia.Content.Pickable;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.EntityGen
{
    [ImplicitDataDefinitionForInheritors]
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
        [DataField]
        public int? Amount { get; set; }

        /// <summary>
        /// Level to set the generated entity to, irrespective of the entity's level in the prototype.
        /// </summary>
        [DataField]
        public int? LevelOverride { get; set; }

        /// <summary>
        /// Level for random generation purposes.
        /// </summary>
        [DataField]
        public int MinLevel { get; set; }

        /// <summary>
        /// Quality of the entity to spawn.
        /// </summary>
        [DataField]
        public Quality? Quality { get; set; }

        /// <summary>
        /// If true, the entity will not be stacked with anything on the same tile.
        /// </summary>
        [DataField]
        public bool NoStack { get; set; } = false;

        /// <summary>
        /// If true, the entity's type will not be randomly modified. Applies to things like Shade generation.
        /// </summary>
        [DataField]
        public bool NoRandomModify { get; set; } = false;

        /// <summary>
        /// If true, the <see cref="EntityGeneratedEvent"/> will not be fired when spawning this entity.
        /// Useful if you want to defer raising the generation events until after some checks are finished.
        /// </summary>
        // TODO implement!
        [DataField]
        public bool NoFireGeneratedEvent { get; set; } = false;

        /// <summary>
        /// Don't modify the entity's level based on things like the current map/area. 
        /// </summary>
        /// <remarks>
        /// In the HSP version this was called <c>noVoidLv</c>, and as the name implies, it disables
        /// the special level scaling applied to characters spawned in The Void.
        /// </remarks>
        [DataField]
        public bool NoLevelScaling { get; set; }

        /// <summary>
        /// Overrides the position searching type for the generated entity.
        /// This allows you to e.g. spawn characters on top of each other.
        /// </summary>
        [DataField]
        public PositionSearchType? PositionSearchType { get; set; }

        /// <summary>
        /// Convenience field for setting the <see cref="MetaDataComponent.IsMapSavable"/> flag on entity generation.
        /// </summary>
        [DataField]
        public bool IsMapSavable { get; set; } = true;
    }

    public sealed class CharaGenArgs : EntityGenArgs
    {
        /// <summary>
        /// Category of creature to spawn, as per <see cref="CreaturePackComponent"/>.
        /// TODO this should really be <see cref="TagPrototype"/> instead...
        /// </summary>
        [DataField]
        public string? Category { get; set; } 
    }

    public sealed class ItemGenArgs : EntityGenArgs
    {
        /// <summary>
        /// If true, this item should be treated as being generated in a shopkeeper's inventory.
        /// </summary>
        /// <remarks>
        /// Known effects in 1.22 when true:
        /// - Do not autoidentify with Sense Quality immediately upon creation.
        /// - No chance to generate unique items.
        /// - No artifact generation log for oracle. (implies <see cref="NoOracle"/>)
        /// - Can generate any kind of home deed.
        /// - Can generate cooked food in addition to raw food.
        /// </remarks>
        [DataField]
        public bool IsShop { get; set; } = false;

        /// <summary>
        /// If true, do not log this item in the oracle artifacts log if it is unique.
        /// </summary>
        [DataField]
        public bool NoOracle { get; set; } = false;

        /// <summary>
        /// Own state to generate this item/pickable with.
        /// </summary>
        [DataField]
        public OwnState OwnState { get; set; } = OwnState.None;
    }
}
