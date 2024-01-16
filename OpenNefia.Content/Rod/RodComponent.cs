using OpenNefia.Content.Effects.New;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Rod
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class RodComponent : Component
    {
        [DataField]
        public IEffectSpecs Effects { get; set; } = new NullEffectSpec();
    }
}