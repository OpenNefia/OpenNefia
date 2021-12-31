using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.VanillaAI
{
    /// <summary>
    /// Indicates that the AI should attempt to move into this entity even if
    /// it's solid. Used for things like doors.
    /// </summary>
    [RegisterComponent]
    public class AICanPassThroughComponent : Component
    {
        public override string Name => "AICanPassThrough";
    }
}