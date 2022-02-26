using OpenNefia.Analyzers;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IVisibilitySystem _visibility = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IMessage _mes = default!;

        public static readonly IntRange PotentialRange = new(1, 400);

        public const float PotentialLevelingDecayRate = 0.9f;

        public void ModifyPotential(EntityUid uid, PrototypeId<SkillPrototype> skillId, int delta, SkillsComponent? skills = null)
        {
            if (delta == 0 || !Resolve(uid, ref skills) || !TryGetKnown(uid, skillId, out var skill))
                return;

            skill.Potential = PotentialRange.Clamp(skill.Potential + delta);
        }

        private int CalcRelatedSkillExp(int baseAmount, int expDivisor)
        {
            return baseAmount / (2 + expDivisor);
        }

        private int CalcSkillExpGain(int baseExpGained, int potential, int skillLevel, float growthBuff)
        {
            var expGained = baseExpGained * potential / (100 + skillLevel * 15);

            if (growthBuff > 0)
                expGained = (int)(expGained * (1.0f + growthBuff));

            return expGained;
        }

        private int CalcLevelExpGainFromSkillExpGain(int experienceToNext, int level, int skillExpGained, int levelExpDivisor)
        {
            return _rand.Next(experienceToNext * skillExpGained / 1000 / (level + levelExpDivisor) + 1) + _rand.Next(2);
        }

        /// <inheritdoc/>
        /// <hsp>#deffunc skillMod int skill, int chara, int exp</hsp>
        public void GainFixedSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillId, int expGained,
            SkillsComponent? skills = null)
        {
            if (!_protos.TryIndex(skillId, out var skillProto))
            {
                Logger.WarningS("skill", $"No skill with ID {skillId} found.");
                return;
            }

            if (expGained == 0 || !Resolve(uid, ref skills) || !TryGetKnown(uid, skillId, out var skill))
                return;

            var potential = skill.Potential;
            var newExp = skill.Experience + expGained;

            if (potential == 0)
                return;

            var levelDelta = ProcSkillLeveling(uid, skillProto, skill, newExp);

            var ev = new SkillExpGainedEvent(skillProto, expGained, expGained, levelDelta);
            RaiseLocalEvent(uid, ref ev);
        }

        /// <inheritdoc/>
        public void GainSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillId,
            int baseExpGained,
            int relatedSkillExpDivisor = 0,
            int levelExpDivisor = 0,
            SkillsComponent? skills = null)
        {
            // >>>>>>>> shade2/module.hsp:283 	#deffunc skillExp int sid,int c,int a,int refExpD ..

            if (!_protos.TryIndex(skillId, out var skillProto))
            {
                Logger.WarningS("skill", $"No skill with ID {skillId} found.");
                return;
            }

            if (baseExpGained == 0 || !Resolve(uid, ref skills) || !TryGetKnown(uid, skillId, out var skill))
                return;

            if (skillProto.RelatedSkill != null)
            {
                // [[[ Recursion alert! ]]]
                var relatedSkillExp = CalcRelatedSkillExp(baseExpGained, relatedSkillExpDivisor);
                GainSkillExp(uid, skillProto.RelatedSkill.Value, relatedSkillExp, skills: skills);
            }

            var baseLevel = skill.Level.Base;
            var potential = skill.Potential;

            if (potential == 0)
                return;

            int actualExpGained;

            if (baseExpGained > 0)
            {
                // TODO growth buffs
                var growthBuff = 0;
                actualExpGained = CalcSkillExpGain(baseExpGained, potential, baseLevel, growthBuff);
                if (actualExpGained == 0)
                {
                    if (_rand.OneIn(baseLevel / 10 + 1))
                        actualExpGained = 1;
                    else
                        return;
                }
            }
            else
            {
                actualExpGained = baseExpGained;
            }

            // TODO map experience divisor

            var applyLevelDivisor = skillProto.SkillType != SkillType.Attribute && skillProto.SkillType != SkillType.AttributeSpecial;
            if (actualExpGained > 0 && applyLevelDivisor && levelExpDivisor <= 1000)
            {
                if (EntityManager.TryGetComponent(uid, out LevelComponent levelComp))
                {
                    var levelExpGained = CalcLevelExpGainFromSkillExpGain(levelComp.ExperienceToNext, levelComp.Level, actualExpGained, levelExpDivisor);

                    levelComp.Experience += levelExpGained;

                    if (EntityManager.TryGetComponent(uid, out SleepExperienceComponent sleepExpComp))
                    {
                        sleepExpComp.SleepExperience += levelExpGained;
                    }
                }
            }

            var newExp = skill.Experience + actualExpGained;
            var levelDelta = ProcSkillLeveling(uid, skillProto, skill, newExp);

            var ev = new SkillExpGainedEvent(skillProto, baseExpGained, actualExpGained, levelDelta);
            RaiseLocalEvent(uid, ref ev);

            // <<<<<<<< shade2/module.hsp:349 	#defcfunc calcFame int c,int per ..
        }

        private int CalcNewPotentialFromLeveling(int potential, int levelDelta)
        {
            var newPotential = potential;

            if (levelDelta > 0)
            {
                for (int i = 0; i < levelDelta; i++)
                {
                    newPotential = PotentialRange.Clamp((int)(newPotential * PotentialLevelingDecayRate));
                }
            }
            else if (levelDelta < 0)
            {
                for (int i = 0; i > -levelDelta; i--)
                {
                    newPotential = PotentialRange.Clamp((int)(newPotential * (1.0f + (1.0f - PotentialLevelingDecayRate))) + 1);
                }
            }

            return newPotential;
        }

        private enum SkillChangeType
        {
            Increased,
            Decreased
        }

        private string GetSkillChangeText(EntityUid entity, SkillPrototype skillProto, SkillChangeType type)
        {
            string keySuffix;

            switch (type)
            {
                case SkillChangeType.Increased:
                default:
                    keySuffix = "OnIncrease";
                    break;
                case SkillChangeType.Decreased:
                    keySuffix = "OnDecrease";
                    break;
            }

            if (Loc.TryGetPrototypeString(skillProto, keySuffix, out var text, ("entity", entity)))
                return text;

            var skillName = Loc.GetPrototypeString(skillProto, "Name");
            return Loc.GetString($"Elona.Skill.Default.{keySuffix}", ("entity", entity), ("skillName", skillName));
        }

        private int ProcSkillLeveling(EntityUid uid, SkillPrototype skillProto, LevelAndPotential skill, int newExp)
        {
            if (newExp >= 1000)
            {
                var levelDelta = newExp / 1000;
                newExp %= 1000;
                var newLevel = skill.Level.Base + levelDelta;
                var newPotential = CalcNewPotentialFromLeveling(skill.Potential, levelDelta);

                skill.Level.Base = newLevel;
                skill.Potential = newPotential;
                skill.Experience = newExp;

                if (_visibility.IsInWindowFov(uid))
                {
                    var mesText = GetSkillChangeText(uid, skillProto, SkillChangeType.Increased);
                    var mesColor = UiColors.MesWhite;
                    var mesAlert = false;

                    if (_parties.IsInPlayerParty(uid))
                    {
                        Sounds.Play(Sound.Ding3);
                        mesColor = UiColors.MesGreen;
                        mesAlert = true;
                    }

                    _mes.Display(mesText, mesColor, alert: mesAlert);
                }

                _refresh.Refresh(uid);
                return levelDelta;
            }
            else if (newExp < 0)
            {
                var levelDelta = (-newExp / 1000 + 1);
                newExp = 1000 + newExp % 1000;
                if (skill.Level.Base - levelDelta < 1)
                {
                    levelDelta = Math.Max(skill.Level.Base - 1, 0);
                    if (skill.Level.Base == 1 && levelDelta == 0)
                    {
                        newExp = 0;
                    }
                }

                var newLevel = Math.Max(skill.Level.Base - levelDelta, 0);
                var newPotential = CalcNewPotentialFromLeveling(skill.Potential, -levelDelta);

                skill.Level.Base = newLevel;
                skill.Potential = newPotential;
                skill.Experience = newExp;

                if (levelDelta != 0 && _visibility.IsInWindowFov(uid))
                {
                    var mesText = GetSkillChangeText(uid, skillProto, SkillChangeType.Decreased);
                    _mes.Display(mesText, UiColors.MesRed, alert: true);
                }

                _refresh.Refresh(uid);
                return levelDelta;
            }
            else
            {
                skill.Experience = newExp;
                return 0;
            }
        }

        /// <inheritdoc/>
        public void GainSkill(EntityUid uid, PrototypeId<SkillPrototype> skillId, LevelAndPotential? initialValues = null, 
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            if (skills.HasSkill(skillId))
            {
                ModifyPotential(uid, skillId, 20);
                return;
            }

            var newSkill = new LevelAndPotential()
            {
                Level = initialValues?.Level ?? new(1),
                Potential = initialValues?.Potential ?? 0,
                Experience = initialValues?.Experience ?? 0
            };
            newSkill.Level.Base = Math.Max(newSkill.Level.Base, 1);

            skills.Skills[skillId] = newSkill;

            if (newSkill.Potential <= 0)
            {
                ModifyPotential(uid, skillId, 50);
            }

            _refresh.Refresh(uid);
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct SkillExpGainedEvent
    {
        public SkillPrototype Skill { get; }
        public int BaseExpGained { get; }
        public int ActualExpGained { get; }
        public int LevelDelta { get; }

        public SkillExpGainedEvent(SkillPrototype skillProto, int baseExpGained, int actualExpGained, int levelDelta)
        {
            Skill = skillProto;
            BaseExpGained = baseExpGained;
            ActualExpGained = actualExpGained;
            LevelDelta = levelDelta;
        }
    }
}
