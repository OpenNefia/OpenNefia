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
using OpenNefia.Content.Charas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Combat;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Roles;
using OpenNefia.Content.World;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Maps;
using OpenNefia.Content.EmotionIcon;

namespace OpenNefia.Content.Damage
{
    public sealed partial class DamageSystem
    {
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IRoleSystem _roles = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IEntityGenMemorySystem _entityGenMemory = default!;

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
                    _emoIcons.SetEmotionIcon(target, EmotionIcons.Angry, 2);
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

        private void SpillBloodOrDebrisOnDeath(EntityUid target, ref EntityKilledEvent ev)
        {
            //>>>>>>>> elona122/shade2/chara_func.hsp:1651 		if cBit(cStoneBlood,tc){ ...
            var elementID = (ev.DamageType as ElementalDamageType)?.ElementID;

            if (CompOrNull<StoneBloodComponent>(target)?.HasStoneBlood ?? false)
            {
                if (_vis.IsInWindowFov(target))
                {
                    _audio.Play(Protos.Sound.Crush1, target);
                    var anim = new DeathMapDrawable(Protos.Asset.DeathFragments, elementID);
                    _mapDrawables.Enqueue(anim, target);
                }
                _mapDebris.SpillFragments(Spatial(target).MapPosition, 3);
            }
            else
            {
                if (_vis.IsInWindowFov(target))
                {
                    _audio.Play(_rand.Pick(KillSounds), target);
                    var anim = new DeathMapDrawable(Protos.Asset.DeathBlood, elementID);
                    _mapDrawables.Enqueue(anim, target);
                }
                _mapDebris.SpillBlood(Spatial(target).MapPosition, 4);
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1661 			} ...
        }

        private const int VillagerRespawnPeriodHours = 48;
        private const int AdventurerRespawnPeriodHours = 24;

        private void SetLivenessOnDeath(EntityUid target)
        {
            // -- >>>>>>>> elona122/shade2/chara_func.hsp:1663 		if cRole(tc)=0{ ...
            if (!TryComp<CharaComponent>(target, out var chara))
            {
                if (TryComp<MetaDataComponent>(target, out var metaData))
                {
                    metaData.Liveness = EntityGameLiveness.DeadAndBuried;
                }
                return;
            }

            if (!_roles.HasAnyRoles(target))
            {
                chara.Liveness = CharaLivenessState.Dead;
            }
            else
            {
                if (HasComp<RoleAdventurerComponent>(target))
                {
                    chara.Liveness = CharaLivenessState.AdventurerHospital;
                    chara.RespawnDate = _world.State.GameDate + GameTimeSpan.FromHours(AdventurerRespawnPeriodHours + _rand.Next(AdventurerRespawnPeriodHours / 2));
                }
                else
                {
                    chara.Liveness = CharaLivenessState.VillagerDead;
                    chara.RespawnDate = _world.State.GameDate + GameTimeSpan.FromHours(VillagerRespawnPeriodHours);
                }
            }

            // -- <<<<<<<< elona122/shade2/chara_func.hsp:1672 			} ...
        }

        private int CalcDefaultKillExperience(EntityUid target, EntityUid attacker)
        {
            var level = _levels.GetLevel(target);
            var exp = Math.Clamp(level, 1, 200) * Math.Clamp(level + 1, 1, 200) * Math.Clamp(level + 2, 1, 200) / 20 + 8;
            if (level > _levels.GetLevel(attacker))
                exp /= 4;

            return exp;
        }

        private int CalcKillExperience(EntityUid target, EntityUid attacker)
        {
            var defaultExp = CalcDefaultKillExperience(target, attacker);
            var ev = new CalcKillExperienceEvent(target, attacker, defaultExp);
            RaiseEvent(target, ref ev);
            return ev.OutExperience;
        }

        private void ApplyExperienceGainOnKill(EntityUid target, ref EntityKilledEvent ev)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1688 		if dmgSource >= dmgFromChara{ ...
            if (!IsAlive(ev.Attacker))
                return;

            var exp = CalcKillExperience(target, ev.Attacker.Value);
            _levels.GainExperience(target, exp);
            // <<<<<<<< elona122/shade2/chara_func.hsp:1695 			} ...
        }

        private void UpdateEntityGenMemory(EntityUid target, ref EntityKilledEvent ev)
        {
            // >>>>>>>> shade2/chara_func.hsp:1726 		if tc!pc{ ..
            if (_gameSession.IsPlayer(target))
                return;

            if (TryComp<MetaDataComponent>(target, out var metaData) && metaData.EntityPrototype != null)
            {
                _entityGenMemory.Killed(metaData.EntityPrototype.GetStrongID());
            }
            // <<<<<<<< shade2/chara_func.hsp:1735 		check_kill dmgSource,tc ..
        }

        private void HandleKilled(EntityUid target, ref EntityKilledEvent ev)
        {
            if (ev.DamageType is ElementalDamageType ele)
            {
                var sound = _protos.Index(ele.ElementID).Sound?.GetSound();
                if (sound != null)
                    _audio.Play(sound.Value, target);

                var pev = new P_ElementKillCharaEvent(ev.Attacker, target);
                _protos.EventBus.RaiseEvent(ele.ElementID, ref pev);
            }

            SpillBloodOrDebrisOnDeath(target, ref ev);
            SetLivenessOnDeath(target);

            // TODO ally impression, quest bodyguard

            if (_gameSession.IsPlayer(target))
            {
                _world.State.TotalDeaths++;
            }

            ApplyExperienceGainOnKill(target, ref ev);

            if (IsAlive(ev.Attacker))
            {
                _vanillaAI.SetTarget(ev.Attacker.Value, null);
            }

            UpdateEntityGenMemory(target, ref ev);

            if (_parties.IsUnderlingOfPlayer(target))
                _mes.Display(Loc.GetString("Elona.Damage.YouFeelSad"));

            if (TryMap(target, out var map) && TryComp<MapCharaGenComponent>(map.MapEntityUid, out var mapCharaGen))
                mapCharaGen.MaxCharaCount--;

            if (ev.Attacker != null)
                RunCheckKillEvents(target, ev.Attacker.Value);
        }

        /// <inheritdoc/>
        public void RunCheckKillEvents(EntityUid target, EntityUid attacker)
        {
            // >>>>>>>> shade2/chara_func.hsp:1121 #deffunc check_kill int cc,int tc ..
            var ev = new CheckKillEvent(target, attacker);
            RaiseEvent(target, ref ev);
            // <<<<<<<< shade2/chara_func.hsp:1146 	return ..
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

    [ByRefEvent]
    public struct CalcKillExperienceEvent
    {
        public EntityUid Target { get; }
        public EntityUid Attacker { get; }

        public int OutExperience { get; set; }

        public CalcKillExperienceEvent(EntityUid target, EntityUid attacker, int exp)
        {
            Target = target;
            Attacker = attacker;
            OutExperience = exp;
        }
    }

    /// <summary>
    /// Event run when trying to assign blame to a character's death.
    /// The target may not necessarily have died from a direct combat strike
    /// from the attacker, but instead from something like a trap or spilt potion
    /// set up by the attacker. This event ensures that karma and impression loss 
    /// applies for those kinds of deathsas well.
    /// 
    /// By contrast, <see cref="EntityKilledEvent"/> alone may not have the entity to blame
    /// for the death passed to the "attacker" field because there technically was no direct "attacker"
    /// for the trap/potion/etc.
    /// </summary>
    [ByRefEvent]
    public struct CheckKillEvent
    {
        public EntityUid Target { get; }
        public EntityUid Attacker { get; }

        public CheckKillEvent(EntityUid target, EntityUid attacker)
        {
            Target = target;
            Attacker = attacker;
        }
    }
}
