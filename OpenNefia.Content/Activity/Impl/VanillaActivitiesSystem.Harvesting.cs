using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.UI;
using OpenNefia.Content.Weight;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly IPickableSystem _pickables = default!;

        private void Initialize_Harvesting()
        {
            SubscribeComponent<ActivityHarvestingComponent, CalcActivityTurnsEvent>(Harvesting_CalcTurns);
            SubscribeComponent<ActivityHarvestingComponent, OnActivityStartEvent>(Harvesting_OnStart);
            SubscribeComponent<ActivityHarvestingComponent, OnActivityPassTurnEvent>(Harvesting_OnPassTurn);
            SubscribeComponent<ActivityHarvestingComponent, OnActivityFinishEvent>(Harvesting_OnFinish);
            SubscribeComponent<ActivityHarvestingComponent, OnActivityCleanupEvent>(Harvesting_OnCleanup);
        }

        private void Harvesting_CalcTurns(EntityUid uid, ActivityHarvestingComponent component, CalcActivityTurnsEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:467 			cActionPeriod(cc)=10+limit(iWeight(ci)/(1+sSTR( ...
            if (!IsAlive(component.Item))
                return;

            var actor = args.Activity.Actor;
            var weight = CompOrNull<WeightComponent>(component.Item)?.Weight?.Buffed ?? 0;
            args.OutTurns = 10 + int.Clamp(weight / (1 + _skills.Level(actor, Prototypes.Protos.Skill.AttrStrength) * 10 + _skills.Level(actor, Protos.Skill.Gardening) * 40), 1, 100);
            // <<<<<<<< shade2/proc.hsp:467 			cActionPeriod(cc)=10+limit(iWeight(ci)/(1+sSTR( ..
        }

        private void Harvesting_OnStart(EntityUid activity, ActivityHarvestingComponent component, OnActivityStartEvent args)
        {
            if (!IsAlive(component.Item))
            {
                args.Cancel();
                return;
            }

            _inUse.SetItemInUse(args.Activity.Actor, component.Item);

            _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Activity.Start",
                ("actor", args.Activity.Actor),
                ("item", component.Item)));
        }

        private void Harvesting_OnPassTurn(EntityUid activity, ActivityHarvestingComponent component, OnActivityPassTurnEvent args)
        {
            // >>>>>>>> shade2/proc.hsp:487 		if gRowAct=rowActHarvest{ ...
            var actor = args.Activity.Actor;
            _inUse.SetItemInUse(actor, component.Item);

            if (_rand.OneIn(5))
                _skills.GainSkillExp(actor, Protos.Skill.Gardening, 20, 4);

            if (_rand.OneIn(6) && _rand.Next(55) > _skills.BaseLevel(actor, Protos.Skill.AttrStrength) + 25)
                _skills.GainSkillExp(actor, Protos.Skill.AttrStrength, 50);

            if (_rand.OneIn(8) && _rand.Next(55) > _skills.BaseLevel(actor, Protos.Skill.AttrConstitution) + 28)
                _skills.GainSkillExp(actor, Protos.Skill.AttrConstitution, 50);

            if (_rand.OneIn(10) && _rand.Next(55) > _skills.BaseLevel(actor, Protos.Skill.AttrWill) + 30)
                _skills.GainSkillExp(actor, Protos.Skill.AttrWill, 50);

            if (_rand.OneIn(4))
                _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Activity.Sound"), color: UiColors.MesBlue);
            // <<<<<<<< shade2/proc.hsp:497 			} ..
        }

        private void Harvesting_OnFinish(EntityUid activity, ActivityHarvestingComponent component, OnActivityFinishEvent args)
        {
            if (!IsAlive(component.Item))
                return;

            // >>>>>>>> shade2/proc.hsp:623 	if gRowAct=rowActHarvest{ ...
            var weight = CompOrNull<WeightComponent>(component.Item)?.Weight?.Buffed ?? 0;
            _mes.Display(Loc.GetString("Elona.Quest.Types.Harvest.Activity.Start",
                ("actor", args.Activity.Actor),
                ("item", component.Item),
                ("weight", UiUtils.DisplayWeight(weight))));
            _inUse.RemoveItemInUse(component.Item);
            _pickables.PickUp(args.Activity.Actor, component.Item);
            // <<<<<<<< shade2/proc.hsp:626 		} ..
        }

        private void Harvesting_OnCleanup(EntityUid activity, ActivityHarvestingComponent component, OnActivityCleanupEvent args)
        {
            _inUse.RemoveItemInUse(component.Item);
        }
    }
}
