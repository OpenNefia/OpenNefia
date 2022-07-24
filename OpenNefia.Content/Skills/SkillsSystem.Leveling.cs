﻿using OpenNefia.Analyzers;
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
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Game;
using OpenNefia.Content.Feats;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IVisibilitySystem _visibility = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;

        public static readonly IntRange PotentialRange = new(1, 400);

        public const float PotentialLevelingDecayRate = 0.9f;

        public void ModifyPotential(EntityUid uid, PrototypeId<SkillPrototype> skillId, int delta, SkillsComponent? skills = null)
        {
            if (delta == 0 || !Resolve(uid, ref skills) || !TryGetKnown(uid, skillId, out var skill))
                return;

            skill.Potential = PotentialRange.Clamp(skill.Potential + delta);
        }

        public void ModifyPotential(EntityUid uid, SkillPrototype skillProto, int delta, SkillsComponent? skills = null)
            => ModifyPotential(uid, skillProto.GetStrongID(), delta, skills);

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
            RaiseEvent(uid, ref ev);
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

                    _levels.GainExperience(uid, levelExpGained, levelComp);
                }
            }

            var newExp = skill.Experience + actualExpGained;
            var levelDelta = ProcSkillLeveling(uid, skillProto, skill, newExp);

            var ev = new SkillExpGainedEvent(skillProto, baseExpGained, actualExpGained, levelDelta);
            RaiseEvent(uid, ref ev);
            // <<<<<<<< shade2/module.hsp:349 	#defcfunc calcFame int c,int per ..
        }

        public void GainSkillExp(EntityUid uid, SkillPrototype skill,
            int baseExpGained,
            int relatedSkillExpDivisor = 0,
            int levelExpDivisor = 0,
            SkillsComponent? skills = null)
            => GainSkillExp(uid, skill.GetStrongID(), baseExpGained, relatedSkillExpDivisor, levelExpDivisor, skills);

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
                        Sounds.Play(Protos.Sound.Ding3);
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

        public void ApplyEntityLevelUpGrowth(EntityUid entity, bool showMessage = false, SkillsComponent? skillsComp = null, LevelComponent? levelComp = null)
        {
            if (!Resolve(entity, ref skillsComp) || !Resolve(entity, ref levelComp))
                return;

            if (showMessage)
            {
                if (_gameSession.IsPlayer(entity))
                {
                    _mes.Display(Loc.GetString("Elona.Level.Gain.Player", ("entity", entity), ("level", levelComp.Level)), UiColors.MesGreen);
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Level.Gain.Other", ("entity", entity), ("level", levelComp.Level)), UiColors.MesGreen);
                }
            }

            var skillBonus = 5 + (100 + BaseLevel(entity, Protos.Skill.AttrLearning) + 10) / (300 + levelComp.Level + 15) + 1;

            if (_gameSession.IsPlayer(entity))
            {
                if (TryComp<FeatsComponent>(entity, out var feats))
                {
                    if (levelComp.Level % 5 == 0 && levelComp.MaxLevelReached < levelComp.Level && levelComp.Level < 50)
                    {
                        feats.NumberOfFeatsAcquirable++;
                    }

                    skillBonus += _feats.Level(entity, Protos.Feat.PermSkillPoint, feats);
                }
            }

            GainBonusPoints(entity, skillBonus, skillsComp);

            if (_feats.HasFeat(entity, Protos.Feat.PermChaosShape))
            {
                if (levelComp.Level < 37 && levelComp.Level % 3 == 0 && levelComp.MaxLevelReached < levelComp.Level)
                {
                    GainRandomEquipSlot(entity, showMessage: true, skillsComp: skillsComp);
                }
            }

            if (!_parties.IsInPlayerParty(entity))
            {
                GrowPrimarySkills(entity, skillsComp);
            }

            _refresh.Refresh(entity);
        }

        private PrototypeId<EquipSlotPrototype> GetRandomEquipSlot()
        {
            if (_rand.OneIn(7))
                return Protos.EquipSlot.Neck;
            if (_rand.OneIn(9))
                return Protos.EquipSlot.Back;
            if (_rand.OneIn(8))
                return Protos.EquipSlot.Hand;
            if (_rand.OneIn(4))
                return Protos.EquipSlot.Ring;
            if (_rand.OneIn(6))
                return Protos.EquipSlot.Hand;
            if (_rand.OneIn(5))
                return Protos.EquipSlot.Waist;
            if (_rand.OneIn(5))
                return Protos.EquipSlot.Leg;
            return Protos.EquipSlot.Head;
        }

        private void RefreshSpeedCorrection(EntityUid entity, SkillsComponent? skillsComp = null)
        {
            if (!Resolve(entity, ref skillsComp))
                return;

            // XXX: "blocked" body parts from things like ether disease still get counted
            // towards the speed penalty.
            var count = _equipSlots.GetEquipSlots(entity).Count();
            if (count > 13)
            {
                skillsComp.SpeedCorrection = (count - 13) + 5;
            }
            else
            {
                skillsComp.SpeedCorrection = 0;
            }
        }

        private void GainRandomEquipSlot(EntityUid entity, bool showMessage, SkillsComponent? skillsComp = null, EquipSlotsComponent? equipSlots = null)
        {
            if (!Resolve(entity, ref skillsComp) || !Resolve(entity, ref equipSlots))
                return;

            var equipSlot = GetRandomEquipSlot();
            if (!_equipSlots.TryAddEquipSlot(entity, equipSlot, out _, out _, equipSlots))
                Logger.WarningS("skills", $"Failed to add equip slot {equipSlot} to {entity}");

            if (showMessage)
            {
                var bodyPartName = Loc.GetPrototypeString(equipSlot, "Name");
                _mes.Display(Loc.GetString("Elona.Skill.Leveling.GainNewBodyPart", ("entity", entity), ("bodyPartName", bodyPartName)), UiColors.MesGreen);
            }

            RefreshSpeedCorrection(entity, skillsComp);
        }

        private void GrowPrimarySkills(EntityUid entity, SkillsComponent? skillsComp = null)
        {
            if (!Resolve(entity, ref skillsComp))
                return;
            
            void Grow(PrototypeId<SkillPrototype> skillId)
            {
                skillsComp!.Ensure(skillId).Level.Base += _rand.Next(3);
            }

            foreach (var attr in EnumerateAllAttributes())
            {
                Grow(attr.GetStrongID());
            }

            // Grow some skills available on all characters (by default: evasion, martial arts, bow)
            foreach (var growableSkill in _protos.EnumeratePrototypes<SkillPrototype>().Where(s => s.GrowOnLevelUp))
            {
                Grow(growableSkill.GetStrongID());
            }
        }
    }

    [ByRefEvent]
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
