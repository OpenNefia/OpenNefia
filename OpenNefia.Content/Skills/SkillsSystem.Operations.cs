using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        public const int BonusPointExperienceAmount = 400;

        public void HealToMax(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            skills.HP = skills.MaxHP;
            skills.MP = skills.MaxHP;
            skills.Stamina = skills.MaxStamina;
        }

        public void GainBonusPoints(EntityUid uid, int bonusPoints, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            bonusPoints = Math.Max(bonusPoints, 0);

            skills.BonusPoints += bonusPoints;
            skills.TotalBonusPointsEarned += bonusPoints;
        }

        public void ApplyBonusPoint(EntityUid uid, PrototypeId<SkillPrototype> skillId, SkillsComponent? skills = null)
        {
            // >>>>>>>> shade2/command.hsp:2737 		if sORG(csSkill,pc)=0:snd seFail1:goto *com_char ..
            GainSkillExp(uid, skillId, BonusPointExperienceAmount, skills: skills);
            ModifyPotential(uid, skillId, Math.Clamp(15 - Potential(uid, skillId) / 15, 2, 15), skills);
            _refresh.Refresh(uid);
            // <<<<<<<< shade2/command.hsp:2743 		goto *com_charaInfo_loop ..
        }
    }
}
