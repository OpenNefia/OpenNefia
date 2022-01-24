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

        private PrototypeId<PortraitPrototype>[] MalePortraits = default!;
        private PrototypeId<PortraitPrototype>[] FemalePortraits = default!;

        public override void Initialize()
        {
            _protos.PrototypesReloaded += OnPrototypesReloaded;
            SubscribeLocalEvent<CharaComponent, EntityGeneratedEvent>(HandleGenerated, nameof(HandleGenerated),
                before: new[] { new SubId(typeof(SkillsSystem), "HandleGenerated") });
            SubscribeLocalEvent<GenRandomPortraitComponent, EntityGeneratedEvent>(GenRandomPortrait, nameof(GenRandomPortrait));
        }

        private void RebuildPortraitArrays()
        {
            var maleList = new List<PrototypeId<PortraitPrototype>>();
            var femaleList = new List<PrototypeId<PortraitPrototype>>();
            foreach (var port in _protos.EnumeratePrototypes<PortraitPrototype>())
            {
                if (!port?.Gender.HasValue ?? true)
                    continue;

                var list = port!.Gender!.Value switch
                {
                    Gender.Female => femaleList,
                    _ => maleList
                };
                list.Add(port!.GetStrongID());
            }
            MalePortraits = maleList.ToArray();
            FemalePortraits = femaleList.ToArray();
        }

        private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
        {
            RebuildPortraitArrays();
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
            if (MalePortraits == null || FemalePortraits == null)
                RebuildPortraitArrays();

            Gender gender = Gender.Male;
            if (_entityManager.TryGetComponent<CharaComponent>(uid, out var chara))
                gender = chara.Gender;

            var randName = gender switch
            {
                Gender.Female => FemalePortraits,
                _ => MalePortraits
            };

            var portComp = _entityManager.EnsureComponent<PortraitComponent>(uid);
            portComp.PortraitID = _random.Pick(randName!);

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
