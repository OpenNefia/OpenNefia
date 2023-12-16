using OpenNefia.Content.Combat;
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
using OpenNefia.Content.Levels;

namespace OpenNefia.Content.Skills
{
    public sealed class VanillaSkillsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;

        public void ForceInitialLevelAnd100Potential(SkillPrototype proto, ref P_SkillCalcFinalInitialLevelAndPotentialEvent ev)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:953 	if (sId=rsHP)or(sID=rsLUC)or(sId=rsMP)  : p(1)=a  ...
            ev.OutInitialLevel = ev.BaseLevel;
            ev.OutInitialPotential = 100;
            // <<<<<<<< elona122/shade2/calculation.hsp:953 	if (sId=rsHP)or(sID=rsLUC)or(sId=rsMP)  : p(1)=a  ...
        }

        #region Elona.AttrSpeed

        public void AttrSpeed_CalcInitialLevel(SkillPrototype proto, ref P_SkillCalcInitialLevelEvent ev)
        {
            // >>>>>>>> shade2/calculation.hsp:954 	if sId=rsSPD : p(1)=a*(100+cLevel(c)*2)/100 : els ..
            var charaLevel = _levels.GetLevel(ev.Entity);
            ev.OutInitialLevel = ev.BaseLevel * (100 + charaLevel * 2) / 100;
            // <<<<<<<< shade2/calculation.hsp:954 	if sId=rsSPD : p(1)=a*(100+cLevel(c)*2)/100 : els ..
        }

        #endregion

        #region Elona.MartialArts

        public void MartialArts_CalcAttackStrength(SkillPrototype proto, ref P_SkillCalcAttackStrengthEvent ev)
        {
            var relatedSkill = Protos.Skill.AttrStrength;
            var diceBonus = _skills.Level(ev.Attacker, relatedSkill) / 8
                + _skills.Level(ev.Attacker, Protos.Skill.MartialArts) / 8
                + CompOrNull<EquipStatsComponent>(ev.Attacker)?.DamageBonus.Buffed ?? 0;
            var diceX = 2;
            var diceY = _skills.Level(ev.Attacker, Protos.Skill.MartialArts) / 8 + 5;
            var multiplier = 0.5f + (_skills.Level(ev.Attacker, relatedSkill) 
                + _skills.Level(ev.Attacker, Protos.Skill.MartialArts)) / 5.0f
                + _skills.Level(ev.Attacker, Protos.Skill.Tactics)
                / 40.0f;
            var pierceRate = Math.Clamp(_skills.Level(ev.Attacker, Protos.Skill.MartialArts) / 5, 5, 50);

            ev.OutStrength = new(new Dice(diceX, diceY, diceBonus), multiplier, pierceRate);
        }

        public void MartialArts_CalcCriticalDamage(SkillPrototype proto, ref P_SkillCalcCriticalDamageEvent ev)
        {
            ev.OutStrength.Multiplier *= 1.25f;
        }

        #endregion
    }
}