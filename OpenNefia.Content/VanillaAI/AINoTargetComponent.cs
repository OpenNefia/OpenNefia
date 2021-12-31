using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.VanillaAI
{
    /// <summary>
    /// Indicates the AI should not try to target this entity, even
    /// if it's deemed an enemy. Also indicates that this entity
    /// should not target anything itself.
    /// </summary>
    /// <remarks>
    /// Used for the escort in the Puppy Cave quest.
    /// </remarks>
    [RegisterComponent]
    public class AINoTargetComponent : Component
    {
        public override string Name => "AINoTarget";
    }
}