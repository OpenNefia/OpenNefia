using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Money;

namespace OpenNefia.Content.Equipment
{
    public sealed class VanillaEquipmentTypesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILootSystem _loot = default!;
        [Dependency] private readonly IMoneySystem _money = default!;

        #region Elona.Warrior

        public void Warrior_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Warrior_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Mage

        public void Mage_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Mage_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatSpellbook);
        }

        #endregion

        #region Elona.Archer

        public void Archer_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Archer_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Gunner

        public void Gunner_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Gunner_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.WarMage

        public void WarMage_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void WarMage_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(50))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatSpellbook);
        }

        #endregion

        #region Elona.Priest

        public void Priest_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Priest_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
        }

        #endregion

        #region Elona.Thief

        public void Thief_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Thief_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
        }

        #endregion

        #region Elona.Claymore

        public void Claymore_OnInitializeEquipment(EquipmentTypePrototype proto, ref P_EquipmentTypeOnInitializeEquipmentEvent ev)
        {
            _money.TryGenerateExtraGoldForChara(ev.Chara);
        }

        public void Claymore_OnGenerateLoot(EquipmentTypePrototype proto, ref P_EquipmentTypeOnGenerateLootEvent ev)
        {
        }

        #endregion
    }

    // TODO
    [ByRefEvent]
    [PrototypeEvent(typeof(EquipmentTypePrototype))]
    public sealed class P_EquipmentTypeOnInitializeEquipmentEvent : PrototypeEventArgs
    {
        public EntityUid Chara { get; }

        public P_EquipmentTypeOnInitializeEquipmentEvent(EntityUid chara)
        {
            Chara = chara;
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