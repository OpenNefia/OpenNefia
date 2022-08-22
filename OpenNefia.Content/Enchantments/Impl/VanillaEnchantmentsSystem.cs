using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
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

namespace OpenNefia.Content.Enchantments
{
    public sealed class VanillaEnchantmentsSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IResistsSystem _resists = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;

        public override void Initialize()
        {
            SubscribeComponent<EncModifyAttributeComponent, EntityBeingGeneratedEvent>(EncModifyAttribute_BeingGenerated);
            SubscribeComponent<EncModifyAttributeComponent, CalcEnchantmentAdjustedPowerEvent>(EncModifyAttribute_GetAdjustedPower);
            SubscribeComponent<EncModifyAttributeComponent, GetEnchantmentDescriptionEventArgs>(EncModifyAttribute_Localize);
            SubscribeComponent<EncModifyAttributeComponent, ApplyEnchantmentEffectsEvent>(EncModifyAttribute_Apply);
            SubscribeComponent<EncModifyAttributeComponent, ApplyEnchantmentFoodEffectsEvent>(EncModifyAttribute_ApplyAfterEaten);

            SubscribeComponent<EncModifyResistanceComponent, EntityBeingGeneratedEvent>(EncModifyResistance_BeingGenerated);
            SubscribeComponent<EncModifyResistanceComponent, CalcEnchantmentAdjustedPowerEvent>(EncModifyResistance_GetAdjustedPower);
            SubscribeComponent<EncModifyResistanceComponent, GetEnchantmentDescriptionEventArgs>(EncModifyResistance_Localize);
            SubscribeComponent<EncModifyResistanceComponent, ApplyEnchantmentEffectsEvent>(EncModifyResistance_Apply);
            SubscribeComponent<EncModifyResistanceComponent, ApplyEnchantmentEffectsEvent>(EncModifyResistance_Apply);

            SubscribeComponent<EncModifySkillComponent, EntityBeingGeneratedEvent>(EncModifySkill_BeingGenerated);
            SubscribeComponent<EncModifySkillComponent, CalcEnchantmentAdjustedPowerEvent>(EncModifySkill_GetAdjustedPower);
            SubscribeComponent<EncModifySkillComponent, GetEnchantmentDescriptionEventArgs>(EncModifySkill_Localize);
            SubscribeComponent<EncModifySkillComponent, ApplyEnchantmentEffectsEvent>(EncModifySkill_Apply);

            SubscribeComponent<EncSustainAttributeComponent, EntityBeingGeneratedEvent>(EncSustainAttribute_BeingGenerated);
            SubscribeComponent<EncSustainAttributeComponent, CalcEnchantmentAdjustedPowerEvent>(EncSustainAttribute_GetAdjustedPower);
            SubscribeComponent<EncSustainAttributeComponent, GetEnchantmentDescriptionEventArgs>(EncSustainAttribute_Localize);
            SubscribeComponent<EncSustainAttributeComponent, ApplyEnchantmentFoodEffectsEvent>(EncSustainAttribute_ApplyAfterEaten);

            SubscribeComponent<EncElementalDamageComponent, EntityBeingGeneratedEvent>(EncElementalDamage_BeingGenerated);
            SubscribeComponent<EncElementalDamageComponent, CalcEnchantmentAdjustedPowerEvent>(EncElementalDamage_GetAdjustedPower);
            SubscribeComponent<EncElementalDamageComponent, GetEnchantmentDescriptionEventArgs>(EncElementalDamage_Localize);
            SubscribeComponent<EncElementalDamageComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncElementalDamage_ApplyPhysicalAttack);

            SubscribeComponent<EncInvokeSpellComponent, EntityBeingGeneratedEvent>(EncInvokeSpell_BeingGenerated);
            SubscribeComponent<EncInvokeSpellComponent, CalcEnchantmentAdjustedPowerEvent>(EncInvokeSpell_GetAdjustedPower);
            SubscribeComponent<EncInvokeSpellComponent, GetEnchantmentDescriptionEventArgs>(EncInvokeSpell_Localize);
            SubscribeComponent<EncInvokeSpellComponent, ApplyEnchantmentPhysicalAttackEffectsEvent>(EncInvokeSpell_ApplyPhysicalAttack);

            SubscribeComponent<EncAmmoComponent, EntityBeingGeneratedEvent>(EncAmmo_BeingGenerated);
            SubscribeComponent<EncAmmoComponent, GetEnchantmentDescriptionEventArgs>(EncAmmo_Localize);
        }

        #region EncModifyAttribute

        private void EncModifyAttribute_BeingGenerated(EntityUid uid, EncModifyAttributeComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.SkillID = _skills.PickRandomBaseAttribute().GetStrongID();
            if (encArgs.OutCursePower > 0 && _rand.Next(100) < encArgs.OutCursePower)
                encArgs.OutPower *= -2;
        }

        private void EncModifyAttribute_GetAdjustedPower(EntityUid uid, EncModifyAttributeComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower = (int)Math.Ceiling(args.OutPower / 50f);
        }

        private void EncModifyAttribute_Localize(EntityUid uid, EncModifyAttributeComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower;
            args.OutShowPower = true;

            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            if (HasComp<FoodComponent>(args.Item))
            {
                if (args.AdjustedPower < 0)
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Food.Decreases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
                else
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Food.Increases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
            else
            {
                if (args.AdjustedPower < 0)
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Equipment.Decreases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
                else
                    args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Equipment.Increases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
        }

        private void EncModifyAttribute_Apply(EntityUid uid, EncModifyAttributeComponent component, ref ApplyEnchantmentEffectsEvent args)
        {
            if (_skills.TryGetKnown(args.Equipper, component.SkillID, out var skill))
                skill.Level.Buffed += args.AdjustedPower;
        }

        private void EncModifyAttribute_ApplyAfterEaten(EntityUid uid, EncModifyAttributeComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            var power = args.AdjustedPower + 1;

            var exp = power * 100;
            if (!_gameSession.IsPlayer(args.Eater))
                exp *= 5;

            _skills.GainSkillExp(args.Eater, component.SkillID, exp);

            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            if (exp < 0)
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Eaten.Decreases", ("chara", args.Eater), ("skillName", skillName)), entity: args.Eater);
            else
                _mes.Display(Loc.GetString("Elona.Enchantment.Item.ModifyAttribute.Eaten.Increases", ("chara", args.Eater), ("skillName", skillName)), entity: args.Eater);
        }

        #endregion

        #region EncModifyResistance

        private void EncModifyResistance_BeingGenerated(EntityUid uid, EncModifyResistanceComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.ElementID = _resists.PickRandomElementByRarity().GetStrongID();
            if (encArgs.OutCursePower > 0 && _rand.Next(100) < encArgs.OutCursePower)
                encArgs.OutPower *= -2;
        }

        private void EncModifyResistance_GetAdjustedPower(EntityUid uid, EncModifyResistanceComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 2;
        }

        private void EncModifyResistance_Localize(EntityUid uid, EncModifyResistanceComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower / ResistHelpers.LevelsPerGrade;
            args.OutShowPower = true;

            var elementName = Loc.GetPrototypeString(component.ElementID, "Name");

            if (args.AdjustedPower < 0)
                args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyResistance.Decreases", ("item", args.Item), ("elementName", elementName), ("adjustedPower", args.AdjustedPower));
            else
                args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifyResistance.Increases", ("item", args.Item), ("elementName", elementName), ("adjustedPower", args.AdjustedPower));
        }

        private void EncModifyResistance_Apply(EntityUid uid, EncModifyResistanceComponent component, ref ApplyEnchantmentEffectsEvent args)
        {
            if (_resists.TryGetKnown(args.Equipper, component.ElementID, out var resist))
            {
                resist.Level.Buffed += args.AdjustedPower;
                if (resist.Level.Buffed < 1)
                    resist.Level.Buffed = 1;
            }
        }

        #endregion

        #region EncModifySkill

        private void EncModifySkill_BeingGenerated(EntityUid uid, EncModifySkillComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.SkillID = _skills.PickRandomRegularSkillOrWeaponProficiency().GetStrongID();
            if (encArgs.OutCursePower > 0 && _rand.Next(100) < encArgs.OutCursePower)
                encArgs.OutPower *= -2;
        }

        private void EncModifySkill_GetAdjustedPower(EntityUid uid, EncModifySkillComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower = (int)Math.Ceiling(args.OutPower / 50f);
        }

        private void EncModifySkill_Localize(EntityUid uid, EncModifySkillComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower;
            args.OutShowPower = true;

            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            if (args.AdjustedPower < 0)
            {
                args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ModifySkill.Decreases", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
            else
            {
                if (!Loc.TryGetPrototypeString(component.SkillID, "EnchantmentDescription", out var desc, ("item", args.Item), ("power", args.AdjustedPower)))
                    desc = Loc.GetString("Elona.Enchantment.Item.ModifySkill.Increases", ("skillName", skillName));
                args.OutDescription = desc;
            }
        }

        private void EncModifySkill_Apply(EntityUid uid, EncModifySkillComponent component, ref ApplyEnchantmentEffectsEvent args)
        {
            if (_skills.TryGetKnown(args.Equipper, component.SkillID, out var skill))
            {
                skill.Level.Buffed += args.AdjustedPower;
                if (skill.Level.Buffed < 1)
                    skill.Level.Buffed = 1;
            }
        }

        #endregion

        #region EncSustainAttribute

        private void EncSustainAttribute_BeingGenerated(EntityUid uid, EncSustainAttributeComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.SkillID = _skills.PickRandomBaseAttribute().GetStrongID();
        }

        private void EncSustainAttribute_GetAdjustedPower(EntityUid uid, EncSustainAttributeComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower = (args.OutPower / 50) + 1;
        }

        private void EncSustainAttribute_Localize(EntityUid uid, EncSustainAttributeComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower / 5;
            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");

            if (HasComp<FoodComponent>(args.Item))
            {
                args.OutShowPower = true;
                args.OutDescription = Loc.GetString("Elona.Enchantment.Item.SustainAttribute.Food", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
            }
            else
                args.OutDescription = Loc.GetString("Elona.Enchantment.Item.SustainAttribute.Equipment", ("item", args.Item), ("skillName", skillName), ("power", args.AdjustedPower));
        }

        private void EncSustainAttribute_ApplyAfterEaten(EntityUid uid, EncSustainAttributeComponent component, ref ApplyEnchantmentFoodEffectsEvent args)
        {
            var skillName = Loc.GetPrototypeString(component.SkillID, "Name");
            _mes.Display(Loc.GetString("Elona.Enchantment.Item.SustainAttribute.Eaten", ("chara", args.Eater), ("skillName", skillName)), entity: args.Eater);

            // TODO food buffs
            _mes.Display("TODO food buffs", color: UiColors.MesYellow);
        }

        #endregion

        #region EncElementalDamage

        private void EncElementalDamage_BeingGenerated(EntityUid uid, EncElementalDamageComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            component.ElementID = _resists.PickRandomElementByRarity().GetStrongID();
        }

        private void EncElementalDamage_GetAdjustedPower(EntityUid uid, EncElementalDamageComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 2;
        }

        private void EncElementalDamage_Localize(EntityUid uid, EncElementalDamageComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            args.OutGrade = args.AdjustedPower / ResistHelpers.LevelsPerGrade;
            args.OutShowPower = true;

            var elementName = Loc.GetPrototypeString(component.ElementID, "Name");
            args.OutDescription = Loc.GetString("Elona.Enchantment.Item.ElementalDamage.Description", ("item", args.Item), ("elementName", elementName), ("adjustedPower", args.AdjustedPower));
        }

        private void EncElementalDamage_ApplyPhysicalAttack(EntityUid uid, EncElementalDamageComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (!IsAlive(args.Target) || args.PhysicalAttackArgs.RawDamage.OriginalDamage <= 1)
                return;

            var damage = _rand.Next(args.PhysicalAttackArgs.RawDamage.OriginalDamage * (100 + args.TotalPower) / 1000 + 1) + 5;
            var damageType = new ElementalDamageType(component.ElementID, args.TotalPower / 2 + 100);
            var extraArgs = new DamageHPExtraArgs()
            {
                DamageSubLevel = 1,
                NoAttackText = true
            };

            _damage.DamageHP(args.PhysicalAttackArgs.Target, damage, args.Attacker, damageType, extraArgs);
        }

        #endregion

        #region EncInvokeSpell

        private void EncInvokeSpell_BeingGenerated(EntityUid uid, EncInvokeSpellComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (!encArgs.Randomize)
                return;

            var item = encArgs.Item;

            bool Filter(EnchantmentSpellPrototype encSpellProto)
            {
                if (encSpellProto.ValidItemCategories == null)
                    return true;

                foreach (var tag in _tags.EnumerateTags(item))
                {
                    if (encSpellProto.ValidItemCategories.Contains(tag))
                        return true;
                }

                return false;
            }

            var candidates = _protos.EnumeratePrototypes<EnchantmentSpellPrototype>().Where(Filter).ToList();
            if (candidates.Count == 0)
            {
                encArgs.OutIsValid = false;
                return;
            }

            var sampler = new WeightedSampler<EnchantmentSpellPrototype>();
            foreach (var cand in candidates)
                sampler.Add(cand, cand.RandomWeight);

            var result = sampler.Sample();
            if (result == null)
            {
                encArgs.OutIsValid = false;
                return;
            }

            component.EnchantmentSpellID = result.GetStrongID();
        }

        private void EncInvokeSpell_GetAdjustedPower(EntityUid uid, EncInvokeSpellComponent component, ref CalcEnchantmentAdjustedPowerEvent args)
        {
            args.OutPower /= 50;
        }

        private void EncInvokeSpell_Localize(EntityUid uid, EncInvokeSpellComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            var encSpellProto = _protos.Index(component.EnchantmentSpellID);
            var spellName = Loc.GetPrototypeString(encSpellProto.SpellID, "Name");
            args.OutDescription = Loc.GetString("Elona.Enchantment.Item.InvokeSpell.Invokes", ("item", args.Item), ("spellName", spellName), ("adjustedPower", args.AdjustedPower));
        }

        private void EncInvokeSpell_ApplyPhysicalAttack(EntityUid uid, EncInvokeSpellComponent component, ref ApplyEnchantmentPhysicalAttackEffectsEvent args)
        {
            if (!IsAlive(args.Target))
                return;

            var encSpellProto = _protos.Index(component.EnchantmentSpellID);

            // TODO magic
            var spellPower = args.TotalPower + _skills.Level(args.Attacker, args.PhysicalAttackArgs.AttackSkill) * 10;
            _spells.Cast(encSpellProto.SpellID, spellPower, args.Target, args.Attacker);
        }

        #endregion

        #region EncAmmo

        private void EncAmmo_BeingGenerated(EntityUid uid, EncAmmoComponent component, ref EntityBeingGeneratedEvent args)
        {
            var encArgs = args.GenArgs.Get<EnchantmentGenArgs>();
            if (encArgs.Randomize)
            {
                if (!TryComp<AmmoComponent>(encArgs.Item, out var itemAmmo))
                {
                    encArgs.OutIsValid = false;
                    return;
                }

                var sampler = new WeightedSampler<AmmoEnchantmentPrototype>();
                foreach (var cand in _protos.EnumeratePrototypes<AmmoEnchantmentPrototype>())
                    sampler.Add(cand, cand.RandomWeight);

                var result = sampler.Sample();
                if (result == null)
                {
                    encArgs.OutIsValid = false;
                    return;
                }

                component.AmmoEnchantmentID = result.GetStrongID();
            }

            var ammoEncProto = _protos.Index(component.AmmoEnchantmentID);

            component.MaxAmmoAmount = Math.Clamp(encArgs.OutPower, 0, 500) * ammoEncProto.AmmoAmountFactor / 500;
            component.CurrentAmmoAmount = component.MaxAmmoAmount;
        }

        private void EncAmmo_Localize(EntityUid uid, EncAmmoComponent component, GetEnchantmentDescriptionEventArgs args)
        {
            var ammoName = Loc.GetPrototypeString(component.AmmoEnchantmentID, "Name");
            args.OutDescription = Loc.GetString("Elona.Enchantment.Item.Ammo.Description", ("item", args.Item), ("ammoName", ammoName), ("maxAmmo", component.MaxAmmoAmount));
        }

        #endregion
    }

    [ByRefEvent]
    public struct CalcEnchantmentAdjustedPowerEvent
    {
        public int OriginalPower { get; }
        public EntityUid Item { get; }

        public int OutPower { get; set; }

        public CalcEnchantmentAdjustedPowerEvent(int power, EntityUid item)
        {
            OriginalPower = power;
            Item = item;
            OutPower = power;
        }
    }

    public sealed class GetEnchantmentDescriptionEventArgs : EntityEventArgs
    {
        public int AdjustedPower { get; }
        public EntityUid Item { get; }

        public string OutDescription { get; set; }
        public int OutGrade { get; set; }
        public bool OutShowPower { get; set; } = false;

        public GetEnchantmentDescriptionEventArgs(int adjustedPower, EntityUid item, string description)
        {
            AdjustedPower = adjustedPower;
            Item = item;
            OutDescription = description;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentEffectsEvent
    {
        public int AdjustedPower { get; }
        public EntityUid Equipper { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentEffectsEvent(int power, EntityUid equipper, EntityUid item)
        {
            AdjustedPower = power;
            Equipper = equipper;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentFoodEffectsEvent
    {
        public int AdjustedPower { get; }
        public EntityUid Eater { get; }
        public EntityUid Item { get; }

        public ApplyEnchantmentFoodEffectsEvent(int power, EntityUid eater, EntityUid item)
        {
            AdjustedPower = power;
            Eater = eater;
            Item = item;
        }
    }

    [ByRefEvent]
    public struct ApplyEnchantmentPhysicalAttackEffectsEvent
    {
        public int TotalPower { get; }
        public int AdjustedPower { get; }
        public EntityUid Attacker { get; }
        public EntityUid Weapon => PhysicalAttackArgs.Weapon!.Value;
        public EntityUid Target => PhysicalAttackArgs.Target;
        public AfterPhysicalAttackHitEventArgs PhysicalAttackArgs { get; }

        public ApplyEnchantmentPhysicalAttackEffectsEvent(int totalPower, int adjustedPower, EntityUid attacker, AfterPhysicalAttackHitEventArgs args)
        {
            TotalPower = totalPower;
            AdjustedPower = adjustedPower;
            Attacker = attacker;
            PhysicalAttackArgs = args;
        }
    }
}
