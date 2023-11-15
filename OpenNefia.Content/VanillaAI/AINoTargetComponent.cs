using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.VanillaAI
{
    /// <summary>
    /// Indicates the AI should not try to target this entity, even if it's deemed an enemy. 
    /// Also indicates that this entity should not search for targets itself.
    /// </summary>
    /// <remarks>
    /// Used for the escort in the Puppy Cave quest.
    /// </remarks>
    [RegisterComponent]
    public class AINoTargetComponent : Component
    {
    }
}