using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public class PregnantSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, ImpregnateEvent>(DoImpregnate);
            SubscribeLocalEvent<PregnantComponent, TurnStartEvent>(DoAlienBirth);
        }

        public void DoImpregnate(EntityUid uid, CharaComponent component, ImpregnateEvent args)
        {
            if (EntityManager.HasComponent<PregnantComponent>(uid))
                return;

            Console.WriteLine("An alien forces its way into " + DisplayNameSystem.GetDisplayName(uid) + "'s stomach!");
            EntityManager.AddComponent<PregnantComponent>(uid);
        }

        public void DoAlienBirth(EntityUid uid, PregnantComponent component, TurnStartEvent args)
        {
            if (Rand.OneIn(10))
            {
                Console.WriteLine("Suddenly an alien bursts from " + DisplayNameSystem.GetDisplayName(uid) + "'s stomach!");
                EntityManager.SpawnEntity("Putit", component.Owner.Spatial.Coords.Offset(1, 1));
            }
        }
    }

    public sealed class ImpregnateEvent : HandledEntityEventArgs
    {
        public ImpregnateEvent()
        {
        }
    }

    public sealed class TurnStartEvent : HandledEntityEventArgs
    {
        public TurnStartEvent()
        {
        }
    }
}
