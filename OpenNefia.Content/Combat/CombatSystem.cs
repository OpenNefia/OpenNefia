using OpenNefia.Content.Activity;
using OpenNefia.Content.Charas;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.Damage;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Enchantments;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Weight;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.Mount;

namespace OpenNefia.Content.Combat
{
    public interface ICombatSystem
    {
        EquipState GetEquipState(EntityUid ent);
        TurnResult? MeleeAttack(EntityUid attacker, EntityUid target);
        bool TryGetRangedWeaponAndAmmo(EntityUid entity, [NotNullWhen(true)] out EntityUid? rangedWeapon, [NotNullWhen(true)] out EntityUid? ammo);
        bool TryGetRangedWeaponAndAmmo(EntityUid entity, [NotNullWhen(true)] out EntityUid? rangedWeapon, out EntityUid? ammo, [NotNullWhen(false)] out string? errorReason);
        bool TryGetAmmoForWeapon(EntityUid entity, EntityUid weapon, [NotNullWhen(true)] out EntityUid? ammo);
        TurnResult? RangedAttack(EntityUid attacker, EntityUid target, EntityUid rangedWeapon);
        void PhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, ref int attackCount, EntityUid? weapon = null, bool isRanged = false);
        void PhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon = null, bool isRanged = false);
        IMapDrawable? GetRangedAttackAnimation(MapCoordinates start, MapCoordinates end, PrototypeId<SkillPrototype> attackSkill, EntityUid weapon);
        IMapDrawable? GetRangedAttackAnimation(EntityUid start, EntityUid end, PrototypeId<SkillPrototype> attackSkill, EntityUid weapon);
        bool TryGetActiveAmmoEnchantment(EntityUid attacker, EntityUid rangedWeapon, [NotNullWhen(true)] out AmmoComponent? ammoComp, [NotNullWhen(true)] out EncAmmoComponent? ammoEncComp);
        bool IsEquippedInMainHand(EntityUid item, EntityUid equipper, EquipSlotsComponent? equipSlots = null);
        bool IsSuitableForWieldingTwoHanded(EntityUid weapon);
        bool IsSuitableForDualWielding(EntityUid weapon, bool mainHand);
        bool IsSuitableForWieldingWhileRiding(EntityUid weapon);
    }

    public sealed partial class CombatSystem : EntitySystem, ICombatSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEquipmentSystem _equip = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;

        public override void Initialize()
        {
            #region Physical attack events
            SubscribeEntity<BeforePhysicalAttackEventArgs>(BlockPhysicalAttackFear, priority: EventPriorities.VeryHigh);

            SubscribeEntity<CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyEquipState, priority: EventPriorities.VeryHigh);
            SubscribeEntity<CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyAttackCount, priority: EventPriorities.VeryHigh);

            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitStatusEffects, priority: EventPriorities.VeryHigh);
            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitGreaterEvasion, priority: EventPriorities.VeryHigh);
            SubscribeEntity<CalcPhysicalAttackHitEvent>(HandleCalcHitCriticals, priority: EventPriorities.VeryHigh);

            SubscribeComponent<SkillsComponent, EntityRefreshEvent>(ApplyShieldPVBonus, priority: EventPriorities.VeryHigh);
            SubscribeComponent<EquipStatsComponent, EntityRefreshEvent>(AdjustExtraMeleeAndCriticalChances, priority: EventPriorities.High);
            SubscribeComponent<EquipStatsComponent, CalcFinalDamageEvent>(ProcDammageImmunity, priority: EventPriorities.Lowest);

            SubscribeComponent<SkillsComponent, AfterPhysicalAttackHitEventArgs>(GainCombatSkillExperienceOnHit, priority: EventPriorities.VeryHigh);
            SubscribeComponent<SkillsComponent, AfterPhysicalAttackMissEventArgs>(GainCombatSkillExperienceOnMiss, priority: EventPriorities.VeryHigh);
            SubscribeComponent<EquipStatsComponent, AfterPhysicalAttackHitEventArgs>(ProcDamageReflection, priority: EventPriorities.High);
            SubscribeEntity<WasEquippedInMenuEvent>(ShowEquipmentSuitabilityMessages);
            #endregion
        }

        public EquipState GetEquipState(EntityUid ent)
        {
            var attackCount = 0;
            var isWieldingShield = false;
            var isWieldingTwoHanded = false;
            var isDualWielding = false;

            foreach (var equipSlot in _equipSlots.GetEquipSlots(ent))
            {
                if (!_equipSlots.TryGetContainerForEquipSlot(ent, equipSlot, out var container))
                    continue;

                if (!EntityManager.IsAlive(container.ContainedEntity))
                    continue;

                if (TryComp<EquipmentComponent>(container.ContainedEntity.Value, out var equip))
                {
                    // >>>>>>>> elona122/shade2/calculation.hsp:442 		if cdata(cnt,r1)/extBody=bodyHand:attackNum++ ...
                    if (equip.EquipSlots.Contains(Protos.EquipSlot.Hand))
                        attackCount++;
                    // <<<<<<<< elona122/shade2/calculation.hsp:442 		if cdata(cnt,r1)/extBody=bodyHand:attackNum++ ...

                    // >>>>>>>> elona122/shade2/calculation.hsp:435 	if iSkillRef(rp)=rsShield : if cAttackStyle(r1)&s ...
                    if (_tags.HasTag(equip.Owner, Protos.Tag.ItemCatEquipShield))
                        isWieldingShield = true;
                    // <<<<<<<< elona122/shade2/calculation.hsp:435 	if iSkillRef(rp)=rsShield : if cAttackStyle(r1)&s ...
                }
            }

            // >>>>>>>> elona122/shade2/calculation.hsp:528 		}else{ ...
            if (!isWieldingShield)
            {
                if (attackCount == 1)
                    isWieldingTwoHanded = true;
                else if (attackCount > 0)
                    isDualWielding = true;
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:530 		} ...

            return new(attackCount, isWieldingShield, isWieldingTwoHanded, isDualWielding);
        }

        public bool IsSuitableForWieldingTwoHanded(EntityUid weapon)
        {
            if (!TryComp<WeightComponent>(weapon, out var weight))
                return false;

            return weight.Weight.Buffed >= EquipmentWeightConsts.TwoHandedMinWeaponWeight;
        }

        public bool IsSuitableForDualWielding(EntityUid weapon, bool mainHand)
        {
            if (!TryComp<WeightComponent>(weapon, out var weight))
                return true;

            if (mainHand)
                return weight.Weight.Buffed < EquipmentWeightConsts.DualWieldingMainHandMaxWeaponWeight;
            else
                return weight.Weight.Buffed < EquipmentWeightConsts.DualWieldingSubHandMaxWeaponWeight;
        }

        public bool IsSuitableForWieldingWhileRiding(EntityUid weapon)
        {
            if (!TryComp<WeightComponent>(weapon, out var weight))
                return true;

            return weight.Weight.Buffed < EquipmentWeightConsts.RidingMaxWeaponWeight;
        }

        public bool IsEquippedInMainHand(EntityUid item, EntityUid equipper, EquipSlotsComponent? equipSlots = null)
        {
            if (!Resolve(equipper, ref equipSlots))
                return false;

            if (!_equipSlots.TryGetSlotEquippedOn(item, out _, out var found))
                return false;

            var first = equipSlots.EquipSlots.FirstOrDefault(x => x.ID == found.ID);
            return found == first;
        }

        private void ShowEquipmentSuitabilityMessages(EntityUid item, WasEquippedInMenuEvent args)
        {
            // >>>>>>>> elona122/shade2/command.hsp:3171 			if (cdata(body,cc)/extBody)=bodyHand: gosub *sh ...
            // TODO maybe check for WeaponComponent instead?
            if (!_equipSlots.CanEquipOn(item, Protos.EquipSlot.Hand))
                return;
            // <<<<<<<< elona122/shade2/command.hsp:3171 			if (cdata(body,cc)/extBody)=bodyHand: gosub *sh ...

            // >>>>>>>> elona122/shade2/command.hsp:3052 *show_weaponStat ...
            var equipState = GetEquipState(args.EquipTarget);
            var actor = args.Equipee;
            var target = args.EquipTarget;

            if (equipState.IsWieldingTwoHanded)
            {
                if (IsSuitableForWieldingTwoHanded(item))
                {
                    _mes.Display(Loc.GetString("Elona.Equipment.Suitability.TwoHand.FitsWell",
                        ("actor", actor),
                        ("target", target),
                        ("item", item)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Equipment.Suitability.TwoHand.TooLight",
                        ("actor", actor),
                        ("target", target),
                        ("item", item)));
                }
            }
            if (equipState.IsDualWielding)
            {
                var isMainHand = IsEquippedInMainHand(item, target);
                if (!IsSuitableForDualWielding(item, isMainHand))
                {
                    if (isMainHand)
                    {
                        _mes.Display(Loc.GetString("Elona.Equipment.Suitability.DualWield.TooHeavy.MainHand",
                            ("actor", actor),
                            ("target", target),
                            ("item", item)));
                    }
                    else
                    {
                        _mes.Display(Loc.GetString("Elona.Equipment.Suitability.DualWield.TooHeavy.MainHand",
                            ("actor", actor),
                            ("target", target),
                            ("item", item)));
                    }
                }
            }
            if (_mounts.IsMounting(args.EquipTarget))
            {
                if (!IsSuitableForWieldingWhileRiding(item))
                {
                    _mes.Display(Loc.GetString("Elona.Equipment.Suitability.Riding.TooHeavy",
                        ("actor", actor),
                        ("target", target),
                        ("item", item)));
                }
            }
            // <<<<<<<< elona122/shade2/command.hsp:3074 	return ...
        }

        private void ApplyShieldPVBonus(EntityUid uid, SkillsComponent skills, ref EntityRefreshEvent args)
        {
            if (!HasComp<CharaComponent>(uid))
                return;

            var state = GetEquipState(uid);
            if (state.IsWieldingShield && skills.PV.Buffed > 0)
            {
                skills.PV.Buffed *= (int)((120 + Math.Sqrt(skills.Level(Protos.Skill.Shield)) * 2) / 100);
            }
        }

        private void AdjustExtraMeleeAndCriticalChances(EntityUid uid, EquipStatsComponent equipStats, ref EntityRefreshEvent args)
        {
            if (!HasComp<CharaComponent>(uid))
                return;

            // >>>>>>>> elona122/shade2/calculation.hsp:552 	if cAttackStyle(r1)&styleTwoWield:cExtraMelee(r1) ...
            var state = GetEquipState(uid);

            if (state.IsDualWielding)
            {
                equipStats.ExtraMeleeAttackRate.Buffed += (MathF.Sqrt(_skills.Level(uid, Protos.Skill.DualWield)) * 3f / 2f + 4f) / 100f;
            }

            // TODO make into double
            if (equipStats.CriticalRate.Buffed > 30)
            {
                equipStats.HitBonus.Buffed += (equipStats.CriticalRate.Buffed - 30) * 2;
                equipStats.CriticalRate.Buffed = 30;
            }
            // <<<<<<<< elona122/shade2/calculation.hsp:553 	if cCritChance(r1)>30:cATK(r1)+=(cCritChance(r1)- ...
        }

        private void ProcDammageImmunity(EntityUid uid, EquipStatsComponent equipStats, ref CalcFinalDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1466 	if cImmuneDamage(tc)>0:if cImmuneDamage(tc)>rnd(1 ...
            if (equipStats.DamageImmunityRate.Buffed > 0 && _rand.Prob(equipStats.DamageImmunityRate.Buffed))
            {
                args.OutFinalDamage = 0;
            }
            // <<<<<<<< elona122/shade2/chara_func.hsp:1466 	if cImmuneDamage(tc)>0:if cImmuneDamage(tc)>rnd(1 ...
        }

        public bool IsMeleeWeapon(EntityUid ent)
        {
            return HasComp<WeaponComponent>(ent)
                && !HasComp<RangedWeaponComponent>(ent);
        }

        public bool IsRangedWeapon(EntityUid ent)
        {
            return HasComp<WeaponComponent>(ent)
                && HasComp<RangedWeaponComponent>(ent);
        }

        public bool IsAmmo(EntityUid ent)
        {
            return HasComp<AmmoComponent>(ent);
        }

        public bool IsArmor(EntityUid ent)
        {
            return HasComp<ArmorComponent>(ent);
        }

        public IList<EntityUid> GetMeleeWeapons(EntityUid ent)
        {
            return _equipSlots.EnumerateEquippedEntities(ent).Where(IsMeleeWeapon).ToList();
        }

        public bool TryGetRangedWeaponAndAmmo(EntityUid entity, [NotNullWhen(true)] out EntityUid? rangedWeapon, out EntityUid? ammo, [NotNullWhen(false)] out string? errorReason)
        {
            // >>>>>>>> elona122/shade2/command.hsp:4284 *FindRangeWeapon ...
            rangedWeapon = _equipSlots.EnumerateEquippedEntities(entity)
                .Where(IsRangedWeapon)
                .FirstOrNull();

            if (!IsAlive(rangedWeapon))
            {
                rangedWeapon = null;
                ammo = null;
                errorReason = RangedWeaponQueryErrors.NoRangedWeapon;
                return false;
            }

            var ranged = Comp<RangedWeaponComponent>(rangedWeapon.Value);
            var weaponComp = Comp<WeaponComponent>(rangedWeapon.Value);
            var needsAmmo = weaponComp.WeaponSkill != Protos.Skill.Throwing;

            if (!needsAmmo)
            {
                ammo = null;
                errorReason = null;
                return true;
            }

            if (!TryGetAmmoForWeapon(entity, rangedWeapon.Value, out ammo))
            {
                rangedWeapon = null;
                ammo = null;
                errorReason = RangedWeaponQueryErrors.NoAmmo;
                return false;
            }

            var ammoComp = Comp<AmmoComponent>(ammo.Value);

            if (ammoComp.AmmoSkill != weaponComp.WeaponSkill)
            {
                rangedWeapon = null;
                ammo = null;
                errorReason = RangedWeaponQueryErrors.WrongAmmoType;
                return false;
            }

            errorReason = null;
            return true;
            // <<<<<<<< elona122/shade2/command.hsp:4296 	return true ...
        }

        public bool TryGetRangedWeaponAndAmmo(EntityUid entity, [NotNullWhen(true)] out EntityUid? rangedWeapon, [NotNullWhen(true)] out EntityUid? ammo)
            => TryGetRangedWeaponAndAmmo(entity, out rangedWeapon, out ammo, out _);

        public bool TryGetAmmoForWeapon(EntityUid entity, EntityUid weapon, [NotNullWhen(true)] out EntityUid? ammo)
        {
            if (!IsAlive(weapon) || !IsRangedWeapon(weapon))
            {
                ammo = null;
                return false;
            }

            if (TryComp<WeaponComponent>(weapon, out var weaponComp))
            {
                ammo = _equipSlots.EnumerateEquippedEntities(entity)
                    .Where(ent => IsAmmo(ent) && weaponComp.WeaponSkill == Comp<AmmoComponent>(ent).AmmoSkill)
                    .FirstOrNull();

                if (ammo != null)
                    return true;
            }

            ammo = _equipSlots.EnumerateEquippedEntities(entity)
                .Where(ent => IsAmmo(ent))
                .FirstOrNull();
            return ammo != null;
        }

        public TurnResult? MeleeAttack(EntityUid attacker, EntityUid target)
        {
            var ev = new BeforeMeleeAttackEvent(target);
            if (Raise(attacker, ev))
                return ev.TurnResult;

            var weapons = GetMeleeWeapons(attacker);

            if (weapons.Count > 0)
            {
                var attackCount = 0;
                foreach (var weapon in weapons)
                {
                    var attackSkill = GetPhysicalAttackSkill(weapon);
                    PhysicalAttack(attacker, target, attackSkill, ref attackCount, weapon, isRanged: false);
                }
            }
            else
            {
                PhysicalAttack(attacker, target, Protos.Skill.MartialArts);
            }

            return TurnResult.Succeeded;
        }

        public bool TryGetActiveAmmoEnchantment(EntityUid attacker, EntityUid rangedWeapon, [NotNullWhen(true)] out AmmoComponent? ammoComp, [NotNullWhen(true)] out EncAmmoComponent? ammoEncComp)
        {
            if (TryGetAmmoForWeapon(attacker, rangedWeapon, out var ammo)
                && TryComp<AmmoComponent>(ammo, out ammoComp)
                && IsAlive(ammoComp.ActiveAmmoEnchantment)
                && TryComp<EncAmmoComponent>(ammoComp.ActiveAmmoEnchantment, out ammoEncComp))
            {
                return true;
            }

            ammoComp = null;
            ammoEncComp = null;
            return false;
        }

        public TurnResult? RangedAttack(EntityUid attacker, EntityUid target, EntityUid rangedWeapon)
        {
            // >>>>>>>> shade2/action.hsp:1148 *act_fire ..
            AmmoComponent? ammoComp = null;
            EncAmmoComponent? ammoEncComp = null;

            if (TryGetActiveAmmoEnchantment(attacker, rangedWeapon, out ammoComp, out ammoEncComp))
            {
                if (ammoEncComp.CurrentAmmoAmount <= 0)
                {
                    _mes.Display(Loc.GetString("Elona.Combat.RangedAttack.LoadNormalAmmo", ("attacker", attacker)));
                    ammoComp.ActiveAmmoEnchantment = null;
                    ammoEncComp = null;
                }
                else
                {
                    var ammoEncProto = _protos.Index(ammoEncComp.AmmoEnchantmentID);

                    if (_gameSession.IsPlayer(attacker) && TryComp<SkillsComponent>(attacker, out var skills))
                    {
                        if (skills.Stamina < FatigueThresholds.Light && skills.Stamina < _rand.Next((int)(FatigueThresholds.Light * 1.5f)))
                        {
                            _mes.Display(Loc.GetString("Elona.Common.TooExhausted"), entity: attacker);
                            _damage.DamageStamina(attacker, ammoEncProto.StaminaCost / 2 + 1);
                            return TurnResult.Failed;
                        }
                        _damage.DamageStamina(attacker, ammoEncProto.StaminaCost + 1);
                        ammoEncComp.CurrentAmmoAmount -= 1;
                    }
                }
            }

            if (!TryComp<WeaponComponent>(rangedWeapon, out var weaponComp))
            {
                Logger.WarningS("combat", $"No {nameof(WeaponComponent)} on {rangedWeapon}!");
                return TurnResult.Failed;
            }

            var weaponSkill = weaponComp.WeaponSkill;

            if (ammoComp != null && ammoEncComp != null)
            {
                var ev = new P_AmmoEnchantmentBeforeRangedAttackEvent(attacker, target, rangedWeapon, ammoComp.Owner, ammoEncComp.Owner, weaponSkill);
                _protos.EventBus.RaiseEvent(ammoEncComp.AmmoEnchantmentID, ev);

                if (ev.Handled)
                    return TurnResult.Succeeded;
            }

            PhysicalAttack(attacker, target, weaponSkill, rangedWeapon, isRanged: true);

            return TurnResult.Succeeded;
            // <<<<<<<< shade2/action.hsp:1195 	return ..
        }

        public PrototypeId<SkillPrototype> GetPhysicalAttackSkill(EntityUid? weapon, WeaponComponent? weaponComp = null)
        {
            if (weapon != null && Resolve(weapon.Value, ref weaponComp))
            {
                return weaponComp.WeaponSkill;
            }
            return Protos.Skill.MartialArts;
        }

        public IMapDrawable? GetRangedAttackAnimation(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid weapon)
            => GetRangedAttackAnimation(Spatial(attacker).MapPosition, Spatial(target).MapPosition, attackSkill, weapon);

        public IMapDrawable? GetRangedAttackAnimation(MapCoordinates start, MapCoordinates end, PrototypeId<SkillPrototype> attackSkill, EntityUid weapon)
        {
            if (!_vis.IsInWindowFov(start) && !_vis.IsInWindowFov(end))
                return null;

            // >>>>>>>> shade2/screen.hsp:654 	preparePicItem 6,aniCol ...
            PrototypeId<ChipPrototype> chipID;
            Color color = CompOrNull<ChipComponent>(weapon)?.Color ?? Color.White;
            PrototypeId<SoundPrototype> sound;
            PrototypeId<SoundPrototype>? impactSound = null;

            if (attackSkill == Protos.Skill.Bow)
            {
                chipID = Protos.Chip.ItemProjectileArrow;
                sound = Protos.Sound.Bow1;
            }
            else if (attackSkill == Protos.Skill.Crossbow)
            {
                chipID = Protos.Chip.ItemProjectileBolt;
                sound = Protos.Sound.Bow1;
            }
            else if (attackSkill == Protos.Skill.Firearm)
            {
                if (_tags.HasTag(weapon, Protos.Tag.ItemCatEquipRangedLaserGun))
                {
                    chipID = Protos.Chip.ItemProjectileLaser;
                    sound = Protos.Sound.Laser1;
                }
                else
                {
                    chipID = Protos.Chip.ItemProjectileBullet;
                    sound = Protos.Sound.Gun1;
                }
            }
            else
            {
                if (TryComp<ChipComponent>(weapon, out var chip))
                {
                    chipID = chip.ChipID;
                    color = chip.Color;
                }
                else
                {
                    chipID = Protos.Chip.Default;
                    color = Color.White;
                }
                sound = Protos.Sound.Throw1;
            }
            // <<<<<<<< shade2/screen.hsp:665 	if animeId=aniArrow	:snd seArrow1 ...

            if (TryComp<RangedWeaponComponent>(weapon, out var ranged))
            {
                chipID = ranged.AnimChip ?? chipID;
                color = ranged.AnimColor ?? color;
                sound = ranged.AnimSound?.GetSound() ?? sound;
                impactSound = ranged.AnimImpactSound?.GetSound() ?? impactSound;
            }

            return new RangedAttackMapDrawable(start, end, chipID, color, sound, impactSound);
        }

        /// <summary>
        /// Does one or more physical attacks, taking into account the extra melee/ranged attack
        /// chances.
        /// </summary>
        public void PhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon = null,
            bool isRanged = false)
        {
            var attackCount = 0;
            PhysicalAttack(attacker, target, attackSkill, ref attackCount, weapon, isRanged);
        }

        /// <summary>
        /// Does one or more physical attacks, taking into account the extra melee/ranged attack
        /// chances.
        /// </summary>
        public void PhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, ref int attackCount, EntityUid? weapon = null,
            bool isRanged = false)
        {
            bool going = true;

            while (going)
            {
                DoPhysicalAttack(attacker, target, attackSkill, weapon, attackCount, isRanged);

                // >>>>>>>> elona122/shade2/action.hsp:1383 	if extraAttack=false{ ...
                if (attackCount == 0)
                {
                    if (isRanged)
                    {
                        if (TryComp<EquipStatsComponent>(attacker, out var stats) && _rand.Prob(stats.ExtraRangedAttackRate.Buffed))
                        {
                            attackCount++;
                        }
                        else
                        {
                            going = false;
                        }
                    }
                    else
                    {
                        if (TryComp<EquipStatsComponent>(attacker, out var stats) && _rand.Prob(stats.ExtraMeleeAttackRate.Buffed))
                        {
                            attackCount++;
                        }
                        else
                        {
                            going = false;
                        }
                    }
                }
                else
                {
                    going = false;
                }
                // <<<<<<<< elona122/shade2/action.hsp:1389 	} ...
            }
        }

        /// <summary>
        /// Causes a physical attack.
        /// </summary>
        /// <param name="attacker">Attacking entity.</param>
        /// <param name="target">Target being attacked.</param>
        /// <param name="weapon">Weapon used in the attack, if any.</param>
        /// <param name="attackCount">
        /// Number of attacks so far.
        /// 
        /// The first weapon gets count 0, the second count 1, and so on. 
        /// </param>
        public void DoPhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon = null,
            int attackCount = 0, bool isRanged = false)
        {
            if (!EntityManager.IsAlive(attacker) || !EntityManager.IsAlive(target))
                return;

            var ev = new BeforePhysicalAttackEventArgs()
            {
                Target = target,
                Weapon = weapon,
                AttackCount = attackCount
            };

            if (Raise(attacker, ev))
                return;

            // >>>>>>>> shade2/action.hsp:1248 	if attackRange=true:call anime,(animeId=attackSki ..
            if (isRanged && weapon != null)
            {
                var startPos = Spatial(attacker).MapPosition;
                var endPos = Spatial(target).MapPosition;
                var cb = GetRangedAttackAnimation(startPos, endPos, attackSkill, weapon.Value);
                if (cb != null)
                    _mapDrawables.Enqueue(cb, startPos);






                var positions = new MapCoordinates[] { startPos, endPos };
                _mapDrawables.Enqueue(new MiracleMapDrawable(positions, Protos.Sound.Heal1, Protos.Sound.Pray2), startPos);
            }
            // <<<<<<<< shade2/action.hsp:1248 	if attackRange=true:call anime,(animeId=attackSki ..

            // >>>>>>>> shade2/action.hsp:1253 	hit=calcAttackHit() ..
            var hitResult = CalcPhysicalAttackHit(attacker, target, attackSkill, weapon, attackCount, isRanged);
            var didHit = hitResult == HitResult.Hit || hitResult == HitResult.CriticalHit;
            var isCritical = hitResult == HitResult.CriticalHit;

            if (didHit)
            {
                if (_gameSession.IsPlayer(attacker))
                {
                    if (isCritical)
                    {
                        _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.CriticalHit"), UiColors.MesRed);
                        _audio.Play(Protos.Sound.Atk2, target);
                    }
                    else
                    {
                        _audio.Play(Protos.Sound.Atk1, target);
                    }
                }

                var rawDamage = CalcPhysicalAttackRawDamage(attacker, target, attackSkill, weapon, attackCount, isRanged, isCritical);

                var playAnimation = _gameSession.IsPlayer(attacker) && _config.GetCVar(CCVars.AnimeAttackAnimation);
                if (playAnimation)
                {
                    var damagePercent = rawDamage.TotalDamage * 100 / (Comp<SkillsComponent>(target).MaxHP);
                    var anim = GetAttackAnimation(target, attackSkill, damagePercent, isCritical);
                    _mapDrawables.Enqueue(anim, Spatial(target).MapPosition);
                }

                IDamageType? damageType = null;

                if (weapon != null)
                {
                    if (CompOrNull<QualityComponent>(weapon.Value)?.Quality.Buffed >= Quality.Great && _rand.OneIn(5))
                    {
                        ShowWieldsProudlyMesssage(attacker, weapon.Value);
                    }
                }
                else
                {
                    if (TryComp<UnarmedDamageComponent>(attacker, out var unarmed))
                        damageType = unarmed.DamageType;
                }

                var damageHPResult = _damage.DamageHP(target, rawDamage.TotalDamage, attacker, damageType, new DamageHPExtraArgs()
                {
                    AttackCount = attackCount,
                    AttackSkill = attackSkill,
                    OriginalDamage = rawDamage.OriginalDamage,
                    Weapon = weapon,
                    MessageTense = _parties.IsInPlayerParty(target) ? DamageHPMessageTense.Passive : DamageHPMessageTense.Active
                });

                var evHit = new AfterPhysicalAttackHitEventArgs()
                {
                    Target = target,
                    Weapon = weapon,
                    AttackCount = attackCount,
                    AttackSkill = attackSkill,
                    IsRanged = isRanged,
                    HitResult = hitResult,
                    RawDamage = rawDamage,
                    FinalDamage = damageHPResult.FinalDamage
                };
                RaiseEvent(attacker, evHit);
            }
            else
            {
                if (_gameSession.IsPlayer(attacker))
                {
                    _audio.Play(Protos.Sound.Miss, target);
                }

                var evMiss = new AfterPhysicalAttackMissEventArgs()
                {
                    Target = target,
                    Weapon = weapon,
                    AttackCount = attackCount,
                    AttackSkill = attackSkill,
                    IsRanged = isRanged,
                    HitResult = hitResult
                };
                RaiseEvent(attacker, evMiss);
            }

            if (hitResult == HitResult.Miss)
            {
                ShowMissText(attacker, target, attackCount);
            }
            else if (hitResult == HitResult.Evade)
            {
                ShowEvadeText(attacker, target, attackCount);
            }

            _activities.InterruptActivity(target);
            // <<<<<<<< shade2/action.hsp:1292  ..
        }

        private void ShowWieldsProudlyMesssage(EntityUid attacker, EntityUid weapon)
        {
            // >>>>>>>> elona122/shade2/action.hsp:1262 		if attackSkill!rsMartial:if iQuality(cw)>=fixGre ...
            var quality = CompOrNull<QualityComponent>(weapon)?.Quality;
            var identifyState = CompOrNull<IdentifyComponent>(weapon)?.IdentifyState;
            var itemName = _displayNames.GetBaseName(weapon);
            string name;

            // Don't spoil the item's alias if not fully identified yet
            if (quality >= Quality.Great && identifyState >= IdentifyState.Full)
            {
                name = Loc.GetString("Elona.Item.NameModifiers.Article", ("name", itemName));
            }
            else
            {
                var alias = CompOrNull<AliasComponent>(weapon);
                if (alias?.Alias != null)
                    name = alias.Alias;
                else
                    name = Loc.GetString("Elona.Item.NameModifiers.Article", ("name", itemName));
            }
            name = Loc.GetString("Elona.Item.NameModifiers.Great", ("name", name));

            _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.WieldsProudly", ("wielder", attacker), ("itemName", name)), UiColors.MesSkyBlue, entity: attacker);
            // <<<<<<<< elona122/shade2/action.hsp:1271 			} ...
        }

        private void ShowMissText(EntityUid attacker, EntityUid target, int attackCount)
        {
            // >>>>>>>> shade2/action.hsp:1365 	if hit=atkEvade:if sync(cc){ ...
            if (!_vis.IsInWindowFov(attacker))
                return;

            var capitalize = true;

            if (attackCount > 0)
            {
                _mes.Display(Loc.GetString("Elona.Damage.Furthermore"));
                capitalize = false;
            }

            LocaleKey key;
            if (_parties.IsUnderlingOfPlayer(target))
            {
                key = "Elona.Combat.PhysicalAttack.Miss.Ally";
            }
            else
            {
                key = "Elona.Combat.PhysicalAttack.Miss.Other";
            }

            _mes.Display(Loc.GetString(key, ("attacker", attacker), ("target", target)), noCapitalize: !capitalize);
            // <<<<<<<< shade2/action.hsp:1368 		} ...
        }

        private void ShowEvadeText(EntityUid attacker, EntityUid target, int attackCount)
        {
            // >>>>>>>> shade2/action.hsp:1369 	if hit=atkEvadePlus:if sync(cc){ ...
            if (!_vis.IsInWindowFov(attacker))
                return;

            var capitalize = true;

            if (attackCount > 0)
            {
                _mes.Display(Loc.GetString("Elona.Damage.Furthermore"));
                capitalize = false;
            }

            LocaleKey key;
            if (_parties.IsUnderlingOfPlayer(target))
            {
                key = "Elona.Combat.PhysicalAttack.Evade.Ally";
            }
            else
            {
                key = "Elona.Combat.PhysicalAttack.Evade.Other";
            }

            _mes.Display(Loc.GetString(key, ("attacker", attacker), ("target", target)), noCapitalize: !capitalize);
            // <<<<<<<< shade2/action.hsp:1372 		} ...
        }

        private IMapDrawable GetAttackAnimation(EntityUid target, PrototypeId<SkillPrototype> attackSkill, int damagePercent, bool isCritical)
        {
            PrototypeId<AssetPrototype>? attackAnimAsset = null;
            if (_protos.TryGetExtendedData<SkillPrototype, ExtAttackAnim>(attackSkill, out var ext))
            {
                attackAnimAsset = ext.AttackAnim;
            }

            var hasStoneBlood = CompOrNull<StoneBloodComponent>(target)?.HasStoneBlood ?? false;
            PrototypeId<AssetPrototype> particleAsset;
            if (hasStoneBlood)
                particleAsset = Protos.Asset.MeleeAttackDebris;
            else
                particleAsset = Protos.Asset.MeleeAttackBlood;

            return new MeleeAttackMapDrawable(particleAsset, attackAnimAsset, damagePercent, isCritical);
        }

        public HitResult CalcPhysicalAttackHit(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged)
        {
            var toHit = CalcPhysicalAttackAccuracy(attacker, target, attackSkill, weapon, attackCount, isRanged, considerDistance: true);
            var evasion = CalcPhysicalAttackEvasion(attacker, target, attackSkill, weapon, attackCount, isRanged);

            var ev = new CalcPhysicalAttackHitEvent(target, attackSkill, weapon, attackCount, isRanged, toHit, evasion);
            RaiseEvent(attacker, ref ev);

            if (ev.OutHitResult != null)
                return ev.OutHitResult.Value;

            toHit = ev.OutAccuracy;
            evasion = ev.OutEvasion;

            // >>>>>>>> elona122/shade2/calculation.hsp:241 	if toHit<1	:return atkEvade ...
            if (toHit < 1)
                return HitResult.Miss;
            else if (evasion < 1)
                return HitResult.Hit;
            else if (_rand.Next(toHit) > _rand.Next(evasion * 3 / 2))
                return HitResult.Hit;
            // <<<<<<<< elona122/shade2/calculation.hsp:244 	if rnd(toHit) > rnd(evade*3/2) : return atkHit ...

            return HitResult.Miss;
        }

        private int CalcBasePhysicalAttackAccuracy(EntityUid attacker, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon)
        {
            var equipState = GetEquipState(attacker);

            // >>>>>>>> elona122/shade2/calculation.hsp:162 	if attackSkill = rsMartial{ ...
            int accuracy;
            if (weapon != null)
            {
                if (!TryComp<WeaponComponent>(weapon.Value, out var weaponComp))
                    return 0;

                // XXX: I believe that attackSkill always gets set to the weapon's skill
                // in vanilla. (attackSkill=iSkillRef(cw) in HSP.) With weapons now being
                // able to act as both a melee and ranged weapon, it's difficult to tell
                // which weapon skill should be used for calculating accuracy.
                var weaponSkill = weaponComp.WeaponSkill;

                accuracy = _skills.Level(attacker, Protos.Skill.AttrDexterity) / 4
                    + _skills.Level(attacker, weaponSkill) / 3
                    + _skills.Level(attacker, attackSkill)
                     + 50
                     + CompOrNull<EquipStatsComponent>(attacker)?.HitBonus.Buffed ?? 0
                     + CompOrNull<EquipStatsComponent>(weapon.Value)?.HitBonus.Buffed ?? 0;

                if (TryGetAmmoForWeapon(attacker, weapon.Value, out var ammo) && TryComp<EquipStatsComponent>(ammo.Value, out var ammoStats))
                    accuracy += ammoStats.HitBonus.Buffed;
            }
            else
            {
                accuracy = _skills.Level(attacker, Protos.Skill.AttrDexterity) / 5
                    + _skills.Level(attacker, Protos.Skill.AttrStrength) / 2
                    + _skills.Level(attacker, attackSkill)
                    + 50;

                if (equipState.IsWieldingShield)
                    accuracy = accuracy * 100 / 130;

                accuracy = accuracy
                    + _skills.Level(attacker, Protos.Skill.AttrDexterity) / 5
                    + _skills.Level(attacker, Protos.Skill.AttrStrength) / 10
                    + CompOrNull<EquipStatsComponent>(attacker)?.HitBonus.Buffed ?? 0;
            }

            return accuracy;
            // <<<<<<<< elona122/shade2/calculation.hsp:172 		} ...
        }

        private int CalcPhysicalAttackAccuracy(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool considerDistance)
        {
            var baseAccuracy = CalcBasePhysicalAttackAccuracy(attacker, attackSkill, weapon);

            var ev = new CalcPhysicalAttackAccuracyEvent(target, attackSkill, weapon, attackCount, isRanged, considerDistance, baseAccuracy);
            RaiseEvent(attacker, ref ev);
            return ev.OutAccuracy;
        }

        private int CalcBasePhysicalAttackEvasion(EntityUid target)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:212 	evade	= sPER(tc)/3 + sEvade(tc) + cDV(tc) +25 ...
            return _skills.Level(target, Protos.Skill.AttrPerception) / 3
                + _skills.Level(target, Protos.Skill.Evasion)
                + CompOrNull<EquipStatsComponent>(target)?.DV.Buffed ?? 0
                + 25;
            // <<<<<<<< elona122/shade2/calculation.hsp:212 	evade	= sPER(tc)/3 + sEvade(tc) + cDV(tc) +25 ...
        }

        private int CalcPhysicalAttackEvasion(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged)
        {
            int baseEvasion = CalcBasePhysicalAttackEvasion(target);

            var ev = new CalcPhysicalAttackEvasionEvent(attacker, attackSkill, weapon, attackCount, isRanged, baseEvasion);
            RaiseEvent(target, ref ev);

            return ev.OutEvasion;
        }

        private AttackStrength CalcDefaultAttackStrength(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon)
        {
            // >>>>>>>> shade2/calculation.hsp:256 		dmgFix	= cDmg(cc) + iDmg(cw) +iLevel(cw)+(iStatu ..
            if (weapon == null || !TryComp<WeaponComponent>(weapon.Value, out var weaponComp))
                return new(new Dice(0, 0, 0), 0, 0);

            var diceBonus = CompOrNull<EquipStatsComponent>(attacker)?.DamageBonus.Buffed ?? 0
                + CompOrNull<EquipStatsComponent>(weapon)?.DamageBonus.Buffed ?? 0
                + CompOrNull<BonusComponent>(weapon)?.Bonus ?? 0;

            if (CompOrNull<CurseStateComponent>(weapon.Value)?.CurseState == CurseState.Blessed)
                diceBonus += 1;

            var diceX = weaponComp.DiceX.Buffed;
            var diceY = weaponComp.DiceY.Buffed;
            var pierceRate = weaponComp.PierceRate.Buffed;
            var weaponSkill = weaponComp.WeaponSkill;
            float multiplier;

            if (TryGetAmmoForWeapon(attacker, weapon.Value, out var ammo))
            {
                var ammoComp = Comp<AmmoComponent>(ammo.Value);
                diceBonus += CompOrNull<EquipStatsComponent>(ammo.Value)?.DamageBonus.Buffed ?? 0
                    + ammoComp.DiceX.Buffed * ammoComp.DiceY.Buffed / 2;
                multiplier = 0.5f + (_skills.Level(attacker, Protos.Skill.AttrPerception)
                    + _skills.Level(attacker, weaponSkill) / 5
                    + _skills.Level(attacker, attackSkill) / 5
                    + _skills.Level(attacker, Protos.Skill.Marksman) * 3 / 2)
                    / 40.0f;
            }
            else
            {
                multiplier = 0.6f + (_skills.Level(attacker, Protos.Skill.AttrStrength)
                    + _skills.Level(attacker, weaponSkill) / 5
                    + _skills.Level(attacker, attackSkill) / 5
                    + _skills.Level(attacker, Protos.Skill.Tactics) * 2)
                    / 40.0f;
            }

            return new(new Dice(diceX, diceY, diceBonus), multiplier, pierceRate);
            // <<<<<<<< shade2/calculation.hsp:264 		} ..
        }

        private AttackDamage CalcPhysicalAttackRawDamage(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical)
        {
            var strength = CalcPhysicalAttackStrength(attacker, target, attackSkill, weapon, attackCount, isRanged, isCritical);
            var protection = CalcPhysicalAttackProtection(attacker, target, attackSkill, weapon, attackCount, isRanged, isCritical);

            // >>>>>>>> elona122/shade2/calculation.hsp:297 	if dmgFix<-100:dmgFix=-100 ...
            strength.Dice.Bonus = Math.Max(strength.Dice.Bonus, -100);
            var damage = strength.Dice.Roll(_rand);

            if (isCritical)
            {
                damage = strength.Dice.MaxRoll();

                if (_protos.EventBus.HasEventHandlerFor<SkillPrototype, P_SkillCalcCriticalDamageEvent>(attackSkill))
                {
                    var ev = new P_SkillCalcCriticalDamageEvent(attacker, target, weapon, attackCount, isRanged, isCritical, strength, protection, damage);
                    _protos.EventBus.RaiseEvent(attackSkill, ref ev);
                    strength = ev.OutStrength;
                    protection = ev.OutProtection;
                    damage = ev.OutDamage;
                }
                else if (weapon != null)
                {
                    if (TryGetAmmoForWeapon(attacker, weapon.Value, out var ammo))
                    {
                        var weight = CompOrNull<WeightComponent>(ammo.Value)?.Weight.Buffed ?? 0;
                        strength.Multiplier *= Math.Clamp((float)weight / 100 + 100, 100, 150) / 100;
                    }
                    else
                    {
                        var weight = CompOrNull<WeightComponent>(weapon.Value)?.Weight.Buffed ?? 0;
                        strength.Multiplier *= Math.Clamp((float)weight / 200 + 100, 100, 150) / 100;
                    }
                }
            }

            damage = (int)(damage * strength.Multiplier);
            var originalDamage = damage;
            var vorpal = false;

            if (protection.Protection > 0)
                damage = damage * 100 / (100 + protection.Protection);

            if (isRanged)
            {
                if (IsAlive(weapon) && TryGetActiveAmmoEnchantment(attacker, weapon.Value, out var ammoComp, out var ammoEncComp))
                {
                    var pev = new P_AmmoEnchantmentCalcAttackDamageEvent(attacker, target, weapon.Value, ammoComp.Owner, ammoEncComp.Owner, attackSkill, attackCount, isCritical, strength, damage);
                    _protos.EventBus.RaiseEvent(ammoEncComp.AmmoEnchantmentID, ref pev);
                    damage = pev.OutDamage;
                    vorpal = pev.OutVorpal;
                }
            }
            else
            {
                if (TryComp<EquipStatsComponent>(attacker, out var equipStats) && _rand.Prob(equipStats.PierceRate.Buffed / 100.0f))
                {
                    strength.PierceRate = 100;
                    vorpal = true;
                    _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.Vorpal"), UiColors.MesYellow, entity: attacker);
                }
            }

            var pierceDamage = damage * strength.PierceRate / 100;
            var normalDamage = damage - pierceDamage;

            if (protection.Protection > 0)
                normalDamage = Math.Max(normalDamage - protection.Dice.Roll(_rand), 0);

            damage = pierceDamage + normalDamage;

            // BUGFIX: Opatos damage reduction
            if (_feats.HasFeat(attacker, Protos.Feat.GodEarth))
                damage = (int)(damage * 0.95f);

            var damageResistance = CompOrNull<EquipStatsComponent>(attacker)?.DamageResistance.Buffed ?? 0;
            if (damageResistance > 0)
                damage = (int)(damage * 100f / Math.Clamp(100f + damageResistance, 25f, 1000f));

            damage = Math.Max(damage, 0);
            // <<<<<<<< elona122/shade2/calculation.hsp:343 	return damage ...

            return new(damage, vorpal, normalDamage, pierceDamage, originalDamage);
        }

        public AttackStrength CalcPhysicalAttackStrength(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical)
        {
            var ev = new P_SkillCalcAttackStrengthEvent(attacker, target, weapon, attackCount, isRanged, isCritical);
            _protos.EventBus.RaiseEvent(attackSkill, ref ev);

            AttackStrength attackStrength;
            if (ev.OutStrength != null)
                attackStrength = ev.OutStrength;
            else
                attackStrength = CalcDefaultAttackStrength(attacker, target, attackSkill, weapon);

            var ev2 = new CalcPhysicalAttackStrengthEvent(target, attackSkill, weapon, attackCount, isRanged, isCritical, attackStrength);
            RaiseEvent(attacker, ref ev2);
            return ev2.OutStrength;
        }

        private int CalcArmorSkillLevel(EntityUid target)
        {
            var armorClass = _equip.GetArmorClass(target);
            return _skills.Level(target, armorClass);
        }

        private AttackProtection CalcPhysicalAttackBaseProtection(EntityUid target)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:280 	prot		= cPV(tc)  + sdata(cArmor(tc),tc) +sDEX(tc) ...
            var amount = CompOrNull<EquipStatsComponent>(target)?.PV?.Buffed ?? 0
                + CalcArmorSkillLevel(target)
                + _skills.Level(target, Protos.Skill.AttrDexterity)
                / 10;

            var diceX = 0;
            var diceY = 0;

            if (amount > 0)
            {
                var prot2 = amount / 4;
                diceX = Math.Max(prot2 / 10 + 1, 1);
                diceY = prot2 / diceX + 2;
            }

            return new(new Dice(diceX, diceY, 0), amount);
            // <<<<<<<< elona122/shade2/calculation.hsp:293 		} ...
        }

        private AttackProtection CalcPhysicalAttackProtection(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical)
        {
            var baseProtection = CalcPhysicalAttackBaseProtection(target);

            var ev2 = new CalcPhysicalAttackProtectionEvent(target, attackSkill, weapon, attackCount, isRanged, isCritical, baseProtection);
            RaiseEvent(attacker, ref ev2);
            return ev2.OutProtection;
        }
    }

    public static class RangedWeaponQueryErrors
    {
        public const string NoRangedWeapon = $"Elona.{nameof(NoRangedWeapon)}";
        public const string NoAmmo = $"Elona.{nameof(NoAmmo)}";
        public const string WrongAmmoType = $"Elona.{nameof(WrongAmmoType)}";
    }

    public sealed class BeforeMeleeAttackEvent : TurnResultEntityEventArgs
    {
        public EntityUid Target { get; }

        public BeforeMeleeAttackEvent(EntityUid target)
        {
            Target = target;
        }
    }

    public sealed record EquipState(int AttackCount, bool IsWieldingShield, bool IsWieldingTwoHanded, bool IsDualWielding);

    public sealed class AttackStrength
    {
        public AttackStrength(Dice dice, float multiplier, int pierceRate)
        {
            Dice = dice;
            Multiplier = multiplier;
            PierceRate = pierceRate;
        }

        public Dice Dice { get; set; } = new();
        public float Multiplier { get; set; }
        public int PierceRate { get; set; }
    }

    public sealed class AttackProtection
    {
        public AttackProtection(Dice dice, int protection)
        {
            Dice = dice;
            Protection = protection;
        }

        public Dice Dice { get; set; } = new();
        public int Protection { get; set; }
    }

    public sealed class AttackDamage
    {
        public AttackDamage(int totalDamage, bool vorpal, int normalDamage, int pierceDamage, int originalDamage)
        {
            TotalDamage = totalDamage;
            Vorpal = vorpal;
            NormalDamage = normalDamage;
            PierceDamage = pierceDamage;
            OriginalDamage = originalDamage;
        }

        /// <summary>
        /// Same as <see cref="DamageHPResult.BaseDamage"/>.
        /// </summary>
        public int TotalDamage { get; set; }
        public bool Vorpal { get; set; }
        public int NormalDamage { get; set; }
        public int PierceDamage { get; set; }
        public int OriginalDamage { get; set; }
    }

    public sealed class BeforePhysicalAttackEventArgs : HandledEntityEventArgs
    {
        public EntityUid Target { get; set; }
        public EntityUid? Weapon { get; set; }
        public int AttackCount { get; set; }
    }

    public sealed class AfterPhysicalAttackHitEventArgs : EntityEventArgs
    {
        public EntityUid Target { get; set; }
        public EntityUid? Weapon { get; set; }
        public int AttackCount { get; set; }
        public PrototypeId<SkillPrototype> AttackSkill { get; set; }
        public bool IsRanged { get; set; }
        public HitResult HitResult { get; set; }
        public AttackDamage RawDamage { get; set; } = default!;
        public int FinalDamage { get; set; }
    }

    public sealed class AfterPhysicalAttackMissEventArgs : EntityEventArgs
    {
        public EntityUid Target { get; set; }
        public EntityUid? Weapon { get; set; }
        public int AttackCount { get; set; }
        public PrototypeId<SkillPrototype> AttackSkill { get; set; }
        public bool IsRanged { get; set; }
        public HitResult HitResult { get; set; }
    }

    [ByRefEvent]
    public struct CalcPhysicalAttackAccuracyEvent
    {
        public EntityUid Target { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }
        public bool ConsiderDistance { get; }

        public int OutAccuracy { get; set; }

        public CalcPhysicalAttackAccuracyEvent(EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool considerDistance, int baseAccuracy)
        {
            Target = target;
            AttackSkill = attackSkill;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            ConsiderDistance = considerDistance;
            OutAccuracy = baseAccuracy;
        }
    }

    [ByRefEvent]
    public struct CalcPhysicalAttackEvasionEvent
    {
        public EntityUid Attacker { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }

        public int OutEvasion { get; set; }

        public CalcPhysicalAttackEvasionEvent(EntityUid attacker, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, int baseEvasion)
        {
            Attacker = attacker;
            AttackSkill = attackSkill;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            OutEvasion = baseEvasion;
        }
    }

    [ByRefEvent]
    public struct CalcPhysicalAttackHitEvent
    {
        public EntityUid Target { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }

        public int OutAccuracy { get; set; }
        public int OutEvasion { get; set; }
        public HitResult? OutHitResult { get; set; } = null;
        public bool Handled { get; set; } = false;

        public CalcPhysicalAttackHitEvent(EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, int toHit, int evasion)
        {
            Target = target;
            AttackSkill = attackSkill;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            OutAccuracy = toHit;
            OutEvasion = evasion;
        }

        public void Handle(HitResult hitResult)
        {
            OutHitResult = hitResult;
            Handled = true;
        }
    }

    [ByRefEvent]
    public struct CalcPhysicalAttackStrengthEvent
    {
        public EntityUid Target { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }
        public bool IsCritical { get; }

        public AttackStrength OutStrength { get; set; }

        public CalcPhysicalAttackStrengthEvent(EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical, AttackStrength attackStrength)
        {
            Target = target;
            AttackSkill = attackSkill;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            IsCritical = isCritical;
            OutStrength = attackStrength;
        }
    }

    [ByRefEvent]
    public struct CalcPhysicalAttackProtectionEvent
    {
        public EntityUid Target { get; }
        public PrototypeId<SkillPrototype> AttackSkill { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }
        public bool IsCritical { get; }

        public AttackProtection OutProtection { get; set; }

        public CalcPhysicalAttackProtectionEvent(EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical, AttackProtection attackProtection)
        {
            Target = target;
            AttackSkill = attackSkill;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            IsCritical = isCritical;
            OutProtection = attackProtection;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(SkillPrototype))]
    public struct P_SkillCalcAttackStrengthEvent
    {
        public EntityUid Attacker { get; }
        public EntityUid Target { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }
        public bool IsCritical { get; }

        public AttackStrength? OutStrength { get; set; } = null;

        public P_SkillCalcAttackStrengthEvent(EntityUid attacker, EntityUid target, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical)
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            IsCritical = isCritical;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(SkillPrototype))]
    public struct P_SkillCalcCriticalDamageEvent
    {
        public EntityUid Attacker { get; }
        public EntityUid Target { get; }
        public EntityUid? Weapon { get; }
        public int AttackCount { get; }
        public bool IsRanged { get; }
        public bool IsCritical { get; }

        public AttackStrength OutStrength { get; set; }
        public AttackProtection OutProtection { get; set; }
        public int OutDamage { get; set; }

        public P_SkillCalcCriticalDamageEvent(EntityUid attacker, EntityUid target, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical, AttackStrength strength, AttackProtection protection, int damage)
        {
            Attacker = attacker;
            Target = target;
            Weapon = weapon;
            AttackCount = attackCount;
            IsRanged = isRanged;
            IsCritical = isCritical;
            OutStrength = strength;
            OutProtection = protection;
            OutDamage = damage;
        }
    }

    public enum HitResult
    {
        Miss,
        Evade,
        Hit,
        CriticalHit
    }

    public sealed class ExtAttackAnim : IPrototypeExtendedData<SkillPrototype>
    {
        [DataField]
        public PrototypeId<AssetPrototype> AttackAnim { get; set; }
    }
}
