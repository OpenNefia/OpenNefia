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
using OpenNefia.Content.Pregnancy;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.BaseAnim;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Pregnancy
{
    public interface IPregnancySystem : IEntitySystem
    {
        int CalcChildLevel(EntityUid uid);
        bool Impregnate(EntityUid uid, CharaFilter? filter = null);
        bool Impregnate(EntityUid uid, PrototypeId<EntityPrototype> charaID);
        CharaFilter MakeDefaultChildCharaFilter(EntityUid uid, PrototypeId<EntityPrototype> charaID);
    }

    public class PregnancySystem : EntitySystem, IPregnancySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;

        public override void Initialize()
        {
            SubscribeComponent<BirthedFromPregnancyComponent, GetBaseNameEventArgs>(BirthedFromPregnancy_GetBaseName);
            SubscribeComponent<PregnancyComponent, EntityPassTurnEventArgs>(ProcAlienBirth);
            SubscribeComponent<PregnancyComponent, BeforeVomitEvent>(VomitOutAlienChildren);
        }

        private void BirthedFromPregnancy_GetBaseName(EntityUid uid, BirthedFromPregnancyComponent component, ref GetBaseNameEventArgs args)
        {
            if (component.ParentName != null)
                args.OutBaseName = Loc.GetString("Elona.Pregnant.Name.ChildOf", ("entityName", component.ParentName));
            else
                args.OutBaseName = Loc.GetString("Elona.Pregnant.Name.AlienKid", ("basename", args.OutBaseName));
        }

        public int CalcChildLevel(EntityUid uid)
        {
            return _levels.GetLevel(uid) / 2 + 1;
        }

        public bool Impregnate(EntityUid uid, PrototypeId<EntityPrototype> charaID)
        {
            return Impregnate(uid, MakeDefaultChildCharaFilter(uid, charaID));
        }

        public bool Impregnate(EntityUid uid, CharaFilter? filter = null)
        {
            var preg = EnsureComp<PregnancyComponent>(uid);
            if (preg.IsPregnant)
                return false;

            if (preg.IsProtectedFromPregnancy.Buffed)
            {
                _mes.Display(Loc.GetString("Elona.Pregnant.Protected", ("entity", uid)), entity: uid);
                return false;
            }

            var anim = new BasicAnimMapDrawable(Protos.BasicAnim.AnimSmoke);
            _mapDrawables.Enqueue(anim, uid);

            _mes.Display(Loc.GetString("Elona.Pregnancy.Impregnated", ("entity", uid)), color: UiColors.MesYellow, entity: uid);
            preg.IsPregnant = true;

            if (filter != null)
            {
                preg.ChildFilter = filter;
            }
            else
            {
                preg.ChildFilter = MakeDefaultChildCharaFilter(uid, Protos.Chara.Alien);
            }

            return true;
        }

        public CharaFilter MakeDefaultChildCharaFilter(EntityUid uid, PrototypeId<EntityPrototype> charaID)
        {
            return new CharaFilter()
            {
                Id = charaID,
                Args = EntityGenArgSet.Make(new EntityGenCommonArgs()
                {
                    LevelOverride = CalcChildLevel(uid),
                    NoLevelScaling = true
                })
            };
        }

        private void ProcAlienBirth(EntityUid uid, PregnancyComponent preg, EntityPassTurnEventArgs args)
        {
            if (args.Handled || !preg.IsPregnant)
                return;

            if (args.TurnOrder.TotalTurnsTaken % 25 != 0)
                return;

            if (_rand.OneIn(15))
            {
                _mes.Display(Loc.GetString("Elona.Pregnancy.PatsStomach", ("entity", uid)), entity: uid);
                _mes.Display(Loc.GetString("Elona.Pregnancy.SomethingIsWrong", ("entity", uid)), entity: uid);
            }

            if (_rand.OneIn(30))
            {
                BirthAlienChild(uid, preg);
            }
        }

        private void BirthAlienChild(EntityUid uid, PregnancyComponent preg)
        {
            if (!TryMap(uid, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                return;

            _mes.Display(Loc.GetString("Elona.Pregnancy.SomethingBreaksOut", ("entity", uid)), entity: uid);
            _statusEffects.AddTurns(uid, Protos.StatusEffect.Bleeding, 15);

            var alien = _charaGen.GenerateChara(uid, preg.ChildFilter);

            if (IsAlive(alien))
            {
                var birthedAlien = EnsureComp<BirthedFromPregnancyComponent>(alien.Value);
                birthedAlien.ParentEntity = uid;
                birthedAlien.ParentName = _displayNames.GetBaseName(uid);
            }
        }

        public void VomitOutAlienChildren(EntityUid uid, PregnancyComponent component, BeforeVomitEvent args)
        {
            component.IsPregnant = false;
            _mes.Display(Loc.GetString("Elona.Pregnancy.SpitsAlienChildren", ("entity", uid)), entity: uid);
        }
    }
}
