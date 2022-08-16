using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Money;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Equipment
{
    public sealed class VanillaEquipmentTypesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILootSystem _loot = default!;
        [Dependency] private readonly IMoneySystem _money = default!;

        #region Elona.Warrior

        private PrototypeId<TagPrototype> RandomLightWeapon()
        {
            // >>>>>>>> shade2/chara.hsp:24 #defcfunc eqWeaponLight ...
            if (_rand.OneIn(2))
                return Tag.ItemCatEquipMeleeShortSword;
            if (_rand.OneIn(2))
                return Tag.ItemCatEquipMeleeHandAxe;
            return Tag.ItemCatEquipMeleeClub;
            // <<<<<<<< shade2/chara.hsp:27 	return fltClub ..
        }

        private PrototypeId<TagPrototype> RandomHeavyWeapon()
        {
            // >>>>>>>> shade2/chara.hsp:29 #defcfunc eqWeaponHeavy ...
            if (_rand.OneIn(3))
                return Tag.ItemCatEquipMeleeBroadsword;
            if (_rand.OneIn(3))
                return Tag.ItemCatEquipMeleeAxe;
            if (_rand.OneIn(3))
                return Tag.ItemCatEquipMeleeHalberd;
            return Tag.ItemCatEquipMeleeHammer;
            // <<<<<<<< shade2/chara.hsp:33 	return fltHammer ..
        }

        private PrototypeId<TagPrototype> RandomMageWeapon()
        {
            // >>>>>>>> shade2/chara.hsp:35 #module ...
            if (_rand.OneIn(2))
                return Tag.ItemCatEquipMeleeShortSword;
            return Tag.ItemCatEquipMeleeStaff;
            // <<<<<<<< shade2/chara.hsp:41 #global ..
        }

        private void AddSpec(P_EquipmentTypeOnInitializeEquipmentEvent ev, PrototypeId<EquipmentSpecPrototype> specID, PrototypeId<TagPrototype> category, Quality quality = Quality.Bad)
        {
            ev.OutEquipTemplate.Specifiers[specID] = (new(new ItemFilter() { Tags = new[] { category }, Quality = quality }));
        }

        public void Warrior_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, RandomHeavyWeapon(), Quality.Normal);
            AddSpec(ev, EquipmentSpec.Shield, Tag.ItemCatEquipShieldShield);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Helmet, Tag.ItemCatEquipHead);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Girdle, Tag.ItemCatEquipBackGirdle);
            AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedThrown);
        }

        public void Warrior_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Mage

        public void Mage_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, RandomMageWeapon(), Quality.Normal);
            AddSpec(ev, EquipmentSpec.Amulet1, Tag.ItemCatEquipNeckArmor);
            AddSpec(ev, EquipmentSpec.Ring1, Tag.ItemCatEquipRingRing, Quality.Normal);
            AddSpec(ev, EquipmentSpec.Ring2, Tag.ItemCatEquipRingRing);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyRobe);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Cloak, Tag.ItemCatEquipBackCloak);
        }

        public void Mage_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatDrink);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatSpellbook);
        }

        #endregion

        #region Elona.Archer

        public void Archer_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, Tag.ItemCatEquipMeleeLongSword);
            AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedBow, Quality.Normal);
            AddSpec(ev, EquipmentSpec.Ammo, Tag.ItemCatEquipAmmoArrow);
            AddSpec(ev, EquipmentSpec.Cloak, Tag.ItemCatEquipBackCloak);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Gloves, Tag.ItemCatEquipWristGauntlet);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
        }

        public void Archer_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Gunner

        public void Gunner_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, Tag.ItemCatEquipMeleeLongSword);
            if (!_rand.OneIn(4))
            {
                AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedGun, Quality.Normal);
                AddSpec(ev, EquipmentSpec.Ammo, Tag.ItemCatEquipAmmoBullet);
            }
            else
            {
                AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedLaserGun, Quality.Normal);
                AddSpec(ev, EquipmentSpec.Ammo, Tag.ItemCatEquipAmmoEnergyCell);
            }
            AddSpec(ev, EquipmentSpec.Cloak, Tag.ItemCatEquipBackCloak);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Gloves, Tag.ItemCatEquipWristGauntlet);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
        }

        public void Gunner_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.WarMage

        public void WarMage_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, RandomMageWeapon(), Quality.Normal);
            AddSpec(ev, EquipmentSpec.Amulet1, Tag.ItemCatEquipNeckArmor);
            AddSpec(ev, EquipmentSpec.Ring1, Tag.ItemCatEquipRingRing, Quality.Normal);
            AddSpec(ev, EquipmentSpec.Ring2, Tag.ItemCatEquipRingRing);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Cloak, Tag.ItemCatEquipBackCloak);
        }

        public void WarMage_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(50))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatSpellbook);
        }

        #endregion

        #region Elona.Priest

        public void Priest_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, Tag.ItemCatEquipMeleeClub);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Shield, Tag.ItemCatEquipShieldShield);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Helmet, Tag.ItemCatEquipHeadHelm);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
        }

        public void Priest_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
        }

        #endregion

        #region Elona.Thief

        public void Thief_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            AddSpec(ev, EquipmentSpec.PrimaryWeapon, RandomLightWeapon(), Quality.Normal);
            AddSpec(ev, EquipmentSpec.DualWieldWeapon, RandomLightWeapon(), Quality.Normal);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Armor, Tag.ItemCatEquipBodyMail);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Helmet, Tag.ItemCatEquipHeadHelm);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipBackGirdle);
            AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedThrown);
        }

        public void Thief_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Claymore

        public void Claymore_OnInitializeEquipment(EquipmentTypePrototype proto, P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);

            ev.OutEquipTemplate.Specifiers.Add(new(EquipmentSpec.TwoHandedWeapon, new ItemFilter() { Id = Item.Claymore, Quality = Quality.Good }));
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipLegHeavyBoots);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Girdle, Tag.ItemCatEquipBackGirdle);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Cloak, Tag.ItemCatEquipLegHeavyBoots);
            if (_rand.Prob(ev.ItemGenProb))
                AddSpec(ev, EquipmentSpec.Boots, Tag.ItemCatEquipBackGirdle);
            AddSpec(ev, EquipmentSpec.RangedWeapon, Tag.ItemCatEquipRangedThrown);
        }

        public void Claymore_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
        }

        #endregion
    }

    [PrototypeEvent(typeof(EquipmentTypePrototype))]
    public sealed class P_EquipmentTypeOnInitializeEquipmentEvent : PrototypeEventArgs
    {
        public EntityUid Chara { get; }
        public float ItemGenProb => OutEquipTemplate.ItemGenProb;

        public EquipmentTemplate OutEquipTemplate { get; }

        public P_EquipmentTypeOnInitializeEquipmentEvent(EntityUid chara, EquipmentTemplate equipTemplate)
        {
            Chara = chara;
            OutEquipTemplate = equipTemplate;
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(EquipmentTypePrototype))]
    public sealed class P_EquipmentTypeOnGenerateLootEvent : PrototypeEventArgs
    {
        public EntityUid Victim { get; }

        public IList<LootDrop> OutLootDrops { get; }

        public P_EquipmentTypeOnGenerateLootEvent(EntityUid victim, IList<LootDrop> lootDrops)
        {
            Victim = victim;
            OutLootDrops = lootDrops;
        }
    }
}