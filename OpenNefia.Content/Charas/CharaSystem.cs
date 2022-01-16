using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Skills;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Charas
{
    public class CharaSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(HandleGenerated, nameof(HandleGenerated),
                before: new[] { new SubId(typeof(SkillsSystem), "HandleGenerated") });
        }

        private void HandleGenerated(EntityUid uid, CharaComponent chara, ref EntityGeneratedEvent args)
        {
            InitRaceSkills(uid, chara);
            InitRaceEquipSlots(uid, chara);
            SetRaceDefaultChip(uid, chara);

            InitClassSkills(uid, chara);
            InitCharaMakeSkills(uid);
        }

        private void InitRaceSkills(EntityUid uid, CharaComponent chara,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            foreach (var pair in _protos.Index(chara.Race).BaseSkills)
            {
                if (!skills.Skills.ContainsKey(pair.Key))
                {
                    skills.Skills.Add(pair.Key, new LevelAndPotential() { Level = new(pair.Value) });
                }
            }
        }

        private void InitRaceEquipSlots(EntityUid uid, CharaComponent chara,
            EquipSlotsComponent? equipSlots = null)
        {
            if (!Resolve(uid, ref equipSlots))
                return;

            var race = _protos.Index(chara.Race);

            // Everyone gets a ranged and ammo slot.
            var initialEquipSlots = race.InitialEquipSlots
                .Append(EquipSlot.Ranged)
                .Append(EquipSlot.Ammo);

            _equipSlots.InitializeEquipSlots(uid, initialEquipSlots, equipSlots: equipSlots);
        }

        private void SetRaceDefaultChip(EntityUid uid, CharaComponent chara, ChipComponent? chip = null)
        {
            if (!Resolve(uid, ref chip))
                return;

            var race = _protos.Index(chara.Race);

            if (chip.ChipID == Chip.Default)
            {
                switch (chara.Gender)
                {
                    case Gender.Male:
                        chip.ChipID = race.ChipMale;
                        break;
                    case Gender.Female:
                    default:
                        chip.ChipID = race.ChipFemale;
                        break;
                }
            }
        }

        private void InitClassSkills(EntityUid uid, CharaComponent chara,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            foreach (var pair in _protos.Index(chara.Class).BaseSkills)
            {
                if (!skills.Skills.ContainsKey(pair.Key))
                {
                    skills.Skills.Add(pair.Key, new LevelAndPotential() { Level = new(0) });
                }

                skills.Skills[pair.Key].Level.Base += pair.Value;
            }
        }

        private void InitCharaMakeSkills(EntityUid uid, SkillsComponent? skills = null, 
            CharaMakeSkillInitTempComponent? charaMakeSkill = null)
        {
            if (!Resolve(uid, ref charaMakeSkill, ref skills, logMissing: false))
                return;

            foreach(var skill in charaMakeSkill.Skills)
            {
                skills.Skills[skill.Key] = new LevelAndPotential
                {
                    Level = new(skill.Value),
                }; 
            }

            _entityManager.RemoveComponent<CharaMakeSkillInitTempComponent>(uid);
        }
    }
}
