using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Scroll
{
    [RegisterComponent]
    public sealed class ScrollComponent : Component
    {
        [DataField]
        public IEffectSpecs Effects { get; set; } = new NullEffectSpec();

        [DataField]
        public int AmountConsumedOnRead { get; set; } = 1;
    }
}