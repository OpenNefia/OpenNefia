using NuGet.DependencyResolver;
using OpenNefia.Content.Activity;
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
        void DamageMP(EntityUid target, int amount, bool noMagicReaction = false, bool showMessage = true, SkillsComponent? skills = null);
        void DamageStamina(EntityUid target, int amount, bool showMessage = true, SkillsComponent? skills = null);

        void Kill(EntityUid target, EntityUid? attacker = null, IDamageType? damageType = null, DamageHPExtraArgs? extraArgs = null, SkillsComponent? skills = null);

        void HealToMax(EntityUid uid, SkillsComponent? skills = null);
        void HealHP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealMP(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);
        void HealStamina(EntityUid uid, int amount, bool showMessage = true, SkillsComponent? skills = null);

        bool DoStaminaCheck(EntityUid uid, int baseCost, PrototypeId<SkillPrototype>? relatedSkillId = null, SkillsComponent? skills = null);

        /// <summary>
        /// Runs some extra events as if <see cref="attacker"/> killed <see cref="target"/>,
        /// including karma loss, quest quotas, etc.
        /// </summary>
        void RunCheckKillEvents(EntityUid target, EntityUid attacker);

        DamageHPMessageTense GetDamageMessageTense(EntityUid target);
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

            skills.MP = Math.Max(skills.MP - amount, -999999);

            var ev = new AfterDamageMPEvent(amount, noMagicReaction, showMessage);
            RaiseEvent(target, ref ev);
        }

        public void DamageStamina(EntityUid target, int amount, bool showMessage = true, SkillsComponent? skills = null)
        {
            if (!Resolve(target, ref skills))
                return;

            skills.Stamina = Math.Max(skills.Stamina - amount, -100);

            var ev = new AfterDamageStaminaEvent(amount, showMessage);
            RaiseEvent(target, ref ev);
        }

        public void Kill(EntityUid target, EntityUid? attacker = null, IDamageType? damageType = null, DamageHPExtraArgs? extraArgs = null, SkillsComponent? skills = null)
        {
            if (!Resolve(target, ref skills))
                return;

            var finalDamage = skills.MaxHP;

            var ev = new EntityKilledEvent(finalDamage, finalDamage, attacker, damageType, extraArgs ?? new());
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

        public bool DoStaminaCheck(EntityUid uid, int baseCost, PrototypeId<SkillPrototype>? relatedSkillId = null, SkillsComponent? skills = null)
        {
            if (!_gameSession.IsPlayer(uid) || !Resolve(uid, ref skills))
                return true;

            var staminaCost = baseCost / 2 + 1;
            if (skills.Stamina < 50 && skills.Stamina < _rand.Next(75))
            {
                DamageStamina(uid, staminaCost, skills: skills);
                return false;
            }

            DamageStamina(uid, _rand.Next(staminaCost) + staminaCost, skills: skills);

            if (relatedSkillId != null)
                _skills.GainSkillExp(uid, relatedSkillId.Value, 25);

            return true;
        }

        public DamageHPMessageTense GetDamageMessageTense(EntityUid target)
        {
            return _parties.IsInPlayerParty(target) ? DamageHPMessageTense.Passive : DamageHPMessageTense.Active;
        }
    }

    /// <param name="WasKilled">True if the target was killed.</param>
    /// <param name="BaseDamage">Damage passed to DamageHP(). Same as <see cref="Combat.AttackDamage.TotalDamage"/>.</param>
    /// <param name="FinalDamage">Final damage calculated by DamageHP events.</param>
    public record struct DamageHPResult(bool WasKilled, int BaseDamage, int FinalDamage);

    public enum DamageHPMessageTense
    {
        /// <summary>
        /// For use with ally targets.
        /// "...was killed."
        /// </summary>
        Passive,

        /// <summary>
        /// For use with enemy targets.
        /// "...and kills them."
        /// </summary>
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

        /// <summary>
        /// Affects the tense of the damage/kill messages. To explain:
        /// - The player can attack with a weapon that does cold damage.
        ///   The message will be "You attack the putit and {transform} him into an ice sculpture."
        /// - The player can cast a bolt that does cold damage.
        ///   Even though the player is the attacker, the message should become
        ///   "The bolt hits the putit and {transforms} him into an ice sculpture."
        /// So by setting this flag to <c>true</c>, it's like saying the player isn't the
        /// one directly responsible for the attack in the message tense (even though
        /// they are still set as the attacker for gameplay purposes).
        /// </summary>
        public bool AttackerIsMessageSubject { get; set; } = true;

        public bool NoAttackText { get; set; }
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

        /// <summary>
        /// If false, things like damage popups shouldn't be shown.
        /// </summary>
        public bool ShowMessage { get; }

        public AfterDamageMPEvent(int amount, bool noMagicReaction, bool showMessage)
        {
            Amount = amount;
            NoMagicReaction = noMagicReaction;
            ShowMessage = showMessage;
        }
    }

    [ByRefEvent]
    public struct AfterDamageStaminaEvent
    {
        public int Amount { get; }

        /// <summary>
        /// If false, things like damage popups shouldn't be shown.
        /// </summary>
        public bool ShowMessage { get; }

        public AfterDamageStaminaEvent(int amount, bool showMessage)
        {
            Amount = amount;
            ShowMessage = showMessage;
        }
    }
}