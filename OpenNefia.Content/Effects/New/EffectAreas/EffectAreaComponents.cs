using OpenNefia.Content.Audio;
using OpenNefia.Core;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Casts a magic missile that strikes a single target from a distance.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaArrowComponent : Component
    {
    }

    /// <summary>
    /// Casts a bolt in a line that can hit multiple targets.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBoltComponent : Component
    {
        [DataField]
        public bool IgnoreFOV { get; set; } = false;
    }

    /// <summary>
    /// Casts a ball that hits all targets in an area of effect.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBallComponent : Component
    {
        /// <summary>
        /// If true, the caster's position will have the effect applied to it.
        /// Set to false if you intend to damage other entities.
        /// </summary>
        [DataField]
        public bool IncludeOriginPos { get; set; } = false;
    }

    /// <summary>
    /// Casts a spell that hits all targets in a cone.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaBreathComponent : Component
    {
        [DataField]
        public bool IncludeOriginPos { get; set; } = false;

        [DataField]
        public LocaleKey BreathNameKey { get; set; } = "Elona.Magic.Message.Breath.NoElement";
    }

    /// <summary>
    /// Affects a random spray of tiles around the origin.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaWebComponent : Component
    {
        /// <summary>
        /// Number of tiles to affect.
        /// </summary>
        [DataField]
        public Formula TileCount { get; set; } = new("power / 100");

        /// <summary>
        /// Controls the spread of the effect spray.
        /// </summary>
        [DataField]
        public Formula Spread { get; set; } = new("3");
    }

    /// <summary>
    /// Shows a message and plays a sound when the area effect is cast.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaMessageComponent : Component
    {
        [DataField]
        public LocaleKey MessageKey { get; set; } = "";

        [DataField]
        public SoundSpecifier? Sound { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaAnimationComponent : Component
    {
        [DataField]
        public bool ShowAnimation { get; set; } = true;

        [DataField]
        public Color? Color { get; set; }

        [DataField]
        public SoundSpecifier? Sound { get; set; } 
    }
}
