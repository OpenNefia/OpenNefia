﻿using OpenNefia.Content.Equipment;
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
            SubscribeComponent<EquipStatsComponent, ApplyEquipmentToEquipperEvent>(OnApplyToEquipper, priority: EventPriorities.High);
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
            equipperStats.DamageImmunityRate.Buffed += bonus.DamageImmunityRate.Buffed;
            equipperStats.DamageReflection.Buffed += bonus.DamageReflection.Buffed;
            equipperStats.ExtraMeleeAttackRate.Buffed += bonus.ExtraMeleeAttackRate.Buffed;
            equipperStats.ExtraRangedAttackRate.Buffed += bonus.ExtraRangedAttackRate.Buffed;
        }
    }
}
