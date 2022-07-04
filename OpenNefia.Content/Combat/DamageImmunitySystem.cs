using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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

namespace OpenNefia.Content.Combat
{
    public sealed class DamageImmunitySystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeComponent<DamageImmunityComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.Highest);
            SubscribeComponent<DamageImmunityComponent, CalcFinalDamageEvent>(HandleCalcFinalDamage, priority: EventPriorities.VeryLow);
        }

        private void HandleRefresh(EntityUid uid, DamageImmunityComponent component, ref EntityRefreshEvent args)
        {
            component.DamageImmunityChance.Reset();
        }

        private void HandleCalcFinalDamage(EntityUid uid, DamageImmunityComponent component, ref CalcFinalDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1466 	if cImmuneDamage(tc)>0:if cImmuneDamage(tc)>rnd(1 ..
            if (_rand.Prob(component.DamageImmunityChance.Buffed))
                args.OutFinalDamage = 0;
            // <<<<<<<< elona122/shade2/chara_func.hsp:1466 	if cImmuneDamage(tc)>0:if cImmuneDamage(tc)>rnd(1 ..
        }
    }
}