using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.GameObjects
{
    public class PregnantSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, ImpregnateEvent>(HandleImpregnate, nameof(HandleImpregnate));
            SubscribeLocalEvent<PregnantComponent, TurnStartEvent>(DoAlienBirth, nameof(DoAlienBirth));
        }

        public void HandleImpregnate(EntityUid uid, CharaComponent component, ImpregnateEvent args)
        {
            if (EntityManager.HasComponent<PregnantComponent>(uid))
                return;

            Console.WriteLine("An alien forces its way into " + DisplayNameSystem.GetDisplayName(uid) + "'s stomach!");
            EntityManager.AddComponent<PregnantComponent>(uid);
        }

        public void DoAlienBirth(EntityUid uid, PregnantComponent component, TurnStartEvent args)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            if (_rand.OneIn(10))
            {
                Console.WriteLine("Suddenly an alien bursts from " + DisplayNameSystem.GetDisplayName(uid) + "'s stomach!");
                EntityManager.SpawnEntity(Protos.Chara.Putit, spatial.MapPosition.Offset(1, 1));
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
