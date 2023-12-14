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
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Maps;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Pregnant;

namespace OpenNefia.Content.Pregnancy
{
    public class PregnancySystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public override void Initialize()
        {
            SubscribeComponent<BirthedAlienComponent, GetBaseNameEventArgs>(BirthedAlien_GetBaseName);
            SubscribeComponent<PregnancyComponent, EntityPassTurnEventArgs>(DoAlienBirth);
            SubscribeComponent<PregnancyComponent, BeforeVomitEvent>(VomitOutAlienChildren);
        }

        private void BirthedAlien_GetBaseName(EntityUid uid, BirthedAlienComponent component, ref GetBaseNameEventArgs args)
        {
            if (component.ParentName != null)
                args.OutBaseName = Loc.GetString("Elona.Pregnant.Name.ChildOf", ("entityName", component.ParentName));
            else
                args.OutBaseName = Loc.GetString("Elona.Pregnant.Name.AlienKid", ("basename", args.OutBaseName));
        }

        public void Impregnate(EntityUid uid)
        {
            var comp = EnsureComp<PregnancyComponent>(uid);
            if (comp.IsPregnant)
                return;

            _mes.Display(Loc.GetString("Elona.Pregnancy.Impregnated", ("entity", uid)), entity: uid);
            EnsureComp<PregnancyComponent>(uid).IsPregnant = true;
        }

        public void DoAlienBirth(EntityUid uid, PregnancyComponent component, EntityPassTurnEventArgs args)
        {
            if (args.Handled || !component.IsPregnant || !TryComp<TurnOrderComponent>(uid, out var turnOrder))
                return;

            if (turnOrder.TotalTurnsTaken % 25 != 0)
                return;

            if (_rand.OneIn(15))
            {
                _mes.Display(Loc.GetString("Elona.Pregnancy.PatsStomach", ("entity", uid)), entity: uid);
                _mes.Display(Loc.GetString("Elona.Pregnancy.SomethingIsWrong", ("entity", uid)), entity: uid);
            }

            if (TryMap(uid, out var map) && !HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) && _rand.OneIn(30))
            {
                _mes.Display(Loc.GetString("Elona.Pregnancy.SomethingBreaksOut", ("entity", uid)), entity: uid);
                _statusEffects.AddTurns(uid, Protos.StatusEffect.Bleeding, 15);

                var alienLevel = _levels.GetLevel(uid) / 2 + 1;
                var genArgs = EntityGenArgSet.Make(new EntityGenCommonArgs() { LevelOverride = alienLevel, NoLevelScaling = true });
                var alien = _charaGen.GenerateChara(uid, Protos.Chara.Alien, args: genArgs);
                if (IsAlive(alien))
                {
                    var birthedAlien = EnsureComp<BirthedAlienComponent>(alien.Value);
                    birthedAlien.ParentEntity = uid;
                    if (!HasComp<BirthedAlienComponent>(uid)) { }
                        birthedAlien.ParentName = _displayNames.GetBaseName(uid);
                }
            }
        }

        private void VomitOutAlienChildren(EntityUid uid, PregnancyComponent component, BeforeVomitEvent args)
        {
            EntityManager.RemoveComponent(uid, component);
            _mes.Display(Loc.GetString("Elona.Pregnancy.SpitsAlienChildren", ("entity", uid)), entity: uid);
        }
    }
}
