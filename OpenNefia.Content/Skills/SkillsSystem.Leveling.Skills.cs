using OpenNefia.Analyzers;
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
        [Dependency] private readonly IVisibilitySystem _visibility = default!;
        
        public void ModifyPotential(EntityUid uid, PrototypeId<SkillPrototype> skillId, int delta, SkillsComponent? skills = null)
        {
            if (delta == 0 || !Resolve(uid, ref skills) || !_protos.TryIndex(skillId, out var skillProto) || !TryGetKnown(uid, skillId, out var level))
                return;

            ModifyPotential(uid, skillProto, level, delta);
        }

        public void ModifyPotential(EntityUid uid, SkillPrototype skillProto, int delta, SkillsComponent? skills = null)
            => ModifyPotential(uid, skillProto.GetStrongID(), delta, skills);
        
        public void GainFixedSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillID, int expGained,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills) || !TryGetKnown(uid, skillID, out var level))
                return;

            if (!_protos.TryIndex(skillID, out var skillProto))
            {
                Logger.WarningS("skill", $"No skill with ID {skillID} found.");
                return;
            }

            GainFixedSkillExp(uid, skillProto, level, expGained);
        }

        public void GainSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillID,
            int baseExpGained,
            int relatedSkillExpDivisor = 0,
            int levelExpDivisor = 0,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills) || !TryGetKnown(uid, skillID, out var level))
                return;

            if (!_protos.TryIndex(skillID, out var skillProto))
            {
                Logger.WarningS("skill", $"No skill with ID {skillID} found.");
                return;
            }

            GainSkillExp(uid, skillProto, level, baseExpGained, relatedSkillExpDivisor, levelExpDivisor);
        }

        public void GainSkillExp(EntityUid uid, SkillPrototype skill,
            int baseExpGained,
            int relatedSkillExpDivisor = 0,
            int levelExpDivisor = 0,
            SkillsComponent? skills = null)
            => GainSkillExp(uid, skill.GetStrongID(), baseExpGained, relatedSkillExpDivisor, levelExpDivisor, skills);

        public const int DefaultSkillPotential = 50;

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
                ModifyPotential(uid, skillId, DefaultSkillPotential);
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
}
