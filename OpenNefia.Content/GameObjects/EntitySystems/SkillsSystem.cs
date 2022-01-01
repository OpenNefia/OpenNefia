using System;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.GameObjects
{
    public class SkillsSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(OnStartup, nameof(OnStartup));
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(OnStartup, nameof(OnStartup));
            SubscribeLocalEvent<SkillsComponent, EntityRefreshEvent>(OnRefresh, nameof(OnRefresh));
        }

        private void OnStartup(EntityUid uid, CharaComponent component, ref EntityGeneratedEvent args)
        {
            InitRaceSkills(uid, component);
            InitClassSkills(uid, component);
        }

        private void InitRaceSkills(EntityUid uid, CharaComponent chara,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            foreach (var pair in _protos.Index(chara.Race).BaseSkills)
            {
                skills.Skills[pair.Key] = new LevelAndPotential()
                {
                    Level = pair.Value,
                    Experience = 0
                };
            }
        }

        private void InitClassSkills(EntityUid uid, CharaComponent chara,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            foreach (var pair in _protos.Index(chara.Class).BaseSkills)
            {
                skills.Skills[pair.Key] = new LevelAndPotential()
                { 
                    Level = pair.Value,
                    Experience = 0
                };
            }
        }

        private void OnRefresh(EntityUid uid, SkillsComponent skills, ref EntityRefreshEvent args)
        {
            var level = EntityManager.EnsureComponent<LevelComponent>(uid);

            ResetSkillBuffs(skills);
            RefreshHPMPAndStamina(skills, level);
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
                         + (skills.Level(Skill.StatLearning) / 3))
                         * (level.Level / 25)
                         + skills.Level(Skill.StatMagic);

            skills.MaxMP = Math.Clamp(maxMPRaw, 1, 1000000) * (skills.Level(Skill.StatMana) / 100);
            skills.MaxMP = Math.Max(skills.MaxHP, 1);

            var maxHPRaw = (skills.Level(Skill.StatConstitution) * 2
                         + skills.Level(Skill.StatStrength)
                         + (skills.Level(Skill.StatWill) / 3))
                         * (level.Level / 25)
                         + skills.Level(Skill.StatConstitution);

            skills.MaxHP = Math.Clamp(maxHPRaw, 1, 1000000) * (skills.Level(Skill.StatLife) / 100) + 5;
            skills.MaxHP = Math.Max(skills.MaxHP, 1);

            // TODO traits
            skills.MaxStamina = 100 + (skills.Level(Skill.StatConstitution) + skills.Level(Skill.StatStrength)) / 5;
        }
    }
}
