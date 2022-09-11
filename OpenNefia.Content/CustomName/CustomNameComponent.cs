using NLua;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.CustomName
{
    /// <summary>
    /// Indicates that this entity has a non-localizable custom name
    /// that should be used in place of the one in its entity prototype.
    /// </summary>
    [RegisterComponent]
    public class CustomNameComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "CustomName";

        [DataField]
        public string? CustomName { get; set; }

        /// <summary>
        /// Display the name like "Orland the putit" instead of just "Orland", where
        /// "putit" is the <see cref="MetaDataComponent.DisplayName"/>.
        /// </summary>
        [DataField]
        public bool ShowDisplayName { get; set; } = false;
    }
}
