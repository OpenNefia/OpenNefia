using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Resists;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Effects.New
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectComponent : Component
    {
    }


    // Possible magic locations:
    //   self (tgSelfOnly): only affects the caster
    //   self_or_nearby (tgSelf): can affect the caster or someone nearby, but only if a wand is being used. fails if no
    //     character on tile (heal, holy veil)
    //   nearby (tgDirection): can affect the caster or someone nearby, fails if no character on tile (touch, steal,
    //     dissasemble)
    //   location (tgLocation): affects a ground position (web, create wall)
    //   target_or_location (tgBoth): affects the currently targeted character or ground position (breaths, bolts)
    //   enemy (tgEnemy): affects the currently targeted character, prompts if friendly (most attack magic)
    //   other (tgOther): affects the currently targeted character (shadow step)
    //   direction (tgDir): casts in a cardinal direction (teleport other)

    /// <summary>
    /// Only affects the caster.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetSelfComponent : Component
    {
    }

    /// <summary>
    /// Can affect the caster or someone nearby, but only if a wand is being used. Fails if no
    /// character on tile (Heal, Holy Veil, Riding, Pickpocket)
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetSelfOrNearbyComponent : Component
    {
    }

    /// <summary>
    /// Affects a ground position (Web, Create Wall)
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetGroundComponent : Component
    {
    }

    /// <summary>
    /// Casts in a cardinal direction (Teleport Other, Touch of Weakness)
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetDirectionComponent : Component
    {
    }

    /// <summary>
    /// Affects the currently targeted character
    /// - Can prompt if friendly (most attack magic)
    /// - Can skip prompting (Shadow Step)
    /// - Can optionally be applied a ground position (arrows, bolts)
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetOtherComponent : Component
    {
        [DataField]
        public bool CanTargetFriendly { get; set; } = false;

        [DataField]
        public bool CanTargetGround { get; set; } = false;
    }

    public enum MagicAlignment
    {
        Positive,
        Neutral,
        Negative
    }

    /// <summary>
    /// Represents spells that can be listed in the casting menu.
    /// They have a casting difficulty and MP cost.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeMagicComponent : Component
    {
        /// <summary>
        /// Spell casting difficulty.
        /// </summary>
        [DataField]
        public int Difficulty { get; set; } = 0;

        /// <summary>
        /// MP cost to cast this spell.
        /// </summary>
        [DataField]
        public int MPCost { get; set; } = 0;

        /// <summary>
        /// Whether this spell has positive or negative effects.
        /// Affects the power of the spell if its corresponding item
        /// is blessed or cursed.
        /// </summary>
        [DataField]
        public MagicAlignment Alignment { get; set; } = MagicAlignment.Positive;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeActionComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTypeItemComponent : Component
    {
    }

    /// <summary>
    /// Casts a bolt in a line that can hit multiple targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBoltComponent : Component
    {
    }

    /// <summary>
    /// Casts a magic missile that strikes a single target from a distance.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaArrowComponent : Component
    {
    }

    /// <summary>
    /// Applies the effect directly to the target without any further
    /// checks or animations.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaDirectComponent : Component
    {
    }

    /// <summary>
    /// Casts a ball that hits all targets in an area of effect.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBallComponent : Component
    {
    }

    /// <summary>
    /// Casts a spell that hits all targets in a cone.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBreathComponent : Component
    {
    }

    /// <summary>
    /// Randomly selects damage based on dice.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectBaseDamageDiceComponent : Component
    {
    }

    /// <summary>
    /// Modifies damage based on the caster's Control Magic level.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageControlMagicComponent : Component
    {
    }

    /// <summary>
    /// Causes elemental damage to targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageElementalComponent : Component
    {
        [DataField]
        public PrototypeId<ElementPrototype> Element { get; set; } = Protos.Element.Fire;
    }
}
