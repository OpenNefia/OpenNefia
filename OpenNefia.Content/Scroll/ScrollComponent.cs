using OpenNefia.Content.Effects;
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

    [ImplicitDataDefinitionForInheritors]
    public interface IEffectSpecs
    {
        public IEnumerable<EffectSpec> EnumerateEffectSpecs();
    }

    public sealed class NullEffectSpec : IEffectSpecs
    {
        public IEnumerable<EffectSpec> EnumerateEffectSpecs()
        {
            yield break;
        }
    }

    public sealed class EffectSpec : IEffectSpecs
    {
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> ID { get; set; }

        [DataField]
        public int MaxRange { get; set; } = 1;

        [DataField]
        public int Power { get; set; } = 1;

        [DataField]
        public int SkillLevel { get; set; } = 0;

        public IEnumerable<EffectSpec> EnumerateEffectSpecs()
        {
            yield return this;
        }
    }
}