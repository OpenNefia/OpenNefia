using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Potion
{
    [RegisterComponent]
    public class PotionComponent : Component
    {
        public override string Name => "Potion";

        [DataField(required: true)]
        public IEffect Effect { get; set; } = new NullEffect();

        [DataField]
        public ImmutableEffectArgSet EffectArgs { get; set; } = new();
    }
}
