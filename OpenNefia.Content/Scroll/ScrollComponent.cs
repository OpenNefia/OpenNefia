using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Scroll
{
    [RegisterComponent]
    public sealed class ScrollComponent : Component
    {
        public override string Name => "Scroll";
        
        [DataField(required: true)]
        public IEffect Effect { get; } = new NullEffect();

        [DataField]
        public int EffectPower { get; set; } = 0;
        
        [DataField]
        public int AmountConsumedOnRead { get; set; } = 1;
    }
}