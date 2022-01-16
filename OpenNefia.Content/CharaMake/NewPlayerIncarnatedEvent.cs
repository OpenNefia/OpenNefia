using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.CharaMake
{
    /// <summary>
    /// Raised just after the character creation process finishes successfully.
    /// </summary>
    public sealed class NewPlayerIncarnatedEvent
    {
        public EntityUid PlayerUid { get; }

        public NewPlayerIncarnatedEvent(EntityUid playerUid)
        {
            PlayerUid = playerUid;
        }
    }
}