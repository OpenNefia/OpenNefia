using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Combat;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Resists;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Effects.New;

namespace OpenNefia.Content.Enchantments
{
    public sealed class VanillaAmmoEnchantmentsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;

        #region Elona.Rapid

        public void Rapid_CalcAttackDamage(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentCalcAttackDamageEvent ev)
        {
            ev.OutDamage /= 2;
        }

        public void Rapid_BeforeRangedAttack(AmmoEnchantmentPrototype proto, P_AmmoEnchantmentBeforeRangedAttackEvent ev)
        {
            ev.Handled = true;
            EntityUid? target = ev.Target;

            for (var i = 0; i < 3; i++)
            {
                _combat.PhysicalAttack(ev.Attacker, target.Value, ev.AttackSkill, ev.Weapon, true);

                if (!IsAlive(ev.Target) && !_targeting.TrySearchForTarget(ev.Attacker, out target, silent: true))
                    break;
            }
        }

        #endregion

        #region Elona.Vopal

        public void Vorpal_CalcAttackDamage(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentCalcAttackDamageEvent ev)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:325 		if ammoProc=encAmmoVopal : pierce=60 		:if sync( ...
            _mes.Display(Loc.GetString("Elona.Combat.RangedAttack.Vorpal"), UiColors.MesYellow, entity: ev.Attacker);

            ev.OutStrength.PierceRate = 60;
            // <<<<<<<< elona122/shade2/calculation.hsp:325 		if ammoProc=encAmmoVopal : pierce=60 		:if sync( ...
        }

        #endregion

        #region Elona.Time

        public void Time_AfterRangedAttackHit(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentAfterRangedAttackHitEvent ev)
        {
            _mes.Display("TODO time stop", UiColors.MesYellow);
        }

        #endregion

        #region Elona.Magic

        public void Magic_CalcAttackDamage(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentCalcAttackDamageEvent ev)
        {
            ev.OutDamage /= 10;
        }

        public void Magic_AfterRangedAttackHit(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentAfterRangedAttackHitEvent ev)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1476 	if ammoProc=encAmmoMagic{ ..
            if (IsAlive(ev.PhysicalAttackArgs.Target))
            {
                var elementID = _resists.PickRandomElement().GetStrongID();
                var elementPower = _skills.Level(ev.Attacker, ev.PhysicalAttackArgs.AttackSkill) * 10 + 100;

                _damage.DamageHP(ev.PhysicalAttackArgs.Target, ev.PhysicalAttackArgs.RawDamage.OriginalDamage * 2 / 3, ev.Attacker, new ElementalDamageType(elementID, elementPower), new DamageHPExtraArgs() { DamageSubLevel = 1 });
            }
            // <<<<<<<< elona122/shade2/action.hsp:1478 	} ..
        }

        #endregion

        #region Elona.Bomb

        public void Bomb_BeforeRangedAttack(AmmoEnchantmentPrototype proto, P_AmmoEnchantmentBeforeRangedAttackEvent ev)
        {
            ev.Handled = true;
            var anim = _combat.GetRangedAttackAnimation(ev.Attacker, ev.Target, ev.AttackSkill, ev.Weapon);
            if (anim != null)
                _mapDrawables.Enqueue(anim, ev.Attacker);

            var power = _skills.Level(ev.Attacker, ev.AttackSkill) * 8 + 10;
            _newEffects.Apply(ev.Attacker, ev.Target, null, Protos.Effect.SpellMagicStorm, power: power);
        }

        #endregion

        #region Elona.Burst

        public void Burst_CalcAttackDamage(AmmoEnchantmentPrototype proto, ref P_AmmoEnchantmentCalcAttackDamageEvent ev)
        {
            ev.OutDamage /= 3;
        }

        public void Burst_BeforeRangedAttack(AmmoEnchantmentPrototype proto, P_AmmoEnchantmentBeforeRangedAttackEvent ev)
        {
            ev.Handled = true;
            var i = 1;
            var max = 10;
            while (i < max)
            {
                var consider = true;
                var targets = _targeting.FindTargets(ev.Attacker);
                if (targets.Count == 0)
                    return;

                var newTarget = _rand.Pick(targets).Owner;
                if (_factions.GetRelationTowards(ev.Attacker, newTarget) >= Relation.Neutral && i > 1)
                {
                    consider = false;
                    if (_rand.OneIn(5))
                        max++;
                }

                if (consider)
                {
                    _combat.PhysicalAttack(ev.Attacker, newTarget, ev.AttackSkill, ev.Weapon, true);
                }

                i++;
            }
        }

        #endregion
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(AmmoEnchantmentPrototype))]
    public struct P_AmmoEnchantmentCalcAttackDamageEvent
    {
        public EntityUid Attacker { get; }
        public EntityUid Target { get; }
        public EntityUid Weapon { get; }
        public EntityUid Ammo { get; }
        public EntityUid AmmoEnchantment { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public int AttackCount { get; }
        public bool IsCritical { get; }

        public AttackStrength OutStrength { get; }
        public int OutDamage { get; set; }
        public bool OutVorpal { get; set; } = false;

        public P_AmmoEnchantmentCalcAttackDamageEvent(EntityUid attacker, EntityUid target, EntityUid weapon, EntityUid ammo, EntityUid ammoEnchantment, PrototypeId<SkillPrototype> attackSkill, int attackCount, bool isCritical, AttackStrength strength, int damage)
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            Ammo = ammo;
            AmmoEnchantment = ammoEnchantment;
            AttackSkill = attackSkill;
            AttackCount = attackCount;
            IsCritical = isCritical;
            OutStrength = strength;
            OutDamage = damage;
        }
    }

    [PrototypeEvent(typeof(AmmoEnchantmentPrototype))]
    public sealed class P_AmmoEnchantmentBeforeRangedAttackEvent : HandledEntityEventArgs
    {
        public EntityUid Attacker { get; }
        public EntityUid Target { get; }
        public EntityUid Weapon { get; }
        public EntityUid Ammo { get; }
        public EntityUid AmmoEnchantment { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public bool IsCritical { get; }

        public P_AmmoEnchantmentBeforeRangedAttackEvent(EntityUid attacker, EntityUid target, EntityUid weapon, EntityUid ammo, EntityUid ammoEnchantment, PrototypeId<SkillPrototype> attackSkill)
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            Ammo = ammo;
            AmmoEnchantment = ammoEnchantment;
            AttackSkill = attackSkill;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(AmmoEnchantmentPrototype))]
    public struct P_AmmoEnchantmentAfterRangedAttackHitEvent
    {
        public EntityUid Attacker { get; }
        public AfterPhysicalAttackHitEventArgs PhysicalAttackArgs { get; }

        public P_AmmoEnchantmentAfterRangedAttackHitEvent(EntityUid attacker, AfterPhysicalAttackHitEventArgs args)
        {
            Attacker = attacker;
            PhysicalAttackArgs = args;
        }
    }
}