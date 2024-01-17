using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Effects.New.Unique
{
    /// <summary>
    /// Inflicts weakness.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTouchOfWeaknessComponent : Component
    {
    }

    /// <summary>
    /// Inflicts hunger.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectTouchOfHungerComponent : Component
    {
    }

    /// <summary>
    /// Cuts HP.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectManisDisassemblyComponent : Component
    {
    }

    /// <summary>
    /// Shows character information.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectMirrorComponent : Component
    {
    }

    /// <summary>
    /// Transforms target into a different type of character.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectChangeComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDrawChargeComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectRechargeComponent : Component
    {
        [DataField]
        public int RechargePowerCost { get; set; } = 0;

        [DataField]
        public Formula AddedCharges { get; set; } = new("1 + randInt(maxCharges / 2 + 1)");
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectMeleeAttackComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectEyeOfEtherComponent : Component
    {
        [DataField]
        public Formula AddedCorruption { get; set; } = new("100");
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectSuspiciousHandComponent : Component
    {
    }

    /// <summary>
    /// Used by Eye of Insanity.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDamageSanityComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectEyeOfDimnessComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectSuicideAttackComponent : Component
    {
        /// <summary>
        /// Targets that were hit in the bomb attack and should also explode.
        /// This occurs in ApplyEffectArea after the main area handlers.
        /// </summary>
        public HashSet<EntityUid> ChainBombTargets { get; set; } = new();
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectInsultComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDistantAttackComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectScavengeComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectVanishComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectPregnantComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectCheerComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectMewMewMewComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDecapitationComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectDropMineComponent : Component
    {
    }
}
