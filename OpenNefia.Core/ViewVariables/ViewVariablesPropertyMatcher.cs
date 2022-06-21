namespace OpenNefia.Core.ViewVariables
{
    /// <summary>
    /// Finds an appropriate <see cref="VVPropEditor"/> for a given
    /// field/property type. This is to allow for custom VV property editors.
    /// </summary>
    public abstract class ViewVariablesPropertyMatcher
    {
        public abstract VVPropEditor? PropEditorFor(Type type);
    }
}