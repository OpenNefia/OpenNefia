using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.EmotionIcon
{
    public interface IEmotionIconSystem : IEntitySystem
    {
        void SetEmotionIcon(EntityUid entity, string? id, int turns = 0, EmotionIconComponent? emoicon = null);
    }

    public sealed class EmotionIconSystem : EntitySystem, IEmotionIconSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<EmotionIconComponent, EntityTurnStartingEventArgs>(UpdateEmotionIcon);
        }

        private void UpdateEmotionIcon(EntityUid uid, EmotionIconComponent emoIcon, EntityTurnStartingEventArgs args)
        {
            emoIcon.TurnsRemaining = Math.Max(emoIcon.TurnsRemaining - 1, 0);
            if (emoIcon.TurnsRemaining == 0)
                emoIcon.EmotionIconId = null;
        }

        public void SetEmotionIcon(EntityUid entity, string? id, int turns = 0, EmotionIconComponent? emoicon = null)
        {
            if (!Resolve(entity, ref emoicon))
                return;

            emoicon.EmotionIconId = id;
            emoicon.TurnsRemaining = turns;
        }
    }
}
