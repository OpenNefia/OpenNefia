using JetBrains.Annotations;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Indicates what kind of entities this component should be used with.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IComponent))]
    public class ComponentUsageAttribute : Attribute
    {
        public ComponentTarget Target { get; }

        public ComponentUsageAttribute(ComponentTarget target)
        {
            Target = target;
        }
    }

    // TODO since mods can define arbitrary entity types anyway, this
    // is probably best represented with strings instead of bitflags
    public enum ComponentTarget
    {
        /// <summary>
        /// This component should be used with non-map/non-area entities.
        /// </summary>
        Normal = 0x1,

        /// <summary>
        /// This component should be used with map entities.
        /// </summary>
        Map = 0x2,

        /// <summary>
        /// This component should be used with area entities.
        /// </summary>
        Area = 0x4,

        /// <summary>
        /// This component should be used with enchantment entities.
        /// </summary>
        Enchantment = 0x8,

        /// <summary>
        /// This component should be used with quest entities.
        /// </summary>
        Quest = 0x10,

        /// <summary>
        /// This component should be used with encounter entities.
        /// </summary>
        Encounter = 0x20,

        /// <summary>
        /// This component should be used with weather entities.
        /// </summary>
        Weather = 0x40,

        /// <summary>
        /// This component should be used with effect entities.
        /// </summary>
        Effect = 0x80,

        /// <summary>
        /// This component should be used with buff entities.
        /// </summary>
        Buff = 0x100,
    }
}
