using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Activity;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Hud;
using OpenNefia.Core.Maths;
using OpenNefia.Core;
using OpenNefia.Content.UI;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Mount;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;

        public override void Initialize()
        {
            SubscribeComponent<SkillsComponent, GetStatusIndicatorsEvent>(AddStaminaStatusIndicator, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SkillsComponent, EntityBeingGeneratedEvent>(InitDefaultSkills, priority: EventPriorities.Low);
            SubscribeComponent<SkillsComponent, EntityGeneratedEvent>(HandleGenerated, priority: EventPriorities.Low);
            SubscribeComponent<SkillsComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.VeryHigh);

            SubscribeComponent<SkillsComponent, EntityTurnStartingEventArgs>(HandleTurnStarting, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SkillsComponent, EntityTurnEndingEventArgs>(HandleTurnEnding, priority: EventPriorities.Low);

            SubscribeComponent<SkillsComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed, priority: EventPriorities.Highest);
        }

        private void AddStaminaStatusIndicator(EntityUid uid, SkillsComponent component, GetStatusIndicatorsEvent args)
        {
            Color? color = null;
            LocaleKey? key = null;

            if (component.Stamina < FatigueThresholds.Heavy)
            {
                color = UiColors.FatigueIndicatorHeavy;
                key = "Elona.Skill.Fatigue.Indicator.Heavy";
            }
            else if (component.Stamina < FatigueThresholds.Moderate)
            {
                color = UiColors.FatigueIndicatorModerate;
                key = "Elona.Skill.Fatigue.Indicator.Moderate";
            }
            else if (component.Stamina < FatigueThresholds.Light)
            {
                color = UiColors.FatigueIndicatorLight;
                key = "Elona.Skill.Fatigue.Indicator.Light";
            }

            if (color != null && key != null)
                args.OutIndicators.Add(new() { Text = Loc.GetString(key.Value), Color = color.Value });
        }

        public const int MaxSkillLevel = 2000;
        public const int MaxSkillPotential = 400;
        public const int MaxSkillExperience = 1000;
        public const double PotentialDecayRate = 0.9;

        private void InitDefaultSkills(EntityUid uid, SkillsComponent component, ref EntityBeingGeneratedEvent args)
        {
            foreach (var proto in _protos.EnumeratePrototypes<SkillPrototype>())
            {
                if (proto.InitialLevel != null)
                {
                    var currentLevel = BaseLevel(uid, proto, component);
                    var entityLevel = _levels.GetLevel(uid);
                    var initial = CalcInitialSkillLevelAndPotential(uid, proto, proto.InitialLevel.Value, currentLevel, entityLevel);
                    component.Skills[proto.GetStrongID()] = initial;
                }
            }
        }

        private LevelAndPotential CalcInitialSkillLevelAndPotential(EntityUid uid, SkillPrototype proto, int initialLevel, int currentLevel, int entityLevel)
        {
            var potential = CalcInitialPotential(proto, initialLevel, currentLevel != 0);
            var ev1 = new P_SkillCalcInitialPotentialEvent(uid, initialLevel, potential);
            _protos.EventBus.RaiseEvent(proto, ev1);
            potential = ev1.OutInitialPotential;

            var level = ((int)Math.Pow(potential, 2) * entityLevel / 45000 + initialLevel + entityLevel / 3);
            var ev2 = new P_SkillCalcInitialLevelEvent(uid, level);
            _protos.EventBus.RaiseEvent(proto, ev2);
            level = ev2.OutInitialLevel;

            potential = CalcDecayedInitialPotential(potential, entityLevel);

            // For life/mana/luck/speed
            var ev3 = new P_SkillCalcFinalInitialLevelAndPotentialEvent(uid, level, potential);
            _protos.EventBus.RaiseEvent(proto, ev3);
            level = ev3.OutInitialLevel;
            potential = ev3.OutInitialPotential;

            level = Math.Clamp(level, 0, MaxSkillLevel);
            potential = Math.Clamp(potential, 1, MaxSkillPotential);

            return new LevelAndPotential(level, potential);
        }

        private int CalcDecayedInitialPotential(int potential, int entityLevel)
        {
            // >>>>>>>> shade2/calculation.hsp:955 	if cLevel(c)>1	:p=int(pow@(growthDec,cLevel(c))*p ..
            if (entityLevel <= 1)
                return potential;

            return (int)(Math.Exp(Math.Log(PotentialDecayRate) * entityLevel) * potential);
            // <<<<<<<< shade2/calculation.hsp:955 	if cLevel(c)>1	:p=int(pow@(growthDec,cLevel(c))*p ..
        }

        private int CalcInitialPotential(SkillPrototype proto, int initialLevel, bool alreadyKnowsSkill)
        {
            // >>>>>>>> shade2/calculation.hsp:955 	if cLevel(c)>1	:p=int(pow@(growthDec,cLevel(c))*p ..
            if (proto.SkillType == SkillType.Attribute)
                return Math.Min(initialLevel * 20, 400);

            var potential = initialLevel * 5;

            if (alreadyKnowsSkill)
                potential += 50;
            else
                potential += 100;

            return potential;
            // <<<<<<<< shade2/calculation.hsp:955 	if cLevel(c)>1	:p=int(pow@(growthDec,cLevel(c))*p ..
        }

        private void HandleGenerated(EntityUid uid, SkillsComponent component, ref EntityGeneratedEvent args)
        {
            _refresh.Refresh(uid);
            _damage.HealToMax(uid);
        }

        private void HandleRefresh(EntityUid uid, SkillsComponent skills, ref EntityRefreshEvent args)
        {
            RefreshHPMPAndStamina(skills);
        }

        private void HandleRefreshSpeed(EntityUid uid, SkillsComponent skills, ref EntityRefreshSpeedEvent args)
        {
            var speedCorrection = CompOrNull<TurnOrderComponent>(uid)?.SpeedCorrection ?? 0;

            args.OutSpeed = Math.Max(TurnOrderSystem.MinSpeed, Level(uid, Protos.Skill.AttrSpeed, skills) + Math.Clamp(100 - speedCorrection, 0, 100));

            if (_gameSession.IsPlayer(uid) && !_mounts.HasMount(uid))
            {
                if (skills.Stamina < FatigueThresholds.Heavy)
                    args.OutSpeedModifier -= 0.3f;
                if (skills.Stamina < FatigueThresholds.Moderate)
                    args.OutSpeedModifier -= 0.2f;
                if (skills.Stamina < FatigueThresholds.Light)
                    args.OutSpeedModifier -= 0.1f;
            }
        }

        private void HandleTurnStarting(EntityUid uid, SkillsComponent skills, EntityTurnStartingEventArgs args)
        {
            if (args.Handled)
                return;

            skills.CanRegenerateThisTurn = true;

            if (_gameSession.IsPlayer(uid))
                GainExperienceAtTurnStart(uid, skills);
        }

        private void GainExperienceAtTurnStart(EntityUid uid, SkillsComponent skills)
        {
            if (!TryComp<TurnOrderComponent>(uid, out var turnOrder))
                return;

            var turn = turnOrder.TotalTurnsTaken % 10;
            if (turn == 1)
            {
                foreach (var member in _parties.EnumerateMembers(uid).Where(e => EntityManager.IsAlive(e)))
                {
                    GainHealingAndMeditationExperience(member);
                }
            }
            else if (turn == 2)
            {
                GainStealthExperience(uid, skills);
            }
            else if (turn == 3)
            {
                GainWeightLiftingExperience(uid, skills);
            }
            else if (turn == 4)
            {
                if (!_activities.HasAnyActivity(uid))
                {
                    _damage.HealStamina(uid, 2, showMessage: false, skills);
                }
            }
        }

        private void GainHealingAndMeditationExperience(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            var exp = 0;
            if (skills.HP != skills.MaxHP)
            {
                var healing = Level(uid, Protos.Skill.Healing);
                if (healing < Level(uid, Protos.Skill.AttrConstitution))
                    exp = 5 + healing / 5;
            }
            GainSkillExp(uid, Protos.Skill.Healing, exp, 1000, skills: skills);

            exp = 0;
            if (skills.MP != skills.MaxMP)
            {
                var meditation = Level(uid, Protos.Skill.Meditation);
                if (meditation < Level(uid, Protos.Skill.AttrMagic))
                    exp = 5 + meditation / 5;
            }
            GainSkillExp(uid, Protos.Skill.Meditation, exp, 1000, skills: skills);
        }

        private void GainStealthExperience(EntityUid uid, SkillsComponent skills)
        {
            var exp = 2;

            if (TryMap(uid, out var map) && HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) && _rand.OneIn(20))
                exp = 0;

            GainSkillExp(uid, Protos.Skill.Stealth, exp, 0, 1000, skills);
        }

        private void GainWeightLiftingExperience(EntityUid uid, SkillsComponent skills)
        {
            if (!TryComp<InventoryComponent>(uid, out var inv))
                return;

            var exp = 0;

            if (inv.BurdenType > BurdenType.None)
            {
                exp = 4;

                if (TryMap(uid, out var map) && HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) && _rand.OneIn(20))
                    exp = 0;
            }

            GainSkillExp(uid, Protos.Skill.WeightLifting, exp, 0, 1000, skills: skills);
        }

        private void HandleTurnEnding(EntityUid uid, SkillsComponent skills, EntityTurnEndingEventArgs args)
        {
            if (skills.CanRegenerateThisTurn)
                Regenerate(uid, skills);
        }

        public void Regenerate(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            if (_rand.OneIn(6))
            {
                var hpDelta = _rand.Next(Level(uid, Protos.Skill.Healing) / 3 + 1) + 1;
                _damage.HealHP(uid, hpDelta, showMessage: false, skills);
            }
            if (_rand.OneIn(5))
            {
                var mpDelta = _rand.Next(Level(uid, Protos.Skill.Meditation) / 2 + 1) + 1;
                _damage.HealMP(uid, mpDelta, showMessage: false, skills);
            }
        }
    }

    public static class FatigueThresholds
    {
        public const int Light = 50;
        public const int Moderate = 25;
        public const int Heavy = 0;
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(SkillPrototype))]
    public struct P_SkillCalcInitialPotentialEvent
    {
        public P_SkillCalcInitialPotentialEvent(EntityUid entity, int initialLevel, int initialPotential)
        {
            Entity = entity;
            InitialLevel = initialLevel;
            OutInitialPotential = initialPotential;
        }

        public EntityUid Entity { get; }
        public int InitialLevel { get; }

        public int OutInitialPotential { get; set; }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(SkillPrototype))]
    public struct P_SkillCalcInitialLevelEvent
    {
        public P_SkillCalcInitialLevelEvent(EntityUid entity, int initialLevel)
        {
            Entity = entity;
            InitialLevel = initialLevel;
            OutInitialLevel = initialLevel;
        }

        public EntityUid Entity { get; }
        public int InitialLevel { get; }

        public int OutInitialLevel { get; set; }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(SkillPrototype))]
    public struct P_SkillCalcFinalInitialLevelAndPotentialEvent
    {
        public P_SkillCalcFinalInitialLevelAndPotentialEvent(EntityUid entity, int initialLevel, int initialPotential)
        {
            Entity = entity;
            OutInitialLevel = initialLevel;
            OutInitialPotential = initialPotential;
        }

        public EntityUid Entity { get; }

        public int OutInitialLevel { get; set; }
        public int OutInitialPotential { get; set; }
    }
}
