using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
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

namespace OpenNefia.Content.Buffs
{
    public sealed class VanillaBuffsSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<BuffsComponent, CalcFinalDamageEvent>(ProcContingency, priority: EventPriorities.Low);
        }

        private void ProcContingency(EntityUid uid, BuffsComponent component, ref CalcFinalDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1465 	if cBit(cContingency,tc):if cHp(tc)-dmg<=0:if cal ..
            if (TryComp<SkillsComponent>(uid, out var skills) && skills.HP - args.OutFinalDamage <= 0)
            {
                // TODO
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1465 	if cBit(cContingency,tc):if cHp(tc)-dmg<=0:if cal ..
        }
    }
}