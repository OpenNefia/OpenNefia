using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Effects.New
{
    /// <summary>
    /// Declares a set of effects that can be applied to a target.
    /// Each effect invocation can have its own max range, power, and skill level.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IEffectSpecs
    {
        /// <summary>
        /// Retrieves the effects to apply, The caller should set
        /// up the <see cref="EffectArgSet"/> for each effect
        /// and use <see cref="INewEffectSystem.Apply"/> to apply them.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<EffectSpec> EnumerateEffectSpecs();
    }

    /// <summary>
    /// Effect specification that does nothing. It is recommended
    /// to use this instead of marking an <see cref="IEffectSpecs"/>
    /// property as nullable.
    /// </summary>
    public sealed class NullEffectSpec : IEffectSpecs
    {
        public IEnumerable<EffectSpec> EnumerateEffectSpecs()
        {
            yield break;
        }
    }

    /// <summary>
    /// A single effect invocation with its own range and power.
    /// </summary>
    public sealed class EffectSpec : IEffectSpecs
    {
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> ID { get; set; }

        /// <summary>
        /// TODO make nullable and make range a property of
        /// effects? then retrieve range from effect prototype
        /// (not instance) when calculating base range
        /// </summary>
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
