namespace OpenNefia.Core.ViewVariables
{
    internal interface IViewVariablesManagerInternal : IViewVariablesManager
    {
        /// <summary>
        ///     Creates the ideal property editor for a specific property type.
        /// </summary>
        /// <param name="type">The type of the property to create an editor for.</param>
        VVPropEditor PropertyFor(Type? type);

        /// <summary>
        ///     Gets a collection of trait IDs that are agreed upon so <see cref="ViewVariablesInstanceObject"/> knows which traits to instantiate.
        /// </summary>
        /// <seealso cref="ViewVariablesBlobMetadata.Traits" />
        /// <seealso cref="ViewVariablesManagerShared.TraitIdsFor"/>
        ICollection<Type> TraitIdsFor(Type type);
    }
}
