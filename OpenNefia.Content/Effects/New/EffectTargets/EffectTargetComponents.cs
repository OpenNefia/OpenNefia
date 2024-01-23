using OpenNefia.Content.RandomGen;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
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
    /// <hsp>tgSelfOnly</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetSelfComponent : Component
    {
    }

    /// <summary>
    /// Can affect the caster, or someone next to them (via directional prompt)
    /// if a wand is being used.
    /// Fails if no character on tile. (Heal, Holy Veil)
    /// </summary>
    /// <hsp>tgSelf</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetSelfOrNearbyComponent : Component
    {
    }

    /// <summary>
    /// Can affect someone next to them (via directional prompt),
    /// Fails if no character on tile.  (Riding, Pickpocket)
    /// </summary>
    /// <hsp>tgDirection</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetNearbyComponent : Component
    {
    }

    /// <summary>
    /// Affects a ground position (Web, Create Wall)
    /// </summary>
    /// <hsp>tgLocation</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetPositionComponent : Component
    {
        [DataField]
        public bool AlwaysShowLine { get; set; } = false;
    }

    /// <summary>
    /// Casts in a cardinal direction (Teleport Other, Touch of Weakness)
    /// </summary>
    /// <hsp>tgDir</hsp>
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
    /// <hsp>tgEnemy</hsp>
    /// <hsp>tgBoth</hsp>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTargetOtherComponent : Component
    {
        [DataField]
        public bool CanTargetGround { get; set; } = false;
    }

    /// <summary>
    /// Special AI and targeting behavior for summoning related skills.
    /// - The AI won't try to spam this spell every turn (only 2/5 of the time).
    /// - If the player casts this spell there is no directional prompt; it always chooses the
    ///   player as the target location. On the other hand, the AI will only try to cast
    ///   the spell if it's directly next to the player
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectSummonComponent : Component
    {
        /// <summary>
        /// Number of entities to summon.
        /// Valid variables include the ones inside <see cref="EffectBaseDamageDiceComponent"/>.
        /// </summary>
        [DataField]
        public Formula SummonCount { get; set; } = new("3");

        [DataField]
        public LocaleKey MessageKey { get; set; } = "Elona.Magic.Message.Summon";
    }

    /// <summary>
    /// Summons characters. For use with <see cref="EffectSummonComponent"/>.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectSummonCharaComponent: Component
    {
        /// <summary>
        /// Set of character filters to choose from.
        /// One is randomly picked per summon.
        /// </summary>
        // TODO random weighted list.
        [DataField]
        public List<SummonCharaFilter> Choices { get; } = new();

        /// <summary>
        /// If <c>false</c>, the summon will not spawn characters with
        /// the same prototype ID as the caster.
        /// </summary>
        [DataField]
        public bool CanBeSameTypeAsCaster { get; set; } = false;
    }

    [DataDefinition]
    public sealed class SummonCharaFilter
    {
        [DataField]
        public CharaFilter CharaFilter { get; set; } = new();

        [DataField]
        public bool NoOverrideLevelAndQuality { get; set; } = false;
    }
}
