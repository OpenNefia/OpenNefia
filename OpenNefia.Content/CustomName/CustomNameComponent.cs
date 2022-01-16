using OpenNefia.Core.GameObjects;
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
        public string CustomName { get; set; } = string.Empty;
    }
}
