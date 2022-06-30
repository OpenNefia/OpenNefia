using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public interface ISkillsSystem : IEntitySystem
    {
        #region Querying

        int Level(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null);
        int Level(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null);
        int BaseLevel(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null);
        int BaseLevel(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null);
        int Potential(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null);
        int Potential(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null);

        bool TryGetKnown(EntityUid uid, PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, SkillsComponent? skills = null);

        bool HasSkill(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null);
        bool HasSkill(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null);

        #endregion

        #region Prototypes

        /// <summary>
        /// Enumerates attributes, including luck and speed.
        /// </summary>
        IEnumerable<SkillPrototype> EnumerateAllAttributes();

        /// <summary>
        /// Enumerates attributes, excluding luck and speed.
        /// </summary>
        IEnumerable<SkillPrototype> EnumerateBaseAttributes();

        IEnumerable<SkillPrototype> EnumerateRegularSkills();

        IEnumerable<SkillPrototype> EnumerateWeaponProficiencies();

        SkillPrototype PickRandomBaseAttribute();

        #endregion

        #region Leveling

        void ModifyPotential(EntityUid uid, PrototypeId<SkillPrototype> skillId, int delta, SkillsComponent? skills = null);
        void GainFixedSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillId, int expGained, SkillsComponent? skills = null);
        void GainSkillExp(EntityUid uid, PrototypeId<SkillPrototype> skillId, int baseExpGained, int relatedSkillExpDivisor = 0, int levelExpDivisor = 0, SkillsComponent? skills = null);
        void GainSkillExp(EntityUid uid, SkillPrototype skill, int baseExpGained, int relatedSkillExpDivisor = 0, int levelExpDivisor = 0, SkillsComponent? skills = null);
        void GainSkill(EntityUid uid, PrototypeId<SkillPrototype> skillId, LevelAndPotential? initialValues = null,
            SkillsComponent? skills = null);
        void GainLevel(EntityUid entity, bool showMessage = false, SkillsComponent? skillComp = null, LevelComponent? levelComp = null);

        #endregion

        #region Operations

        void HealToMax(EntityUid uid, SkillsComponent? skills = null);

        void GainBonusPoints(EntityUid uid, int bonusPoints, SkillsComponent? skills = null);

        /// <summary>
        /// Applies the skill point bonus. Does not consume any skill points.
        /// </summary>
        void ApplyBonusPoint(EntityUid uid, PrototypeId<SkillPrototype> skillId, SkillsComponent? skills = null);
        void DamageHP(EntityUid uid, int amount, string source, bool showMessage = true, SkillsComponent? skills = null);
        void HealHP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealMP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealStamina(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);

        #endregion
    }

    public sealed partial class SkillsSystem : EntitySystem, ISkillsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        public override void Initialize()
        {
            SubscribeComponent<SkillsComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SkillsComponent, EntityGeneratedEvent>(HandleGenerated, priority: EventPriorities.VeryHigh);

            SubscribeComponent<SkillsComponent, EntityTurnStartingEventArgs>(HandleTurnStarting, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SkillsComponent, EntityTurnEndingEventArgs>(HandleTurnEnding, priority: EventPriorities.Low);
        }

        private void HandleGenerated(EntityUid uid, SkillsComponent component, ref EntityGeneratedEvent args)
        {
            _refresh.Refresh(uid);
            HealToMax(uid);
        }

        private void HandleRefresh(EntityUid uid, SkillsComponent skills, ref EntityRefreshEvent args)
        {
            var level = EntityManager.EnsureComponent<LevelComponent>(uid);

            ResetStatBuffs(skills);
            ResetSkillBuffs(skills);
            RefreshHPMPAndStamina(skills, level);
        }

        private void ResetStatBuffs(SkillsComponent skills)
        {
            skills.DV.Reset();
            skills.PV.Reset();
            skills.HitBonus.Reset();
            skills.DamageBonus.Reset();
        }

        private void ResetSkillBuffs(SkillsComponent skills)
        {
            foreach (var (_, level) in skills.Skills)
            {
                level.Level.Reset();
            }
        }

        public bool TryGetKnown(EntityUid uid, PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
            {
                level = null;
                return false;
            }

            return skills.TryGetKnown(protoId, out level);
        }

        public int Level(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null)
            => Level(uid, proto.GetStrongID(), skills);

        public int Level(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return 0;

            return skills.Level(protoId);
        }

        public int BaseLevel(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null)
            => BaseLevel(uid, proto.GetStrongID(), skills);

        public int BaseLevel(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return 0;

            return skills.BaseLevel(protoId);
        }

        public int Potential(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null)
            => Potential(uid, proto.GetStrongID(), skills);

        public int Potential(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return 0;

            return skills.Potential(protoId);
        }

        public bool HasSkill(EntityUid uid, SkillPrototype proto, SkillsComponent? skills = null)
            => HasSkill(uid, proto.GetStrongID(), skills);

        public bool HasSkill(EntityUid uid, PrototypeId<SkillPrototype> protoId, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return false;

            return skills.TryGetKnown(protoId, out _);
        }

        private void RefreshHPMPAndStamina(SkillsComponent skills, LevelComponent level)
        {
            var maxMPRaw = (skills.Level(Skill.AttrMagic) * 2
                         + skills.Level(Skill.AttrWill)
                         + skills.Level(Skill.AttrLearning) / 3)
                         * (level.Level / 25)
                         + skills.Level(Skill.AttrMagic);

            skills.MaxMP = Math.Clamp(maxMPRaw, 1, 1000000) * (skills.Level(Skill.AttrMana) / 100);
            skills.MaxMP = Math.Max(skills.MaxHP, 1);

            var maxHPRaw = (skills.Level(Skill.AttrConstitution) * 2
                         + skills.Level(Skill.AttrStrength)
                         + skills.Level(Skill.AttrWill) / 3)
                         * (level.Level / 25)
                         + skills.Level(Skill.AttrConstitution);

            skills.MaxHP = Math.Clamp(maxHPRaw, 1, 1000000) * (skills.Level(Skill.AttrLife) / 100) + 5;
            skills.MaxHP = Math.Max(skills.MaxHP, 1);

            // TODO traits
            skills.MaxStamina = 100 + (skills.Level(Skill.AttrConstitution) + skills.Level(Skill.AttrStrength)) / 5;
        }
    }
}
