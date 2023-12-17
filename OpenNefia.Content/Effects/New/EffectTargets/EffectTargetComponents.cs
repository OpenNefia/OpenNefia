using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New
{
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
}
