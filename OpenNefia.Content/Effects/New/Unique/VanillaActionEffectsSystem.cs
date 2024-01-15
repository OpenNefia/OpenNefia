using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
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
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Enchantments;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.UI;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Formulae;

namespace OpenNefia.Content.Effects.New.Unique
{
    public sealed class VanillaActionEffectsSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IVanillaEnchantmentsSystem _vanillaEnchantments = default!;
        [Dependency] private readonly IQualitySystem _qualities = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillAdjustsSystem _skillAdjusts = default!;
        [Dependency] private readonly IRefreshSystem _refreshes = default!;
        [Dependency] private readonly IHungerSystem _hungers = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectTouchOfWeaknessComponent, ApplyEffectDamageEvent>(Apply_TouchOfWeakness);
            SubscribeComponent<EffectTouchOfHungerComponent, ApplyEffectDamageEvent>(Apply_TouchOfHunger);
            SubscribeComponent<EffectManisDisassemblyComponent, ApplyEffectDamageEvent>(Apply_ManisDisassembly);
        }

        private void Apply_TouchOfWeakness(EntityUid uid, EffectTouchOfWeaknessComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1865 	if efId=actTouchWeaken{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            var attrID = _skills.PickRandomAttribute().GetStrongID();

            var proceed = true;

            if (_vanillaEnchantments.HasSustainEnchantment(args.InnerTarget.Value, attrID))
                proceed = false;

            if (_qualities.GetQuality(args.InnerTarget.Value) >= Quality.Great && _rand.OneIn(4))
                proceed = false;

            if (proceed)
            {
                var adjustment = _skillAdjusts.GetSkillAdjust(args.InnerTarget.Value, attrID);
                var diff = _skills.BaseLevel(args.InnerTarget.Value, attrID) - adjustment;
                if (diff > 0)
                {
                    diff = diff * args.CommonArgs.Power / 2000 + 1;
                    _skillAdjusts.SetSkillAdjust(args.InnerTarget.Value, attrID, adjustment - diff);
                }
                _mes.Display(Loc.GetString("Elona.Effect.TouchOfWeakness.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)), color: UiColors.MesPurple, entity: args.InnerTarget.Value);
                _refreshes.Refresh(args.InnerTarget.Value);
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1877 		} ...
        }

        private void Apply_TouchOfHunger(EntityUid uid, EffectTouchOfHungerComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1859 	if efId=actTouchHunger{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (TryComp<HungerComponent>(args.InnerTarget.Value, out var hunger))
            {
                hunger.Nutrition -= HungerSystem.HungerDecrementAmount * 100;
                _mes.Display(Loc.GetString("Elona.Effect.TouchOfHunger.Apply", ("source", args.Source), ("target", args.InnerTarget.Value)), color: UiColors.MesPurple, entity: args.InnerTarget.Value);
                _hungers.MakeHungry(args.InnerTarget.Value, hunger);
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1863 		} ...
        }

        private void Apply_ManisDisassembly(EntityUid uid, EffectManisDisassemblyComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1843 	if efId=actDisassemble{ ...
            if (args.Handled || !IsAlive(args.InnerTarget))
                return;

            if (args.AffectedTileIndex == 0)
                _mes.Display(Loc.GetString("Elona.Effect.ManisDisassembly.Dialog", ("source", args.Source), ("target", args.InnerTarget.Value)), entity: args.Source);

            if (TryComp<SkillsComponent>(args.InnerTarget.Value, out var skills))
            {
                skills.HP = skills.MaxHP / 12 + 1;
            }

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1847 		} ...
        }
    }
}