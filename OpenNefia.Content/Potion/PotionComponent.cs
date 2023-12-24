using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Effects.New;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Potion
{
    [RegisterComponent]
    public class PotionComponent : Component
    {
        [DataField]
        public IEffectSpecs Effects { get; set; } = new NullEffectSpec();
    }
}
