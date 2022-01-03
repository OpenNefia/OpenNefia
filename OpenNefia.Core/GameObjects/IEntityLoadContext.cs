namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Interface used to allow the map loader to override prototype data with map data.
    /// </summary>
    public interface IEntityLoadContext
    {
        /// <summary>
        /// If true, the serializer will always try to load prototype components.
        /// If false, the context can remove entity components.
        /// </summary>
        bool EnsurePrototypeComponents { get; }

        /// <summary>
        ///     Gets the serializer used to ExposeData a specific component.
        /// </summary>
        IComponent GetComponentData(string componentName, IComponent? protoData);

        /// <summary>
        ///     Gets extra component names that must also be instantiated on top of the ones defined in the prototype,
        ///     (and then deserialized with <see cref="GetComponentData"/>)
        /// </summary>
        IEnumerable<string> GetExtraComponentTypes();
    }
}
