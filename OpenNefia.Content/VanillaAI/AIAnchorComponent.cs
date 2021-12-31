using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.VanillaAI
{
    /// <summary>
    /// A component for making an entity's AI wait near a preset anchor.
    /// This is used for things like shopkeepers. It's also intended to be
    /// decoupled from the <see cref="VanillaAIComponent"/> in case other
    /// AI systems want to use it.
    /// </summary>
    [RegisterComponent]
    public class AIAnchorComponent : Component
    {
        public override string Name => "AIAnchor";

        /// <summary>
        /// Position in the map this entity should stay near.
        /// </summary>
        [DataField]
        public Vector2i Anchor { get; set; }
    }
}