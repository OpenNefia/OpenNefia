using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Food;
using OpenNefia.Core.Game;
using OpenNefia.Content.Resists;
using OpenNefia.Content.UI;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Spells;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Logic;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Levels;
using System.Runtime.InteropServices;
using OpenNefia.Content.Visibility;

namespace OpenNefia.Content.Enchantments
{
    public sealed partial class VanillaEnchantmentsSystem
    {
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;

        private void Initialize_Unique()
        {
            SubscribeComponent<EncResistBlindnessComponent, ApplyEnchantmentOnRefreshEvent>(EncResistBlindness_ApplyOnRefresh);
            SubscribeComponent<EncResistParalysisComponent, ApplyEnchantmentOnRefreshEvent>(EncResistParalysis_ApplyOnRefresh);
            SubscribeComponent<EncResistConfusionComponent, ApplyEnchantmentOnRefreshEvent>(EncResistConfusion_ApplyOnRefresh);
            SubscribeComponent<EncResistFearComponent, ApplyEnchantmentOnRefreshEvent>(EncResistFear_ApplyOnRefresh);
            SubscribeComponent<EncResistSleepComponent, ApplyEnchantmentOnRefreshEvent>(EncResistSleep_ApplyOnRefresh);
            SubscribeComponent<EncResistPoisonComponent, ApplyEnchantmentOnRefreshEvent>(EncResistPoison_ApplyOnRefresh);
            SubscribeComponent<EncResistTheftComponent, ApplyEnchantmentOnRefreshEvent>(EncResistTheft_ApplyOnRefresh);
            SubscribeComponent<EncResistRottenFoodComponent, ApplyEnchantmentOnRefreshEvent>(EncResistRottenFood_ApplyOnRefresh);
            SubscribeComponent<EncResistEtherwindComponent, ApplyEnchantmentOnRefreshEvent>(EncResistEtherwind_ApplyOnRefresh);
            SubscribeComponent<EncResistBadWeatherComponent, ApplyEnchantmentOnRefreshEvent>(EncResistBadWeather_ApplyOnRefresh);
            SubscribeComponent<EncResistPregnancyComponent, ApplyEnchantmentOnRefreshEvent>(EncResistPregnancy_ApplyOnRefresh);
            SubscribeComponent<EncFloatComponent, ApplyEnchantmentOnRefreshEvent>(EncFloat_ApplyOnRefresh);
            SubscribeComponent<EncResistMutationComponent, ApplyEnchantmentOnRefreshEvent>(EncResistMutation_ApplyOnRefresh);
            SubscribeComponent<EncEnhanceSpellsComponent, ApplyEnchantmentOnRefreshEvent>(EncEnhanceSpells_ApplyOnRefresh);
            SubscribeComponent<EncSeeInvisibleComponent, ApplyEnchantmentOnRefreshEvent>(EncSeeInvisible_ApplyOnRefresh);
            SubscribeComponent<EncDetectReligionComponent, ApplyEnchantmentOnRefreshEvent>(EncDetectReligion_ApplyOnRefresh);
            SubscribeComponent<EncGouldComponent, ApplyEnchantmentOnRefreshEvent>(EncGould_ApplyOnRefresh);

            SubscribeComponent<EncRandomTeleportComponent, CalcEnchantmentAdjustedPowerEvent>(EncRandomTeleport_CalcAdjustedPower);
            SubscribeComponent<EncRandomTeleportComponent, ApplyEnchantmentAfterPassTurnEvent>(EncRandomTeleport_ApplyAfterPassTurn);

            SubscribeComponent<EncSuckBloodComponent, CalcEnchantmentAdjustedPowerEvent>(EncSuckBlood_CalcAdjustedPower);
            SubscribeComponent<EncSuckBloodComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSuckBlood_ApplyAfterPassTurn);

            SubscribeComponent<EncSuckExperienceComponent, CalcEnchantmentAdjustedPowerEvent>(EncSuckExperience_CalcAdjustedPower);
            SubscribeComponent<EncSuckExperienceComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSuckExperience_ApplyAfterPassTurn);

            SubscribeComponent<EncSummonCreatureComponent, CalcEnchantmentAdjustedPowerEvent>(EncSummonCreature_CalcAdjustedPower);
            SubscribeComponent<EncSummonCreatureComponent, ApplyEnchantmentAfterPassTurnEvent>(EncSummonCreature_ApplyAfterPassTurn);

            SubscribeComponent<EncFastTravelComponent, CalcEnchantmentAdjustedPowerEvent>(EncFastTravel_CalcAdjustedPower);
            SubscribeComponent<EncFastTravelComponent, ApplyEnchantmentOnRefreshEvent>(EncFastTravel_ApplyOnRefresh);

            SubscribeComponent<EncAbsorbStaminaComponent, CalcEnchantmentAdjustedPowerEvent>(EncAbsorbStamina_CalcAdjustedPower);
            SubscribeComponent<EncAbsorbStaminaComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncAbsorbStamina_ApplyAfterPhysicalAttack);
            SubscribeComponent<EncAbsorbStaminaComponent, ApplyEnchantmentFoodEffectsEvent>(EncAbsorbStamina_ApplyFoodEffects);

            SubscribeComponent<EncRagnarokComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncRagnarok_ApplyAfterPhysicalAttack);
            SubscribeComponent<EncRagnarokComponent, ApplyEnchantmentFoodEffectsEvent>(EncRagnarok_ApplyFoodEffects);

            SubscribeComponent<EncAbsorbManaComponent, CalcEnchantmentAdjustedPowerEvent>(EncAbsorbMana_CalcAdjustedPower);
            SubscribeComponent<EncAbsorbManaComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncAbsorbMana_ApplyAfterPhysicalAttack);
            SubscribeComponent<EncAbsorbManaComponent, ApplyEnchantmentFoodEffectsEvent>(EncAbsorbMana_ApplyFoodEffects);

            SubscribeComponent<EncPierceChanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncPierceChance_CalcAdjustedPower);
            SubscribeComponent<EncPierceChanceComponent, ApplyEnchantmentOnRefreshEvent>(EncPierceChance_ApplyOnRefresh);

            SubscribeComponent<EncCriticalChanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncCriticalChance_CalcAdjustedPower);
            SubscribeComponent<EncCriticalChanceComponent, ApplyEnchantmentOnRefreshEvent>(EncCriticalChance_ApplyOnRefresh);

            SubscribeComponent<EncExtraMeleeAttackChanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncExtraMeleeAttackChance_CalcAdjustedPower);
            SubscribeComponent<EncExtraMeleeAttackChanceComponent, ApplyEnchantmentOnRefreshEvent>(EncExtraMeleeAttackChance_ApplyOnRefresh);

            SubscribeComponent<EncExtraRangedAttackChanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncExtraRangedAttackChance_CalcAdjustedPower);
            SubscribeComponent<EncExtraRangedAttackChanceComponent, ApplyEnchantmentOnRefreshEvent>(EncExtraRangedAttackChance_ApplyOnRefresh);

            SubscribeComponent<EncFastTravelComponent, CalcEnchantmentAdjustedPowerEvent>(EncFastTravel_CalcAdjustedPower);
            SubscribeComponent<EncFastTravelComponent, ApplyEnchantmentOnRefreshEvent>(EncFastTravel_ApplyOnRefresh);

            SubscribeComponent<EncTimeStopComponent, CalcEnchantmentAdjustedPowerEvent>(EncTimeStop_CalcAdjustedPower);
            SubscribeComponent<EncTimeStopComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncTimeStop_ApplyAfterPhysicalAttack);
            SubscribeComponent<EncTimeStopComponent, ApplyEnchantmentFoodEffectsEvent>(EncTimeStop_ApplyFoodEffects);

            SubscribeComponent<EncResistCurseComponent, CalcEnchantmentAdjustedPowerEvent>(EncResistCurse_CalcAdjustedPower);
            SubscribeComponent<EncResistCurseComponent, ApplyEnchantmentOnRefreshEvent>(EncResistCurse_ApplyOnRefresh);

            SubscribeComponent<EncStradivariusComponent, CalcEnchantmentAdjustedPowerEvent>(EncStradivarius_CalcAdjustedPower);
            SubscribeComponent<EncStradivariusComponent, ApplyEnchantmentOnRefreshEvent>(EncStradivarius_ApplyOnRefresh);

            SubscribeComponent<EncDamageResistanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncDamageResistance_CalcAdjustedPower);
            SubscribeComponent<EncDamageResistanceComponent, ApplyEnchantmentOnRefreshEvent>(EncDamageResistance_ApplyOnRefresh);

            SubscribeComponent<EncDamageImmunityComponent, CalcEnchantmentAdjustedPowerEvent>(EncDamageImmunity_CalcAdjustedPower);
            SubscribeComponent<EncDamageImmunityComponent, ApplyEnchantmentOnRefreshEvent>(EncDamageImmunity_ApplyOnRefresh);

            SubscribeComponent<EncDamageReflectionComponent, CalcEnchantmentAdjustedPowerEvent>(EncDamageReflection_CalcAdjustedPower);
            SubscribeComponent<EncDamageReflectionComponent, ApplyEnchantmentOnRefreshEvent>(EncDamageReflection_ApplyOnRefresh);

            SubscribeComponent<EncCuresBleedingQuicklyComponent, ApplyEnchantmentOnRefreshEvent>(EncCuresBleedingQuickly_ApplyOnRefresh);
            SubscribeComponent<EncCatchesGodSignalsComponent, ApplyEnchantmentOnRefreshEvent>(EncCatchesGodSignals_ApplyOnRefresh);

            SubscribeComponent<EncDragonBaneComponent, CalcEnchantmentAdjustedPowerEvent>(EncDragonBane_CalcAdjustedPower);
            SubscribeComponent<EncDragonBaneComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncDragonBane_ApplyAfterPhysicalAttack);

            SubscribeComponent<EncUndeadBaneComponent, CalcEnchantmentAdjustedPowerEvent>(EncUndeadBane_CalcAdjustedPower);
            SubscribeComponent<EncUndeadBaneComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncUndeadBane_ApplyAfterPhysicalAttack);

            SubscribeComponent<EncGodBaneComponent, CalcEnchantmentAdjustedPowerEvent>(EncGodBane_CalcAdjustedPower);
            SubscribeComponent<EncGodBaneComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncGodBane_ApplyAfterPhysicalAttack);
        }

        private void EncRandomTeleport_CalcAdjustedPower(EntityUid uid, EncRandomTeleportComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncRandomTeleport_ApplyAfterPassTurn(EntityUid uid, EncRandomTeleportComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (!TryMap(args.Equipper, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                return;

            if (_rand.Next(25) < Math.Clamp(Math.Abs(args.AdjustedPower) / 50, 1, 25))
            {
                _spells.Cast(Protos.Spell.SpellTeleport, args.AdjustedPower, target: args.Equipper, source: args.Equipper);
            }
        }


        private void EncSuckBlood_CalcAdjustedPower(EntityUid uid, EncSuckBloodComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSuckBlood_ApplyAfterPassTurn(EntityUid uid, EncSuckBloodComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (_rand.OneIn(4))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SuckBlood.BloodSucked", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                var bleedPower = Math.Abs(args.TotalPower) / 25 + 3;
                _statusEffects.Apply(args.Equipper, Protos.StatusEffect.Bleeding, bleedPower);
            }
        }


        private void EncSuckExperience_CalcAdjustedPower(EntityUid uid, EncSuckExperienceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSuckExperience_ApplyAfterPassTurn(EntityUid uid, EncSuckExperienceComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (_rand.OneIn(4) && TryComp<LevelComponent>(args.Equipper, out var level))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SuckExperience.ExperienceReduced", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                var lostExp = level.ExperienceToNext / (100 - Math.Clamp(Math.Abs(args.TotalPower) / 2, 0, 50)) + _rand.Next(100);
                level.Experience = Math.Max(level.Experience - lostExp, 0);
            }
        }


        private void EncSummonCreature_CalcAdjustedPower(EntityUid uid, EncSummonCreatureComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncSummonCreature_ApplyAfterPassTurn(EntityUid uid, EncSummonCreatureComponent component, ref ApplyEnchantmentAfterPassTurnEvent args)
        {
            if (!TryMap(args.Equipper, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) || HasComp<MapTypePlayerOwnedComponent>(map.MapEntityUid))
            {
                return;
            }

            if (_rand.Next(50) < Math.Clamp(Math.Abs(args.AdjustedPower), 1, 50))
            {
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.SummonCreature.CreatureSummoned", ("entity", args.Equipper)), color: UiColors.MesPurple, entity: args.Equipper);
                for (var i = 0; i < 3; i++)
                {
                    var filter = new CharaFilter()
                    {
                        MinLevel = _randomGen.CalcObjectLevel(_levels.GetLevel(_gameSession.Player) * 3 / 2 + 3),
                        Quality = _randomGen.CalcObjectQuality(Qualities.Quality.Normal)
                    };
                    _charaGen.GenerateChara(args.Equipper, filter);
                }
            }
        }
        
        private void EncResistBlindness_ApplyOnRefresh(EntityUid uid, EncResistBlindnessComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Blindness);
        }
        
        private void EncResistParalysis_ApplyOnRefresh(EntityUid uid, EncResistParalysisComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Paralysis);
        }

        private void EncResistConfusion_ApplyOnRefresh(EntityUid uid, EncResistConfusionComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Confusion);
        }

        private void EncResistFear_ApplyOnRefresh(EntityUid uid, EncResistFearComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Fear);
        }

        private void EncResistSleep_ApplyOnRefresh(EntityUid uid, EncResistSleepComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Sleep);
        }

        private void EncResistPoison_ApplyOnRefresh(EntityUid uid, EncResistPoisonComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            _statusEffects.AddTemporaryEffectImmunity(args.Equipper, Protos.StatusEffect.Poison);
        }


        private void EncResistTheft_ApplyOnRefresh(EntityUid uid, EncResistTheftComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromTheft.Buffed = true;
        }


        private void EncResistRottenFood_ApplyOnRefresh(EntityUid uid, EncResistRottenFoodComponent Componentcomponent, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromRottenFood.Buffed = true;
        }


        private void EncFastTravel_CalcAdjustedPower(EntityUid uid, EncFastTravelComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 100;
        }

        private void EncFastTravel_ApplyOnRefresh(EntityUid uid, EncFastTravelComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            if (_skills.TryGetKnown(args.Equipper, Protos.Skill.AttrSpeed, out var speed))
                speed.Level.Buffed += args.AdjustedPower * 2 + 1;

            EnsureComp<FastTravelComponent>(args.Equipper).TravelSpeedModifier.Buffed += args.TotalPower / 8 / 100f;
        }


        private void EncResistEtherwind_ApplyOnRefresh(EntityUid uid, EncResistEtherwindComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromEtherwind.Buffed = true;
        }


        private void EncResistBadWeather_ApplyOnRefresh(EntityUid uid, EncResistBadWeatherComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromBadWeather.Buffed = true;
        }


        private void EncResistPregnancy_ApplyOnRefresh(EntityUid uid, EncResistPregnancyComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromPregnancy.Buffed = true;
        }


        private void EncFloat_ApplyOnRefresh(EntityUid uid, EncFloatComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsFloating.Buffed = true;
        }


        private void EncResistMutation_ApplyOnRefresh(EntityUid uid, EncResistMutationComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromMutation.Buffed = true;
        }


        private void EncEnhanceSpells_ApplyOnRefresh(EntityUid uid, EncEnhanceSpellsComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).HasEnhancedSpells.Buffed = true;
        }


        private void EncSeeInvisible_ApplyOnRefresh(EntityUid uid, EncSeeInvisibleComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<VisibilityComponent>(args.Equipper).CanSeeInvisible.Buffed = true;
        }


        private void EncAbsorbStamina_CalcAdjustedPower(EntityUid uid, EncAbsorbStaminaComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncAbsorbStamina_ApplyAfterPhysicalAttack(EntityUid uid, EncAbsorbStaminaComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            var staminaAmount = _rand.Next(args.AdjustedPower + 1) + 1;
            _damage.HealStamina(args.Attacker, staminaAmount);

            if (IsAlive(args.Target))
                _damage.DamageStamina(args.Target, staminaAmount / 2);
        }

        private void EncAbsorbStamina_ApplyFoodEffects(EntityUid uid, EncAbsorbStaminaComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            var staminaAmount = _rand.Next(args.AdjustedPower + 1) + 1;
            _damage.HealStamina(args.Eater, staminaAmount);
        }


        private void EncRagnarok_ApplyAfterPhysicalAttack(EntityUid uid, EncRagnarokComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            _mes.Display("TODO ragnarok", color: UiColors.MesYellow);
        }

        private void EncRagnarok_ApplyFoodEffects(EntityUid uid, EncRagnarokComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            _mes.Display("TODO ragnarok", color: UiColors.MesYellow);
        }


        private void EncAbsorbMana_CalcAdjustedPower(EntityUid uid, EncAbsorbManaComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncAbsorbMana_ApplyAfterPhysicalAttack(EntityUid uid, EncAbsorbManaComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            var mpAmount = _rand.Next(args.AdjustedPower * 2 + 1) + 1;
            _damage.HealMP(args.Attacker, mpAmount / 5);

            if (IsAlive(args.Target))
                _damage.DamageMP(args.Target, mpAmount);
        }

        private void EncAbsorbMana_ApplyFoodEffects(EntityUid uid, EncAbsorbManaComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            var mpAmount = _rand.Next(args.AdjustedPower * 2 + 1) + 1;
            _damage.HealStamina(args.Eater, mpAmount / 5);
        }


        private void EncPierceChance_CalcAdjustedPower(EntityUid uid, EncPierceChanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncPierceChance_ApplyOnRefresh(EntityUid uid, EncPierceChanceComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).PierceRate.Buffed += args.AdjustedPower;
        }


        private void EncCriticalChance_CalcAdjustedPower(EntityUid uid, EncCriticalChanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncCriticalChance_ApplyOnRefresh(EntityUid uid, EncCriticalChanceComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).CriticalRate.Buffed += args.AdjustedPower;
        }


        private void EncExtraMeleeAttackChance_CalcAdjustedPower(EntityUid uid, EncExtraMeleeAttackChanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncExtraMeleeAttackChance_ApplyOnRefresh(EntityUid uid, EncExtraMeleeAttackChanceComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).ExtraMeleeAttackRate.Buffed += args.AdjustedPower / 100f;
        }


        private void EncExtraRangedAttackChance_CalcAdjustedPower(EntityUid uid, EncExtraRangedAttackChanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncExtraRangedAttackChance_ApplyOnRefresh(EntityUid uid, EncExtraRangedAttackChanceComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).ExtraRangedAttackRate.Buffed += args.AdjustedPower / 100f;
        }


        private void EncTimeStop_CalcAdjustedPower(EntityUid uid, EncTimeStopComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncTimeStop_ApplyAfterPhysicalAttack(EntityUid uid, EncTimeStopComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (_rand.OneIn(25))
            {
                _mes.Display("TODO time stop", color: UiColors.MesYellow);
            }
        }

        private void EncTimeStop_ApplyFoodEffects(EntityUid uid, EncTimeStopComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            _mes.Display("TODO time stop", color: UiColors.MesYellow);
        }


        private void EncResistCurse_CalcAdjustedPower(EntityUid uid, EncResistCurseComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncResistCurse_ApplyOnRefresh(EntityUid uid, EncResistCurseComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Equipper).IsProtectedFromCurse.Buffed = true;
        }


        private void EncStradivarius_CalcAdjustedPower(EntityUid uid, EncStradivariusComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncStradivarius_ApplyOnRefresh(EntityUid uid, EncStradivariusComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<SpecialInstrumentsComponent>(args.Item).IsStradivarius.Buffed = true;
        }


        private void EncDamageResistance_CalcAdjustedPower(EntityUid uid, EncDamageResistanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncDamageResistance_ApplyOnRefresh(EntityUid uid, EncDamageResistanceComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).DamageResistance.Buffed += args.TotalPower / 50 + 5;
        }


        private void EncDamageImmunity_CalcAdjustedPower(EntityUid uid, EncDamageImmunityComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncDamageImmunity_ApplyOnRefresh(EntityUid uid, EncDamageImmunityComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).DamageImmunityRate.Buffed += args.TotalPower / 60 + 3;
        }


        private void EncDamageReflection_CalcAdjustedPower(EntityUid uid, EncDamageReflectionComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncDamageReflection_ApplyOnRefresh(EntityUid uid, EncDamageReflectionComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<EquipStatsComponent>(args.Equipper).DamageReflection.Buffed += args.TotalPower / 5 / 100f;
        }


        private void EncCuresBleedingQuickly_ApplyOnRefresh(EntityUid uid, EncCuresBleedingQuicklyComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CuresBleedingQuicklyComponent>(args.Item).CuresBleedingQuickly.Buffed = true;
        }


        private void EncCatchesGodSignals_ApplyOnRefresh(EntityUid uid, EncCatchesGodSignalsComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Item).CanCatchGodSignals.Buffed = true;
        }


        private void EncDragonBane_CalcAdjustedPower(EntityUid uid, EncDragonBaneComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncDragonBane_ApplyAfterPhysicalAttack(EntityUid uid, EncDragonBaneComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (!IsAlive(args.Target))
                return;

            if (_tags.HasTag(args.Target, Protos.Tag.CharaDragon))
            {
                var damage = args.PhysicalAttackArgs.RawDamage.OriginalDamage;
                _damage.DamageHP(args.Target, damage / 2, args.Attacker, extraArgs: new DamageHPExtraArgs()
                {
                    DamageSubLevel = 1
                });
            }
        }


        private void EncUndeadBane_CalcAdjustedPower(EntityUid uid, EncUndeadBaneComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncUndeadBane_ApplyAfterPhysicalAttack(EntityUid uid, EncUndeadBaneComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (!IsAlive(args.Target))
                return;

            if (_tags.HasTag(args.Target, Protos.Tag.CharaUndead))
            {
                var damage = args.PhysicalAttackArgs.RawDamage.OriginalDamage;
                _damage.DamageHP(args.Target, damage / 2, args.Attacker, extraArgs: new DamageHPExtraArgs()
                {
                    DamageSubLevel = 1
                });
            }
        }


        private void EncDetectReligion_ApplyOnRefresh(EntityUid uid, EncDetectReligionComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<CommonProtectionsComponent>(args.Item).CanDetectReligion.Buffed = true;
        }


        private void EncGould_ApplyOnRefresh(EntityUid uid, EncGouldComponent component, ref ApplyEnchantmentOnRefreshEvent args)
        {
            EnsureComp<SpecialInstrumentsComponent>(args.Item).IsGouldsPiano.Buffed = true;
        }


        private void EncGodBane_CalcAdjustedPower(EntityUid uid, EncGodBaneComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncGodBane_ApplyAfterPhysicalAttack(EntityUid uid, EncGodBaneComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (!IsAlive(args.Target))
                return;

            if (_tags.HasTag(args.Target, Protos.Tag.CharaGod))
            {
                var damage = args.PhysicalAttackArgs.RawDamage.OriginalDamage;
                _damage.DamageHP(args.Target, damage / 2, args.Attacker, extraArgs: new DamageHPExtraArgs()
                {
                    DamageSubLevel = 1
                });
            }
        }
    }
}
