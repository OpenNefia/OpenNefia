using JetBrains.Annotations;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
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
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace OpenNefia.Content.Combat
{
    public interface ICombatSystem
    {
        EquipState GetEquipState(EntityUid ent);
        TurnResult? MeleeAttack(EntityUid attacker, EntityUid target);
    }

    public sealed partial class CombatSystem : EntitySystem, ICombatSystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IInputSystem _input = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEquipmentSystem _equip = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

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
                    // TODO generalize
                    if (equip.EquipSlots.Contains(Protos.EquipSlot.Hand))
                        attackCount++;

                    if (_tags.HasTag(equip.Owner, Protos.Tag.ItemCatEquipShield))
                        isWieldingShield = true;
                }
            }

            if (isWieldingShield)
            {
                if (TryComp<SkillsComponent>(ent, out var skills))
                {
                    if (skills.PV.Buffed > 0)
                    {
                        skills.PV.Buffed *= (int)((120 + Math.Sqrt(skills.Level(Protos.Skill.Shield)) * 2) / 100);
                    }
                }
                else
                {
                    if (attackCount == 1)
                        isWieldingTwoHanded = true;
                    else if (attackCount > 0)
                        isDualWielding = true;
                }
            }

            return new(attackCount, isWieldingShield, isWieldingTwoHanded, isDualWielding);
        }

        private readonly PrototypeId<SoundPrototype>[] KillSounds = new[]
        {
            Protos.Sound.Kill1,
            Protos.Sound.Kill2,
        };

        private void KillEntity(EntityUid target, MetaDataComponent? metaData = null, SpatialComponent? spatial = null)
        {
            if (!Resolve(target, ref metaData))
                return;

            _sounds.Play(_rand.Pick(KillSounds), target);

            // TODO
            if (Resolve(target, ref spatial))
                _mapDebris.SpillBlood(spatial.MapPosition, 5);

            // TODO
            if (TryComp<CharaComponent>(target, out var chara))
            {
                chara.Liveness = CharaLivenessState.Dead;
            }
            else
            {
                metaData.Liveness = EntityGameLiveness.DeadAndBuried;
            }
        }

        public TurnResult? MeleeAttack(EntityUid attacker, EntityUid target)
        {
            var ev = new GetMeleeWeaponsEvent();
            RaiseEvent(attacker, ev);

            if (ev.Weapons.Count > 0)
            {
                foreach (var (weapon, attackCount) in ev.Weapons.WithIndex())
                {

                    var attackSkill = GetPhysicalAttackSkill(weapon);
                    PhysicalAttack(attacker, target, attackSkill, weapon, attackCount);
                }
            }
            else
            {
                PhysicalAttack(attacker, target, Protos.Skill.MartialArts);
            }

            return TurnResult.Succeeded;
        }

        public PrototypeId<SkillPrototype> GetPhysicalAttackSkill(EntityUid? weapon, WeaponComponent? weaponComp = null)
        {
            if (weapon != null && Resolve(weapon.Value, ref weaponComp))
            {
                return weaponComp.WeaponSkill;
            }
            return Protos.Skill.MartialArts;
        }

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
        public void PhysicalAttack(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon = null,
            int attackCount = 0, bool isRanged = false)
        {
            if (!EntityManager.IsAlive(attacker) || !EntityManager.IsAlive(target))
                return;

            var ev = new BeforePhysicalAttackEventArgs()
            {
                Attacker = attacker,
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
                    if (TryGetAttackAnimation(target, attackSkill, damagePercent, isCritical, out var anim))
                    {
                        _mapDrawables.Enqueue(anim, Spatial(target).MapPosition);
                    }
                }

                if (weapon != null)
                {
                    
                }
                else
                {
                    
                }
            }
            else
            {
                if (_gameSession.IsPlayer(attacker))
                {
                    _audio.Play(Protos.Sound.Miss, target);
                }
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

        private void ShowMissText(EntityUid attacker, EntityUid target, int attackCount)
        {
            // >>>>>>>> shade2/action.hsp:1365 	if hit=atkEvade:if sync(cc){ ...
            if (!_vis.IsInWindowFov(attacker))
                return;

            var capitalize = true;
            
            if (attackCount > 0)
            {
                _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.Furthermore"));
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
                _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.Furthermore"));
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

        private bool TryGetAttackAnimation(EntityUid target, PrototypeId<SkillPrototype> attackSkill, int damagePercent, bool isCritical, [NotNullWhen(true)] out IMapDrawable? anim)
        {
            if (!_protos.TryGetExtendedData<SkillPrototype, ExtAttackAnim>(attackSkill, out var ext))
            {
                anim = null;
                return false;
            }

            var breaksIntoDebris = HasComp<BreaksIntoDebrisComponent>(target);
            PrototypeId<AssetPrototype> particleAsset;
            if (breaksIntoDebris)
                particleAsset = Protos.Asset.MeleeAttackDebris;
            else
                particleAsset = Protos.Asset.MeleeAttackBlood;

            anim = new MeleeAttackMapDrawable(particleAsset, ext.AttackAnim, damagePercent, isCritical);
            return true;
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

            if (toHit < 1)
                return HitResult.Miss;
            else if (evasion < 1)
                return HitResult.Hit;
            else if (_rand.Next(toHit) > _rand.Next(evasion * 3 / 2))
                return HitResult.Hit;

            return HitResult.Miss;
        }

        public bool TryGetAmmo(EntityUid entity, EntityUid? weapon, [NotNullWhen(true)] out EquipmentComponent? ammo)
        {
            // TODO
            ammo = null;
            return false;
        }

        private int CalcBasePhysicalAttackAccuracy(EntityUid attacker, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon)
        {
            var equipState = GetEquipState(attacker);

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

                if (TryGetAmmo(attacker, weapon, out var ammo) && TryComp<EquipStatsComponent>(ammo.Owner, out var ammoStats))
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
            return _skills.Level(target, Protos.Skill.AttrPerception) / 3
                + _skills.Level(target, Protos.Skill.Evasion)
                + CompOrNull<EquipStatsComponent>(target)?.DV.Buffed ?? 0
                + 25;
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

            var diceX = weaponComp.DiceX;
            var diceY = weaponComp.DiceY;
            var pierceRate = weaponComp.PierceRate;
            var weaponSkill = weaponComp.WeaponSkill;
            float multiplier;

            if (TryGetAmmo(attacker, weapon.Value, out var ammo))
            {
                var ammoComp = Comp<AmmoComponent>(ammo.Owner);
                diceBonus += CompOrNull<EquipStatsComponent>(ammo.Owner)?.DamageBonus.Buffed ?? 0
                    + ammoComp.DiceX * ammoComp.DiceY / 2;
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
                else if (TryGetAmmo(attacker, weapon, out var ammo))
                {
                    var weight = CompOrNull<WeightComponent>(ammo.Owner)?.Weight ?? 0;
                    strength.Multiplier *= Math.Clamp((float)weight / 100 + 100, 100, 150) / 100;
                }
                else if (weapon != null)
                {
                    var weight = CompOrNull<WeightComponent>(weapon.Value)?.Weight ?? 0;
                    strength.Multiplier *= Math.Clamp((float)weight / 200 + 100, 100, 150) / 100;
                }
            }

            damage = (int)(damage * strength.Multiplier);
            var originalDamage = damage;
            var vorpal = false;

            if (protection.Protection > 0)
                damage = damage * 100 / (100 + protection.Protection);

            if (isRanged)
            {
                // TODO ammo enchantments
            }
            else
            {
                if (TryComp<EquipStatsComponent>(attacker, out var equipStats) && _rand.Prob(equipStats.PierceRate.Buffed / 100.0f))
                {
                    strength.PierceRate = 100;
                    vorpal = true;
                    _mes.Display(Loc.GetString("Elona.Combat.PhysicalAttack.Vorpal.Melee"), UiColors.MesYellow, entity: attacker);
                }
            }

            var pierceDamage = damage * strength.PierceRate / 100;
            var normalDamage = damage - pierceDamage;

            if (protection.Protection > 0)
                normalDamage = Math.Max(protection.Dice.Roll(_rand), 0);

            damage = pierceDamage + normalDamage;

            // BUGFIX: Opatos damage reduction
            if (_feats.HasFeat(attacker, Protos.Feat.GodEarth))
                damage = (int)(damage * 0.95f);

            var damageResistance = CompOrNull<EquipStatsComponent>(attacker)?.DamageResistance.Buffed ?? 0;
            if (damageResistance > 0)
                damage = (int)(damage * 100f / Math.Clamp(100f + damageResistance, 25f, 1000f));

            damage = Math.Max(damage, 0);

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
        }

        private AttackProtection CalcPhysicalAttackProtection(EntityUid attacker, EntityUid target, PrototypeId<SkillPrototype> attackSkill, EntityUid? weapon, int attackCount, bool isRanged, bool isCritical)
        {
            var baseProtection = CalcPhysicalAttackBaseProtection(target);

            var ev2 = new CalcPhysicalAttackProtectionEvent(target, attackSkill, weapon, attackCount, isRanged, isCritical, baseProtection);
            RaiseEvent(attacker, ref ev2);
            return ev2.OutProtection;
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

        public int TotalDamage { get; set; }
        public bool Vorpal { get; set; }
        public int NormalDamage { get; set; }
        public int PierceDamage { get; set; }
        public int OriginalDamage { get; set; }
    }

    public sealed class BeforePhysicalAttackEventArgs : HandledEntityEventArgs
    {
        public EntityUid Attacker;
        public EntityUid Target;
        public EntityUid? Weapon;
        public int AttackCount;
    }

    public sealed class GetMeleeWeaponsEvent : EntityEventArgs
    {
        public readonly List<EntityUid> Weapons = new();
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

    public sealed class ExtAttackAnim : IPrototypeExtendedData
    {
        [DataField]
        public PrototypeId<AssetPrototype> AttackAnim { get; set; }
    }
}