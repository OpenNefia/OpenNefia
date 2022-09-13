using OpenNefia.Core.Serialization.Markdown.Mapping;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Interface used to allow the map loader to override prototype data with map data.
    /// </summary>
    public interface IEntityLoadContext
    {
        /// <summary>
        ///     Gets the serializer used to ExposeData a specific component.
        /// </summary>
        MappingDataNode GetComponentData(string componentName, MappingDataNode? protoData);

        /// <summary>
        ///     Gets extra component names that must also be instantiated on top of the ones defined in the prototype,
        ///     (and then deserialized with <see cref="GetComponentData"/>)
        /// </summary>
        IEnumerable<string> GetExtraComponentTypes();

        /// <summary>
        /// If false, don't load this component during entity load.
        /// </summary>
        bool ShouldLoadComponent(string componentName);
    }
}
