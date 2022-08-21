using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Combat
{
    public class EquipStatsSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeComponent<EquipStatsComponent, EntityRefreshEvent>(OnRefresh, priority: EventPriorities.Highest);
            SubscribeComponent<EquipStatsComponent, ApplyEquipmentToEquipperEvent>(OnApplyToEquipper, priority: EventPriorities.High);
        }

        private void OnRefresh(EntityUid uid, EquipStatsComponent bonus, ref EntityRefreshEvent args)
        {
            bonus.DV.Reset();
            bonus.PV.Reset();
            bonus.HitBonus.Reset();
            bonus.DamageBonus.Reset();
            bonus.PierceRate.Reset();
            bonus.CriticalRate.Reset();
            bonus.DamageResistance.Reset();
        }

        private void OnApplyToEquipper(EntityUid uid, EquipStatsComponent bonus, ref ApplyEquipmentToEquipperEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Equipper, out EquipStatsComponent equipperStats))
                return;

            equipperStats.DV.Buffed += bonus.DV.Buffed;
            equipperStats.PV.Buffed += bonus.PV.Buffed;
            equipperStats.HitBonus.Buffed += bonus.HitBonus.Buffed;
            equipperStats.DamageBonus.Buffed += bonus.DamageBonus.Buffed;
            equipperStats.PierceRate.Buffed += bonus.PierceRate.Buffed;
            equipperStats.CriticalRate.Buffed += bonus.CriticalRate.Buffed;
            equipperStats.DamageResistance.Buffed += bonus.DamageResistance.Buffed;
        }
    }
}
