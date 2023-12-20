using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Skills;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
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
}