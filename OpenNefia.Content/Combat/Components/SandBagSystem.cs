using OpenNefia.Content.Damage;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Combat
{
    public interface ISandBagSystem : IEntitySystem
    {
        void HangOnSandBag(EntityUid target, EntityUid sandBag);
        bool IsHungOnSandBag(EntityUid ent);
        void ReleaseFromSandBag(EntityUid uid);
    }

    public sealed class SandBagSystem : EntitySystem, ISandBagSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public override void Initialize()
        {
            SubscribeComponent<SandBaggedComponent, AfterDamageAppliedEvent>(ProcSandBag, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SandBaggedComponent, RunAIActionEvent>(BlockAIOnSandBag, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SandBaggedComponent, AfterDamageHPEvent>(ShowDialogAndDamageNumbers, priority: EventPriorities.Low);
            SubscribeComponent<SandBaggedComponent, AfterRecruitedAsAllyEvent>(HandleRecruited, priority: EventPriorities.VeryHigh);
        }

        private void ProcSandBag(EntityUid uid, SandBaggedComponent _, ref AfterDamageAppliedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1499 		if cBit(cSandBag,tc):cHp(tc)=cMhp(tc) ..
            if (TryComp<SkillsComponent>(uid, out var skills) && skills.HP < 0)
            {
                skills.HP = skills.MaxHP;
            }
            // <<<<<<<< shade2/chara_func.hsp:1499 		if cBit(cSandBag,tc):cHp(tc)=cMhp(tc) ..
        }

        private void BlockAIOnSandBag(EntityUid uid, SandBaggedComponent _, RunAIActionEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/ai.hsp:29 	if cBit(cSandBag,cc){ ...
            if (_rand.OneIn(30))
            {
                _mes.Display(Loc.GetString("Elona.SandBag.Dialog.TurnStart", ("entity", uid)), UiColors.MesTalk);
            }
            _vanillaAI.SetTarget(uid, null);
            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< shade2/ai.hsp:32 		} ..
        }

        private void ShowDialogAndDamageNumbers(EntityUid uid, SandBaggedComponent _, ref AfterDamageHPEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1769 	if cBit(cSandBag,tc):if sync(tc):txt "("+dmg+")"+ ..
            _mes.Display($"({args.FinalDamage}{Loc.Space})", entity: uid);

            if (_rand.OneIn(20))
                _mes.Display(Loc.GetString("Elona.SandBag.Dialog.Damage"), UiColors.MesTalk);

            if (_config.GetCVar(CCVars.MessageShowDamageNumbers) == DisplayDamageType.SandbagOnly)
            {
                _mes.Display($"({args.FinalDamage})", UiColors.MesYellow, noCapitalize: true, entity: uid);
            }
            // <<<<<<<< shade2/chara_func.hsp:1769 	if cBit(cSandBag,tc):if sync(tc):txt "("+dmg+")"+ ..
        }

        private void HandleRecruited(EntityUid uid, SandBaggedComponent component, AfterRecruitedAsAllyEvent args)
        {
            ReleaseFromSandBag(uid);
        }

        public bool IsHungOnSandBag(EntityUid ent)
        {
            return HasComp<SandBaggedComponent>(ent);
        }

        public void HangOnSandBag(EntityUid target, EntityUid sandBag)
        {
            // TODO
            EntityManager.AddComponent<SandBaggedComponent>(target);
            _refresh.Refresh(target);
        }

        public void ReleaseFromSandBag(EntityUid uid)
        {
            if (!TryComp<SandBaggedComponent>(uid, out var sandBagged))
                return;

            // TODO
            EntityManager.RemoveComponent(uid, sandBagged);
            _refresh.Refresh(uid);
        }
    }
}