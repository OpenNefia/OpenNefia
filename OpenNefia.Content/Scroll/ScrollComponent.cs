using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Scroll
{
    [RegisterComponent]
    public sealed class ScrollComponent : Component
    {        
        [DataField(required: true)]
        public IEffect Effect { get; } = new NullEffect();

        [DataField]
        public ImmutableEffectArgSet EffectArgs { get; set; } = new();
        
        [DataField]
        public int AmountConsumedOnRead { get; set; } = 1;
    }
}