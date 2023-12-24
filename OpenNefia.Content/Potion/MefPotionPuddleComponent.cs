using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Potion
{
    /// <summary>
    /// Puddle created by a thrown potion that spilled on the ground.
    /// Carries the same effects as the potion it spawned from.
    /// </summary>
    [RegisterComponent]
    public class MefPotionPuddleComponent : Component
    {
        [DataField]
        public IEffectSpecs Effects { get; set; } = new NullEffectSpec();
    }
}
