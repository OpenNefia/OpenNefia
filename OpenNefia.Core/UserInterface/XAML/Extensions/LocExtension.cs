using JetBrains.Annotations;
using OpenNefia.Core.Locale;

namespace OpenNefia.Core.UserInterface
{
    /// <summary>
    /// XAML extension for retrieving localized strings in XAML.
    /// This lets you do "{Loc 'Some.Locale.Namespace.Text'}" inline.
    /// </summary>
    /// <remarks>
    /// Something that is non-obvious: The XAML compiler will look
    /// for classes with names that end with "Extension" under the
    /// resolved namespaces to add support for extensions like these.
    /// (see <c>Namespaces.cs</c>)
    /// </remarks>
    // TODO: Code a XAML compiler transformer to remove references to this type at compile time.
    // And just replace them with the Loc.GetString() call.
    // TODO (OpenNefia): Make the compiler autogenerate a "relocalize" method for automatically
    // localizing everything when the current language is switched. (may conflict with the above)
    [PublicAPI]
    public sealed class LocExtension
    {
        public string Key { get; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public object ProvideValue()
        {
            return Loc.GetString(Key);
        }
    }
}
