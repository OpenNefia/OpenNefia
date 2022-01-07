using System;
using System.Security.Cryptography;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Skills
{
    public interface ISkillsSystem : IEntitySystem
    {
        void HealToMax(EntityUid uid, SkillsComponent? skills = null);
    }

    public class SkillsSystem : EntitySystem, ISkillsSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SkillsComponent, EntityRefreshEvent>(HandleRefresh, nameof(HandleRefresh));
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

        private void RefreshHPMPAndStamina(SkillsComponent skills, LevelComponent level)
        {
            var maxMPRaw = (skills.Level(Skill.StatMagic) * 2
                         + skills.Level(Skill.StatWill)
                         + skills.Level(Skill.StatLearning) / 3)
                         * (level.Level / 25)
                         + skills.Level(Skill.StatMagic);

            skills.MaxMP = Math.Clamp(maxMPRaw, 1, 1000000) * (skills.Level(Skill.StatMana) / 100);
            skills.MaxMP = Math.Max(skills.MaxHP, 1);

            var maxHPRaw = (skills.Level(Skill.StatConstitution) * 2
                         + skills.Level(Skill.StatStrength)
                         + skills.Level(Skill.StatWill) / 3)
                         * (level.Level / 25)
                         + skills.Level(Skill.StatConstitution);

            skills.MaxHP = Math.Clamp(maxHPRaw, 1, 1000000) * (skills.Level(Skill.StatLife) / 100) + 5;
            skills.MaxHP = Math.Max(skills.MaxHP, 1);

            // TODO traits
            skills.MaxStamina = 100 + (skills.Level(Skill.StatConstitution) + skills.Level(Skill.StatStrength)) / 5;
        }
    }
}
