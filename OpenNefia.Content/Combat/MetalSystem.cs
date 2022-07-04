using OpenNefia.Content.Damage;
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
    public sealed class MetalSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;

        public override void Initialize()
        {
            SubscribeComponent<MetalComponent, CalcFinalDamageEvent>(HandleCalcFinalDamage, priority: EventPriorities.VeryHigh + 30000);
        }

        private void HandleCalcFinalDamage(EntityUid uid, MetalComponent component, ref CalcFinalDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1464 	if cBit(cMetal,tc):dmg=rnd(dmg/10+2) ..
            if (component.IsMetal)
                args.OutFinalDamage = _rand.Next(args.OutFinalDamage / 10 + 2);
            // <<<<<<<< elona122/shade2/chara_func.hsp:1464 	if cBit(cMetal,tc):dmg=rnd(dmg/10+2) ..
        }
    }
}