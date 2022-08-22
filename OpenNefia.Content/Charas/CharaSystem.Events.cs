using OpenNefia.Content.Activity;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.CharaMake;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Weight;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.CharaInfo.SkillsListControl.SkillsListEntry;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.Game;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Charas
{
    public sealed partial class CharaSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRandom _rand = default!; // randomness is only used for race height/weight here.
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IEmotionIconSystem _emoicons = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IBuffsSystem _buffs = default!;
        [Dependency] private readonly ISkillAdjustsSystem _skillAdjusts = default!;
        [Dependency] private readonly ISlotSystem _slots = default!;
        [Dependency] private readonly IEquipmentGenSystem _equipmentGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, GetInitialEquipSlotsEvent>(GetInitialEquipSlots_Race, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CharaComponent, EntityBeingGeneratedEvent>(HandleGenerated, priority: EventPriorities.VeryHigh);
        }

        private void HandleGenerated(EntityUid uid, CharaComponent chara, ref EntityBeingGeneratedEvent args)
        {
            InitRaceComponents(uid, chara);
            InitRaceSkills(uid, chara);
            InitRacePhysiqueAndGender(uid, chara);
            SetDefaultCharaChip(uid, chara);

            InitClassComponents(uid, chara);
            InitClassSkills(uid, chara);
            InitCharaMakeSkills(uid, args.GenArgs);
            InitClassEquipmentType(uid, chara);

            InitEquipment(uid);
        }

        private void InitRaceComponents(EntityUid uid, CharaComponent chara)
        {
            var race = _protos.Index(chara.Race);
            chara.RaceSlot = _slots.AddSlot(uid, race.Components);
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

        private void GetInitialEquipSlots_Race(EntityUid uid, CharaComponent chara, GetInitialEquipSlotsEvent ev)
        {
            var race = _protos.Index(chara.Race);
            var initialEquipSlots = race.InitialEquipSlots
                .Append(EquipSlot.Ranged)
                .Append(EquipSlot.Ammo);
            ev.OutEquipSlots.AddRange(initialEquipSlots);
        }

        private void InitRacePhysiqueAndGender(EntityUid uid, CharaComponent chara)
        {
            var race = _protos.Index(chara.Race);

            var weight = EntityManager.EnsureComponent<WeightComponent>(uid);

            weight.Age = _rand.Next(race.MinAge, race.MaxAge);

            if (chara.Gender == Gender.Unknown)
            {
                if (_rand.Prob(race.MaleRatio))
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
            weight.Height = weight.Height + _rand.Next(weight.Height / 5 + 1) - _rand.Next(weight.Height / 5 + 1);
            weight.Weight.Base = weight.Height * weight.Height * (_rand.Next(6) + 18) / 10000;

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

            return GetDefaultCharaChip(chara.Race, chara.Gender, CompOrNull<ChipFromGenderComponent>(uid)?.Chips);
        }

        public PrototypeId<ChipPrototype> GetDefaultCharaChip(PrototypeId<RacePrototype> raceID, Gender gender,
            IReadOnlyDictionary<Gender, PrototypeId<ChipPrototype>>? chipFromGender = null)
            => GetDefaultCharaChip(_protos.Index(raceID), gender, chipFromGender);

        public PrototypeId<ChipPrototype> GetDefaultCharaChip(
            RacePrototype race,
            Gender gender,
            IReadOnlyDictionary<Gender, PrototypeId<ChipPrototype>>? chipFromGender = null)
        {
            if (chipFromGender != null && chipFromGender.TryGetValue(gender, out var chip))
                return chip;
            
            switch (gender)
            {
                case Gender.Male:
                    return race.ChipMale;
                case Gender.Female:
                default:
                    return race.ChipFemale;
            }
        }

        private void InitClassComponents(EntityUid uid, CharaComponent chara)
        {
            var klass = _protos.Index(chara.Class);
            chara.ClassSlot = _slots.AddSlot(uid, klass.Components);
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

        private void InitCharaMakeSkills(EntityUid uid, EntityGenArgSet args, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills) || !args.TryGet<CharaMakeGenArgs>(out var charaMakeArgs))
                return;

            foreach (var skill in charaMakeArgs.InitialSkills)
            {
                skills.Skills[skill.Key] = new LevelAndPotential
                {
                    Level = new(skill.Value),
                };
            }
        }

        private void InitClassEquipmentType(EntityUid uid, CharaComponent chara)
        {
            var klass = _protos.Index(chara.Class);
            if (klass.EquipmentType != null && !HasComp<EquipmentGenComponent>(uid))
            {
                EnsureComp<EquipmentGenComponent>(uid).EquipmentType = klass.EquipmentType.Value;
            }
        }

        private void InitEquipment(EntityUid uid)
        {
            _equipmentGen.GenerateEquipment(uid);
        }
    }
}
