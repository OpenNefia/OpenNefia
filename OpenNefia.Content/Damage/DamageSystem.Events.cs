using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Factions;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Random;
using NuGet.DependencyResolver;
using System.IO.Abstractions;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.Damage
{
    public sealed partial class DamageSystem
    {
        #region Damage events

        private void ProcRetreatInFear(EntityUid entity, ref EntityWoundedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1535 		if cHp(tc)<cMhp(tc)/5:if tc!pc:if cFear(tc)=0:if ..
            if (!TryComp<SkillsComponent>(entity, out var skills) || skills.HP >= skills.MaxHP / 5)
                return;

            if (_gameSession.IsPlayer(entity) || _effects.HasEffect(entity, Protos.StatusEffect.Fear))
                return;

            if (!_effects.CanApplyTo(entity, Protos.StatusEffect.Fear))
                return;

            var retreats = (args.FinalDamage * 100 / skills.MaxHP + 10) > _rand.Next(200);

            if (EntityManager.IsAlive(args.Attacker) && _feats.HasFeat(args.Attacker.Value, Protos.Feat.NoFear))
                retreats = false;

            if (retreats)
            {
                _effects.SetTurns(entity, Protos.StatusEffect.Fear, _rand.Next(20 + 5));
                _mes.Display(Loc.GetString("Elona.Damage.RunsAway", ("entity", entity)), UiColors.MesBlue, entity: entity);
            }
            // <<<<<<<< shade2/chara_func.hsp:1539 			} ..
        }

        private void DisturbSleep(EntityUid entity, ref EntityWoundedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1563 		if cSleep(tc)!0: if (ele!rsResMind)&(ele!rsResNe ...
            if (args.DamageType is not ElementalDamageType ele || !_protos.Index(ele.ElementID).PreservesSleep)
            {
                if (_effects.HasEffect(entity, Protos.StatusEffect.Sleep))
                {
                    _effects.Remove(entity, Protos.StatusEffect.Sleep);
                    _mes.Display(Loc.GetString("Elona.Damage.SleepIsDisturbed", ("entity", entity)));
                }
            }

            if (EntityManager.IsAlive(args.Attacker) && TryComp<VisibilityComponent>(args.Attacker.Value, out var vis))
            {
                vis.Noise = 100;
                _factions.ActHostileTowards(args.Attacker.Value, entity);
            }

            // <<<<<<<< shade2/chara_func.hsp:1567 			} ..
        }

        private void ApplyHostileActionAfterDamage(EntityUid target, ref EntityWoundedEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1583 		if dmgSource>=0{ ..
            if (!EntityManager.IsAlive(args.Attacker)
                || _gameSession.IsPlayer(target)
                || !TryComp<VanillaAIComponent>(target, out var targetAI)
                || !TryComp<VanillaAIComponent>(args.Attacker.Value, out var attackerAI))
                return;

            var applyAggro = false;
            var attacker = args.Attacker.Value;

            if (_factions.GetRelationTowards(target, attacker) <= Relation.Enemy)
            {
                if (_factions.GetOriginalRelationTowards(target, attacker) > Relation.Enemy)
                {
                    if (targetAI.Aggro <= 0 || _rand.OneIn(4))
                        applyAggro = true;
                }
            }
            else
            {
                if (_factions.GetOriginalRelationTowards(target, attacker) <= Relation.Enemy)
                {
                    if (targetAI.Aggro <= 0 || _rand.OneIn(4))
                        applyAggro = true;
                }
            }

            if (!_gameSession.IsPlayer(attacker) && attackerAI.CurrentTarget == target && _rand.OneIn(3))
                applyAggro = true;

            if (applyAggro)
            {
                if (!_gameSession.IsPlayer(target))
                    targetAI.CurrentTarget = attacker;

                if (targetAI.Aggro <= 0)
                {
                    _emoIcons.SetEmotionIcon(target, "Elona.Angry", 2);
                    targetAI.Aggro = 20;
                }
                else
                {
                    targetAI.Aggro += 2;
                }
            }
            // <<<<<<<< shade2/chara_func.hsp:1593 		} ..
        }

        private void PlayHeartbeatSound(EntityUid uid, ref EntityWoundedEvent args)
        {
            if (!_gameSession.IsPlayer(uid))
                return;

            var threshold = _config.GetCVar(CCVars.DisplayHeartbeatThreshold);
            if (TryComp<SkillsComponent>(uid, out var skills) && skills.HP < skills.MaxHP * threshold)
                _audio.Play(Protos.Sound.Heart1);
        }

        private void ProcMagicReaction(EntityUid uid, ref AfterDamageMPEvent args)
        {
            // >>>>>>>> shade2/chara_func.hsp:1778 	if cMP(tc)<0{	 ..
            if (!TryComp<SkillsComponent>(uid, out var skills) || skills.MP >= 0 || args.NoMagicReaction)
                return;

            var magicCapacityExp = Math.Abs(skills.MP) * 200 / (skills.MaxMP + 1);
            _skills.GainSkillExp(uid, Protos.Skill.MagicCapacity, magicCapacityExp, skills: skills);

            var damage = (skills.MP * -1) * 400 / (100 + _skills.Level(uid, Protos.Skill.MagicCapacity, skills) * 10);

            if (_gameSession.IsPlayer(uid))
            {
                if (_feats.HasFeat(uid, Protos.Feat.PermCapacity))
                    damage /= 2;
            }
            else
            {
                damage /= 5;
                if (damage < 10)
                    return;
            }

            _mes.Display(Loc.GetString("Elona.Damage.MagicReaction.Hurts", ("entity", uid)));
            DamageHP(uid, damage, damageType: new GenericDamageType("Elona.DamageType.MagicReaction"));
            // <<<<<<<< shade2/chara_func.hsp:1789 	return true ..
        }

        #endregion

        #region Kill events

        private readonly PrototypeId<SoundPrototype>[] KillSounds = new[]
        {
            Protos.Sound.Kill1,
            Protos.Sound.Kill2,
        };

        private void HandleKilled(EntityUid target, ref EntityKilledEvent ev)
        {
            if (ev.DamageType is ElementalDamageType ele)
            {
                var sound = _protos.Index(ele.ElementID).Sound?.GetSound();
                if (sound != null)
                    _audio.Play(sound.Value, target);
            }

            ShowKillText(target, ref ev);

            _sounds.Play(_rand.Pick(KillSounds), target);

            // TODO
            _mapDebris.SpillBlood(Spatial(target).MapPosition, 5);

            // TODO
            if (TryComp<CharaComponent>(target, out var chara))
            {
                chara.Liveness = CharaLivenessState.Dead;
            }
            else if (TryComp<MetaDataComponent>(target, out var metaData))
            {
                metaData.Liveness = EntityGameLiveness.DeadAndBuried;
            }
        }

        private void ShowKillText(EntityUid target, ref EntityKilledEvent ev)
        {
            if (!_vis.IsInWindowFov(target))
                return;

            var capitalize = true;
            if (ev.ExtraArgs.AttackCount > 0)
            {
                _mes.Display("Elona.Combat.PhysicalAttack.Furthermore");
                capitalize = false;
            }

            var isAlly = _parties.IsInPlayerParty(target);

            if (isAlly)
            {
                if (!ev.ExtraArgs.NoAttackText)
                {
                    if (EntityManager.IsAlive(ev.ExtraArgs.Weapon))
                    {
                        // TODO
                        if (ev.ExtraArgs.AttackSkill == Protos.Skill.Throwing)
                        {

                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        #endregion
    }
    [ByRefEvent]
    public struct CalcFinalDamageEvent
    {
        public int BaseDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType? DamageType { get; }
        public DamageHPExtraArgs ExtraArgs { get; }

        public int OutFinalDamage { get; set; }

        public CalcFinalDamageEvent(int damage, EntityUid? attacker, IDamageType? damageType, DamageHPExtraArgs extraArgs)
        {
            BaseDamage = damage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;

            OutFinalDamage = damage;
        }
    }

    [ByRefEvent]
    public struct AfterDamageAppliedEvent
    {
        public int BaseDamage { get; }
        public int FinalDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType? DamageType { get; }
        public DamageHPExtraArgs ExtraArgs { get; }

        public AfterDamageAppliedEvent(int baseDamage, int finalDamage, EntityUid? attacker, IDamageType? damageType, DamageHPExtraArgs extraArgs)
        {
            BaseDamage = baseDamage;
            FinalDamage = finalDamage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;
        }
    }

    [ByRefEvent]
    public struct EntityWoundedEvent
    {
        public int BaseDamage { get; }
        public int FinalDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType? DamageType { get; }
        public DamageHPExtraArgs ExtraArgs { get; }

        public EntityWoundedEvent(int baseDamage, int finalDamage, EntityUid? attacker, IDamageType? damageType, DamageHPExtraArgs extraArgs)
        {
            BaseDamage = baseDamage;
            FinalDamage = finalDamage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;
        }
    }

    [ByRefEvent]
    public struct EntityKilledEvent
    {
        public int BaseDamage { get; }
        public int FinalDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType? DamageType { get; }
        public DamageHPExtraArgs ExtraArgs { get; }

        public EntityKilledEvent(int baseDamage, int finalDamage, EntityUid? attacker, IDamageType? damageType, DamageHPExtraArgs extraArgs)
        {
            BaseDamage = baseDamage;
            FinalDamage = finalDamage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;
        }
    }

    [ByRefEvent]
    public struct AfterDamageHPEvent
    {
        public int BaseDamage { get; }
        public int FinalDamage { get; }
        public EntityUid? Attacker { get; }
        public IDamageType? DamageType { get; }
        public DamageHPExtraArgs ExtraArgs { get; }

        public AfterDamageHPEvent(int baseDamage, int finalDamage, EntityUid? attacker, IDamageType? damageType, DamageHPExtraArgs extraArgs)
        {
            BaseDamage = baseDamage;
            FinalDamage = finalDamage;
            Attacker = attacker;
            DamageType = damageType;
            ExtraArgs = extraArgs;
        }
    }

    [ByRefEvent]
    public struct AfterDamageMPEvent
    {
        public int Amount { get; }
        public bool NoMagicReaction { get; }
        public bool ShowMessage { get; }

        public AfterDamageMPEvent(int amount, bool noMagicReaction, bool showMessage)
        {
            Amount = amount;
            NoMagicReaction = noMagicReaction;
            ShowMessage = showMessage;
        }
    }
}
