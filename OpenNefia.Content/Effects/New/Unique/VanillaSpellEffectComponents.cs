using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Spells;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Buffs;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New.Unique
{
    /// <summary>
    /// Mutates the target.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectMutationComponent : Component
    {
        /// <summary>
        /// Do not randomly pick from negative mutations.
        /// </summary>
        [DataField]
        public bool NoNegativeMutations { get; set; } = false;
    }

    /// <summary>
    /// Cures mutations.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectCureMutationComponent : Component
    {
    }

    /// <summary>
    /// Chance to recruit the target as an ally.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDominateComponent : Component
    {
    }

    /// <summary>
    /// Identifies a target item.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectIdentifyComponent : Component
    {
    }

    /// <summary>
    /// Uncurses a target item.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectUncurseComponent : Component
    {
    }


    /// <summary>
    /// Reveals generated artifact locations.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectOracleComponent : Component
    {
    }

    /// <summary>
    /// Creates a wall at the affected location.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectWallCreationComponent : Component
    {
    }

    /// <summary>
    /// Creates a wall at the affected location.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDoorCreationComponent : Component
    {
    }

    /// <summary>
    /// Prompts to resurrect dead allies.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectResurrectionComponent : Component
    {
    }

    /// <summary>
    /// Creates valuable items.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectWizardsHarvestComponent : Component
    {
    }

    /// <summary>
    /// Creates valuable items.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectRestoreComponent : Component
    {
        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Effect.Restore.Body";

        [DataField]
        public List<PrototypeId<SkillPrototype>> SkillsToRestore { get; set; } = new();
    }

    /// <summary>
    /// Prompts the player for a wish.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectWishComponent : Component
    {
    }

    /// <summary>
    /// Inflicts gravity.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectGravityComponent : Component
    {
    }

    /// <summary>
    /// Curses items in the target's inventory.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectCurseComponent : Component
    {
    }

    /// <summary>
    /// Teleports the player to a known location.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectReturnComponent : Component
    {
    }

    /// <summary>
    /// Teleports the player to the the outside of the current area.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectEscapeComponent : Component
    {
    }

    /// <summary>
    /// Reveals tiles in the map.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectMagicMapComponent : Component
    {
    }

    /// <summary>
    /// Reveals objects in the map.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectSenseObjectComponent : Component
    {
    }

    /// <summary>
    /// Opens a global inventory.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectFourDimensionalPocketComponent : Component
    {
        /// <summary>
        /// Global entity managed by <see cref="Content.GlobalEntities.IGlobalEntitySystem"/>.
        /// The items will be stored inside a container there.
        /// </summary>
        [DataField]
        public string GlobalEntityID { get; set; } = "Elona.FourDimensionalPocket";

        /// <summary>
        /// Prototype ID of the global container.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype>? ContainerEntity { get; set; } = new("Elona.ContainerFourDimensionalPocket");

        [DataField]
        public Formula? MaxTotalWeight { get; set; }

        [DataField]
        public Formula? MaxItemWeight { get; set; }

        [DataField]
        public Formula? MaxItemCount { get; set; }
    }

    /// <summary>
    /// Removes hexes.
    /// </summary>
    /// <remarks>
    /// Used by Holy Veil and Vanquish Hex.
    /// </remarks>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectRemoveHexComponent : Component
    {
        /// <summary>
        /// Maximum hexes to remove. If <c>null</c>, remove all hexes.
        /// </summary>
        [DataField]
        public int? MaxRemovedHexes { get; set; }
    }

    /// <summary>
    /// Adds a buff (blessing/hex) to the target.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectApplyBuffComponent : Component
    {
        /// <summary>
        /// Entity prototype ID of the buff to add.
        /// Must have a <see cref="BuffComponent"/>.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> BuffID { get; set; }
    }
}