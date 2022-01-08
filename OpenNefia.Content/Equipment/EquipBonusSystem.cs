using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Equipment
{
    public class EquipBonusSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<EquipBonusComponent, EntityRefreshEvent>(OnRefresh, nameof(OnRefresh));
            SubscribeLocalEvent<EquipBonusComponent, ApplyEquipmentToEquipperEvent>(OnApplyToEquipper, nameof(OnApplyToEquipper));
        }

        private void OnRefresh(EntityUid uid, EquipBonusComponent equipBonus, ref EntityRefreshEvent args)
        {
            equipBonus.DV.Reset();
            equipBonus.PV.Reset();
            equipBonus.HitBonus.Reset();
            equipBonus.DamageBonus.Reset();
        }

        private void OnApplyToEquipper(EntityUid uid, EquipBonusComponent component, ref ApplyEquipmentToEquipperEvent args)
        {
            if (!EntityManager.TryGetComponent(args.Equipper, out SkillsComponent equipperSkills))
                return;

            equipperSkills.DV.Buffed += component.DV.Buffed;
            equipperSkills.PV.Buffed += component.PV.Buffed;
            equipperSkills.HitBonus.Buffed += equipperSkills.HitBonus.Buffed;
            equipperSkills.DamageBonus.Buffed += equipperSkills.DamageBonus.Buffed;
        }
    }
}
