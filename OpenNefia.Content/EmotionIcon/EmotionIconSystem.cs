using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.EmotionIcon
{
    public interface IEmotionIconSystem : IEntitySystem
    {
        void SetEmotionIcon(EntityUid entity, string id, GameTimeSpan turns, EmotionIconComponent? emoicon = null);
    }

    public sealed class EmotionIconSystem : EntitySystem, IEmotionIconSystem
    {
        public void SetEmotionIcon(EntityUid entity, string id, GameTimeSpan turns, EmotionIconComponent? emoicon = null)
        {
            if (!Resolve(entity, ref emoicon))
                return;

            emoicon.EmotionIconId = id;
            emoicon.TimeRemaining = turns;
        }
    }
}
