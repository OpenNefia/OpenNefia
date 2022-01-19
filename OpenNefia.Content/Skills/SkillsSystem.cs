using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public interface ISkillsSystem : IEntitySystem
    {
        void HealToMax(EntityUid uid, SkillsComponent? skills = null);

        void GainBonusPoints(EntityUid uid, int bonusPoints, SkillsComponent? skill = null);

        int Level(SkillsComponent skills, PrototypeId<SkillPrototype> id);

        LevelAndPotential Ensure(SkillsComponent skills, PrototypeId<SkillPrototype> protoId);

        bool TryGetKnown(SkillsComponent skills, PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level);

        /// <summary>
        /// Enumerates attributes, including luck and speed.
        /// </summary>
        IEnumerable<SkillPrototype> EnumerateAllAttributes();
        
        /// <summary>
        /// Enumerates attributes, excluding luck and speed.
        /// </summary>
        IEnumerable<SkillPrototype> EnumerateBaseAttributes();
    }

    public sealed partial class SkillsSystem : EntitySystem, ISkillsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SkillsComponent, EntityRefreshEvent>(HandleRefresh, nameof(HandleRefresh),
                before: new[] { new SubId(typeof(EquipmentSystem), "HandleRefresh") });
            SubscribeLocalEvent<SkillsComponent, EntityGeneratedEvent>(HandleGenerated, nameof(HandleGenerated));
        }

        private void HandleGenerated(EntityUid uid, SkillsComponent component, ref EntityGeneratedEvent args)
        {
            _refresh.Refresh(uid);
            HealToMax(uid);
        }

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

        public LevelAndPotential Ensure(SkillsComponent skills, PrototypeId<SkillPrototype> protoId)
        {
            if (!skills.Skills.TryGetValue(protoId, out var level))
            {
                level = new LevelAndPotential()
                {
                    Level = new(0)
                };
                skills.Skills[protoId] = level;
            }
            return level;
        }

        public bool TryGetKnown(SkillsComponent skills, PrototypeId<SkillPrototype> protoId, [NotNullWhen(true)] out LevelAndPotential? level)
        {
            if (!skills.Skills.TryGetValue(protoId, out level))
            {
                return false;
            }

            if (level.Level.Base <= 0)
            {
                return false;
            }

            return true;
        }

        public int Level(SkillsComponent skills, PrototypeId<SkillPrototype> id)
        {
            if (!TryGetKnown(skills, id, out var level))
                return 0;

            return level.Level.Buffed;
        }

        private void RefreshHPMPAndStamina(SkillsComponent skills, LevelComponent level)
        {
            var maxMPRaw = (Level(skills, Skill.AttrMagic) * 2
                         + Level(skills, Skill.AttrWill)
                         + Level(skills, Skill.AttrLearning) / 3)
                         * (level.Level / 25)
                         + Level(skills, Skill.AttrMagic);

            skills.MaxMP = Math.Clamp(maxMPRaw, 1, 1000000) * (Level(skills, Skill.AttrMana) / 100);
            skills.MaxMP = Math.Max(skills.MaxHP, 1);

            var maxHPRaw = (Level(skills, Skill.AttrConstitution) * 2
                         + Level(skills, Skill.AttrStrength)
                         + Level(skills, Skill.AttrWill) / 3)
                         * (level.Level / 25)
                         + Level(skills, Skill.AttrConstitution);

            skills.MaxHP = Math.Clamp(maxHPRaw, 1, 1000000) * (Level(skills, Skill.AttrLife) / 100) + 5;
            skills.MaxHP = Math.Max(skills.MaxHP, 1);

            // TODO traits
            skills.MaxStamina = 100 + (Level(skills, Skill.AttrConstitution) + Level(skills, Skill.AttrStrength)) / 5;
        }
    }
}
