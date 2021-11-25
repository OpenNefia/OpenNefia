using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Maps;
using Why.Core.Random;
using Why.Core.Utility;

namespace Why.Core.GameObjects
{
    public class PregnantSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _random = default!;

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
            if (_random.OneIn(10))
            {
                Console.WriteLine("Suddenly an alien bursts from " + DisplayNameSystem.GetDisplayName(uid) + "'s stomach!");
                EntityManager.SpawnEntity("Putit", EntityManager.GetEntity(uid).Coords.Offset(1, 1));
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
