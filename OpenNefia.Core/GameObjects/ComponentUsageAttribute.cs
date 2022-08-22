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

    public enum ComponentTarget
    {
        /// <summary>
        /// This component should be used with non-map/non-area entities.
        /// </summary>
        Normal,

        /// <summary>
        /// This component should be used with map entities.
        /// </summary>
        Map,

        /// <summary>
        /// This component should be used with area entities.
        /// </summary>
        Area,

        /// <summary>
        /// This component should be used with enchantment entities.
        /// </summary>
        Enchantment
    }
}
