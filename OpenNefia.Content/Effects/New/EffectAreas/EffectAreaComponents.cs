using OpenNefia.Content.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Effects.New
{

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
    /// Casts a magic missile that strikes a single target from a distance.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaArrowComponent : Component
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

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Effect)]
    public sealed class EffectAreaAnimationComponent : Component
    {
        [DataField]
        public bool ShowAnimation { get; set; } = true;

        [DataField]
        public Color? Color { get; set; } = OpenNefia.Core.Maths.Color.White;

        [DataField]
        public SoundSpecifier? Sound { get; set; } 
    }
}
