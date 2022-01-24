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
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Charas
{
    public interface ICharaSystem : IEntitySystem
    {
        PrototypeId<ChipPrototype> GetDefaultCharaChip(EntityUid uid, CharaComponent? chara = null);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(PrototypeId<RacePrototype> raceID, Gender gender);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(RacePrototype race, Gender gender);
    }

    public class CharaSystem : EntitySystem, ICharaSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRandom _random = default!; // randomness is only used for race height/weight here.

        private PrototypeId<PortraitPrototype>[] MalePortraits = new[]
        {
            Protos.Portrait.Man1,
            Protos.Portrait.Man2,
            Protos.Portrait.Man3,
            Protos.Portrait.Man4,
            Protos.Portrait.Man5,
            Protos.Portrait.Man6,
            Protos.Portrait.Man7,
            Protos.Portrait.Man8,
            Protos.Portrait.Man9,
            Protos.Portrait.Man10,
            Protos.Portrait.Man11,
            Protos.Portrait.Man12,
            Protos.Portrait.Man13,
            Protos.Portrait.Man14,
            Protos.Portrait.Man15,
            Protos.Portrait.Man16,
            Protos.Portrait.Man17,
            Protos.Portrait.Man18,
            Protos.Portrait.Man19,
            Protos.Portrait.Man20,
            Protos.Portrait.Man21,
            Protos.Portrait.Man22,
            Protos.Portrait.Man23,
            Protos.Portrait.Man24,
            Protos.Portrait.Man25,
            Protos.Portrait.Man26,
            Protos.Portrait.Man27,
            Protos.Portrait.Man28,
            Protos.Portrait.Man29,
            Protos.Portrait.Man30,
            Protos.Portrait.Man31,
            Protos.Portrait.Man32,
            Protos.Portrait.Man33,
            Protos.Portrait.Man34,
            Protos.Portrait.Man35,
            Protos.Portrait.Man36,
            Protos.Portrait.Man39,
            Protos.Portrait.Man40,
            Protos.Portrait.Man41,
            Protos.Portrait.Man42,
            Protos.Portrait.Man43,
            Protos.Portrait.Man44,
            Protos.Portrait.Man45,
            Protos.Portrait.Man46,
            Protos.Portrait.Man47,
            Protos.Portrait.Man48,
        };

        private PrototypeId<PortraitPrototype>[] FemalePortraits = new[]
        {
            Protos.Portrait.Woman1,
            Protos.Portrait.Woman2,
            Protos.Portrait.Woman3,
            Protos.Portrait.Woman4,
            Protos.Portrait.Woman5,
            Protos.Portrait.Woman6,
            Protos.Portrait.Woman7,
            Protos.Portrait.Woman8,
            Protos.Portrait.Woman9,
            Protos.Portrait.Woman10,
            Protos.Portrait.Woman11,
            Protos.Portrait.Woman12,
            Protos.Portrait.Woman13,
            Protos.Portrait.Woman14,
            Protos.Portrait.Woman15,
            Protos.Portrait.Woman16,
            Protos.Portrait.Woman17,
            Protos.Portrait.Woman18,
            Protos.Portrait.Woman19,
            Protos.Portrait.Woman20,
            Protos.Portrait.Woman21,
            Protos.Portrait.Woman22,
            Protos.Portrait.Woman23,
            Protos.Portrait.Woman24,
            Protos.Portrait.Woman25,
            Protos.Portrait.Woman26,
            Protos.Portrait.Woman27,
            Protos.Portrait.Woman28,
            Protos.Portrait.Woman29,
            Protos.Portrait.Woman30,
            Protos.Portrait.Woman31,
            Protos.Portrait.Woman32,
            Protos.Portrait.Woman34,
            Protos.Portrait.Woman35,
            Protos.Portrait.Woman36,
            Protos.Portrait.Woman37,
            Protos.Portrait.Woman38,
            Protos.Portrait.Woman39,
            Protos.Portrait.Woman40,
            Protos.Portrait.Woman41,
            Protos.Portrait.Woman42,
            Protos.Portrait.Woman43,
            Protos.Portrait.Woman44,
            Protos.Portrait.Woman45,
            Protos.Portrait.Woman46,
            Protos.Portrait.Woman47,
            Protos.Portrait.Woman48,
            Protos.Portrait.Woman49,
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(HandleGenerated, nameof(HandleGenerated),
                before: new[] { new SubId(typeof(SkillsSystem), "HandleGenerated") });
            SubscribeLocalEvent<GenRandomPortraitComponent, EntityGeneratedEvent>(GenRandomPortrait, nameof(GenRandomPortrait));
        }

        private void HandleGenerated(EntityUid uid, CharaComponent chara, ref EntityGeneratedEvent args)
        {
            InitRaceSkills(uid, chara);
            InitRaceEquipSlots(uid, chara);
            InitRacePhysiqueAndGender(uid, chara);
            SetDefaultCharaChip(uid, chara);

            InitClassSkills(uid, chara);
            InitCharaMakeSkills(uid);
        }
        
        private void GenRandomPortrait(EntityUid uid, GenRandomPortraitComponent genPor, ref EntityGeneratedEvent args)
        {
            Gender gender = Gender.Male;
            if (_entityManager.TryGetComponent<CharaComponent>(uid, out var chara))
                gender = chara.Gender;

            var randName = gender switch
            {
                Gender.Female => FemalePortraits,
                _ => MalePortraits
            };

            var portComp = _entityManager.EnsureComponent<PortraitComponent>(uid);
            portComp.PortraitID = _random.Pick(randName);

            _entityManager.RemoveComponent<GenRandomPortraitComponent>(uid);
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

        private void SetDefaultCharaChip(EntityUid uid, CharaComponent chara, ChipComponent? chip = null)
        {
            if (!Resolve(uid, ref chip))
                return;
            
            if (chip.ChipID == Chip.Default)
            {
                chip.ChipID = GetDefaultCharaChip(uid, chara);
            }
        }

        public PrototypeId<ChipPrototype> GetDefaultCharaChip(EntityUid uid, CharaComponent? chara = null)
        {
            if (!Resolve(uid, ref chara))
            {
                Logger.ErrorS("chara", "No default chara chip found for entity '{uid}'!");
                return Chip.Default;
            }

            return GetDefaultCharaChip(chara.Race, chara.Gender);
        }

        public PrototypeId<ChipPrototype> GetDefaultCharaChip(PrototypeId<RacePrototype> raceID, Gender gender)
            => GetDefaultCharaChip(_protos.Index(raceID), gender);

        public PrototypeId<ChipPrototype> GetDefaultCharaChip(
            RacePrototype race,
            Gender gender)
        {
            switch (gender)
            {
                case Gender.Male:
                    return race.ChipMale;
                case Gender.Female:
                default:
                    return race.ChipFemale;
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
