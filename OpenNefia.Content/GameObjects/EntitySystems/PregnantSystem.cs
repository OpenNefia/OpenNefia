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
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Charas;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Hunger;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Logic;

namespace OpenNefia.Content.GameObjects
{
    public class PregnantSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, ImpregnateEvent>(HandleImpregnate);
            SubscribeComponent<PregnantComponent, TurnStartEvent>(DoAlienBirth);
            SubscribeComponent<PregnantComponent, BeforeVomitEvent>(VomitOutAlienChildren);
        }

        public void HandleImpregnate(EntityUid uid, CharaComponent component, ImpregnateEvent args)
        {
            if (EntityManager.HasComponent<PregnantComponent>(uid))
                return;

            Console.WriteLine("An alien forces its way into " + _displayNames.GetDisplayName(uid) + "'s stomach!");
            EntityManager.AddComponent<PregnantComponent>(uid);
        }

        public void DoAlienBirth(EntityUid uid, PregnantComponent component, TurnStartEvent args)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            if (_rand.OneIn(10))
            {
                // TODO
                Console.WriteLine("Suddenly an alien bursts from " + _displayNames.GetDisplayName(uid) + "'s stomach!");
                _charaGen.GenerateChara(spatial.MapPosition.Offset(1, 1), id: Protos.Chara.Alien);
            }
        }

        private void VomitOutAlienChildren(EntityUid uid, PregnantComponent component, BeforeVomitEvent args)
        {
            EntityManager.RemoveComponent(uid, component);
            _mes.Display(Loc.GetString("Elona.Pregnant.SpitsAlienChildren", ("entity", uid)), entity: uid);
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
