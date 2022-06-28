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
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.World;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Weight;

namespace OpenNefia.Content.Charas
{
    public interface ICharaSystem : IEntitySystem
    {
        PrototypeId<ChipPrototype> GetDefaultCharaChip(EntityUid uid, CharaComponent? chara = null);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(PrototypeId<RacePrototype> raceID, Gender gender);
        PrototypeId<ChipPrototype> GetDefaultCharaChip(RacePrototype race, Gender gender);
        bool RenewStatus(EntityUid entity, CharaComponent? chara);
        bool Revive(EntityUid uid, CharaComponent? chara = null);
    }

    public class CharaSystem : EntitySystem, ICharaSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IRandom _random = default!; // randomness is only used for race height/weight here.
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEmotionIconSystem _emoicons = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IBuffsSystem _buffs = default!;
        [Dependency] private readonly ISkillAdjustsSystem _skillAdjusts = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, EntityGeneratedEvent>(HandleGenerated, priority: EventPriorities.Highest);
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

        public bool Revive(EntityUid entity, CharaComponent? chara = null)
        {
            if (!Resolve(entity, ref chara) || EntityManager.IsAlive(entity))
                return false;

            chara.Liveness = CharaLivenessState.Alive;

            if (EntityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
            {
                skills.HP = skills.MaxHP / 3;
                skills.MP = skills.MaxMP / 3;
                skills.Stamina = skills.MaxStamina / 3;
            }

            if (EntityManager.TryGetComponent<SanityComponent>(entity, out var sanity))
            {
                sanity.Insanity = 0;
            }

            if (EntityManager.TryGetComponent<HungerComponent>(entity, out var hunger))
            {
                hunger.Nutrition = 8000;
            }

            _vanillaAI.SetTarget(entity, null, 0);

            return RenewStatus(entity, chara);
        }

        public bool RenewStatus(EntityUid entity, CharaComponent? chara = null)
        {
            if (!Resolve(entity, ref chara))
                return false;

            _activities.RemoveActivity(entity);
            _effects.RemoveAll(entity);
            _buffs.RemoveAllBuffs(entity);
            _emoicons.SetEmotionIcon(entity, "None", GameTimeSpan.Zero);
            _skillAdjusts.RemoveAllSkillAdjusts(entity);

            if (EntityManager.TryGetComponent<VanillaAIComponent>(entity, out var vai))
            {
                vai.Aggro = 0;
            }
            
            _refresh.Refresh(entity);

            return true;
        }
    }
}
