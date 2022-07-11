﻿using OpenNefia.Content.Activity;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Damage
{
    public interface IDamageSystem : IEntitySystem
    {
        DamageHPResult DamageHP(EntityUid target, int baseDamage, EntityUid? attacker = null, IDamageType? damageType = null, DamageHPExtraArgs? extraArgs = null, SkillsComponent? skills = null);

        void HealToMax(EntityUid uid, SkillsComponent? skills = null);
        void HealHP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealMP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealStamina(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);

        /// <summary>
        /// Runs some extra events as if <see cref="attacker"/> killed <see cref="target"/>,
        /// including karma loss, quest quotas, etc.
        /// </summary>
        void RunCheckKillEvents(EntityUid target, EntityUid attacker);
    }

    public sealed partial class DamageSystem : EntitySystem, IDamageSystem
    {
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;

        public DamageHPResult DamageHP(EntityUid target, int baseDamage, EntityUid? attacker = null, IDamageType? damageType = null, DamageHPExtraArgs? extraArgs = null, SkillsComponent? skills = null)
        {
            if (!Resolve(target, ref skills))
                return new(false, 0, 0);

            if (!EntityManager.IsAlive(target))
                return new(false, 0, 0);

            extraArgs ??= new();

            var ev1 = new CalcFinalDamageEvent(baseDamage, attacker, damageType, extraArgs);
            RaiseEvent(target, ref ev1);
            var finalDamage = ev1.OutFinalDamage;

            skills.HP = Math.Min(skills.HP - finalDamage, skills.MaxHP);

            // This is for nether damage HP recovery.
            var ev2 = new AfterDamageAppliedEvent(baseDamage, finalDamage, attacker, damageType, extraArgs);
            RaiseEvent(target, ref ev2);

            // shade2/chara_func.hsp:1501 	if cHP(tc)>=0{ ...
            if (skills.HP >= 0)
            {
                var ev3 = new EntityWoundedEvent(baseDamage, finalDamage, attacker, damageType, extraArgs);
                RaiseEvent(target, ref ev3);
            }

            // shade2/chara_func.hsp:1596 	if cHp(tc)<0{ ...
            var wasKilled = false;
            if (skills.HP < 0)
            {
                var ev4 = new EntityKilledEvent(baseDamage, finalDamage, attacker, damageType, extraArgs);
                RaiseEvent(target, ref ev4);
                wasKilled = !EntityManager.IsAlive(target);
            }

            var ev5 = new AfterDamageHPEvent(baseDamage, finalDamage, attacker, damageType, extraArgs);
            RaiseEvent(target, ref ev5);

            return new(wasKilled, baseDamage, finalDamage);
        }

        public void DamageMP(EntityUid target, int amount, bool noMagicReaction = false, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(target, ref skills))
                return;

            var ev = new AfterDamageMPEvent(amount, noMagicReaction, showMessage);
            RaiseEvent(target, ref ev);
        }

        public void HealToMax(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            skills.HP = skills.MaxHP;
            skills.MP = skills.MaxHP;
            skills.Stamina = skills.MaxStamina;
        }

        public void HealHP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            amount = Math.Clamp(amount, 0, Math.Max(skills.MaxHP - skills.HP, 0));
            skills.HP += amount;

            var ev = new AfterHealEvent(uid, HealType.HP, amount, showMessage);
            RaiseEvent(uid, ref ev);
        }

        public void HealMP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            amount = Math.Clamp(amount, 0, Math.Max(skills.MaxMP - skills.MP, 0));
            skills.MP += amount;

            var ev = new AfterHealEvent(uid, HealType.MP, amount, showMessage);
            RaiseEvent(uid, ref ev);
        }

        public void HealStamina(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            amount = Math.Clamp(amount, 0, Math.Max(skills.MaxStamina - skills.Stamina, 0));
            skills.Stamina += amount;

            var ev = new AfterHealEvent(uid, HealType.Stamina, amount, showMessage);
            RaiseEvent(uid, ref ev);
        }
    }

    public record struct DamageHPResult(bool WasKilled, int BaseDamage, int FinalDamage);

    public enum DamageHPMessageTense
    {
        // "...was killed."
        Passive,

        // "...and kills them."
        Active
    }

    public sealed class DamageHPExtraArgs
    {
        public int OriginalDamage { get; set; }

        /// <summary>
        /// Number of recursive calls to DamageHP().
        /// This is for damage types that themselves call DamageHP() somewhere,
        /// so that certain effects like splitting up monsters (bubbles, etc.) are
        /// not applied twice.
        /// </summary>
        public int DamageSubLevel { get; set; } = 0;

        // The following properties are only used for printing the damage message.

        public bool ShowMessage { get; set; } = true;
        public EntityUid? Weapon { get; set; }
        public int AttackCount { get; set; }
        public DamageHPMessageTense MessageTense { get; set; } = DamageHPMessageTense.Passive;
        public PrototypeId<SkillPrototype>? AttackSkill { get; set; }
        public bool IsThirdPerson { get; set; }
        public bool NoAttackText { get; set; }
    }
}