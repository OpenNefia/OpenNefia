using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Skills;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using System;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Charas
{
    public class CharaSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRandom _random = default!; // randomness is only used for race height/weight here.

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(HandleGenerated, nameof(HandleGenerated),
                before: new[] { new SubId(typeof(SkillsSystem), "HandleGenerated") });
        }

        private void HandleGenerated(EntityUid uid, CharaComponent chara, ref EntityGeneratedEvent args)
        {
            InitRaceSkills(uid, chara);
            InitRaceEquipSlots(uid, chara);
            InitRacePhysiqueAndGender(uid, chara);
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

        private void InitRacePhysiqueAndGender(EntityUid uid, CharaComponent chara)
        {
            var race = _protos.Index(chara.Race);

            var weight = EntityManager.EnsureComponent<WeightComponent>(uid);

            weight.Age = _random.Next(race.MinAge, race.MaxAge);
            
            if (chara.Gender == Gender.Unknown)
            {
                if (_random.Prob(race.MaleRatio))
                {
                    chara.Gender = Gender.Male;
                }
                else
                {
                    chara.Gender = Gender.Female;
                }
            }

            // >>>>>>>> shade2/chara.hsp:518 	cHeight(rc)=cHeight(rc) + rnd(cHeight(rc)/5+1) -  ...

            weight.Height = race.BaseHeight;
            weight.Height = weight.Height + _random.Next(weight.Height / 5 + 1) - _random.Next(weight.Height / 5 + 1);
            weight.Weight = weight.Height * weight.Height * (_random.Next(6) + 18) / 10000;

            // <<<<<<<< shade2/chara.hsp:519 	cWeight(rc)= cHeight(rc)*cHeight(rc)*(rnd(6)+18)/ ..
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

            foreach (var skill in charaMakeSkill.Skills)
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
