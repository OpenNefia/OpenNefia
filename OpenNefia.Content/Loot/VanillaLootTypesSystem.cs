using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Loot
{
    public sealed class VanillaLootTypesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ILootSystem _loot = default!;

        #region Elona.Animal

        public void Animal_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:244 	case lootAnimal ...
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatRemains, _loot.ModifyItemForRemains);
            // <<<<<<<< shade2/item.hsp:246 	swbreak ..
        }

        #endregion

        #region Elona.Insect

        public void Insect_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:248 	case lootInsect ...
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatRemains, _loot.ModifyItemForRemains);
            // <<<<<<<< shade2/item.hsp:250 	swbreak ..
        }

        #endregion

        #region Elona.Humanoid

        public void Humanoid_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:236 	case lootHumanoid ...
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatScroll);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWear);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWeapon);
            if (_rand.OneIn(40))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatGold);
            // <<<<<<<< shade2/item.hsp:242 	swbreak ..
        }

        #endregion

        #region Elona.Drake

        public void Drake_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:263 	case lootDrake ...
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWear);
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWeapon);
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatContainer);
            if (_rand.OneIn(4))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatGold);
            // <<<<<<<< shade2/item.hsp:268 	swbreak ..
        }

        #endregion

        #region Elona.Dragon

        public void Dragon_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:270 	case lootDragon ...
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWear);
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.TagSet.ItemWeapon);
            if (_rand.OneIn(15))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatSpellbook);
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
            if (_rand.OneIn(5))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatScroll);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatContainer);
            if (_rand.OneIn(4))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatGold);
            if (_rand.OneIn(4))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatOre);
            // <<<<<<<< shade2/item.hsp:279 	swbreak ..
        }

        #endregion

        #region Elona.Lich

        public void Lich_OnGenerateLoot(LootTypePrototype proto, ref P_LootTypeOnGenerateLootEvent ev)
        {
            // >>>>>>>> shade2/item.hsp:252 	case lootLich ...
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatEquipRing);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatEquipNeck);
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatSpellbook);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatDrink);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatScroll);
            if (_rand.OneIn(20))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatContainer);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatGold);
            if (_rand.OneIn(10))
                _loot.AddLootToResultList(ev.OutLootDrops, ev.Victim, Protos.Tag.ItemCatOre);
            // <<<<<<<< shade2/item.hsp:261 	swbreak ..
        }

        #endregion
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(LootTypePrototype))]
    public sealed class P_LootTypeOnGenerateLootEvent : PrototypeEventArgs
    {
        public EntityUid Victim { get; }

        public IList<LootDrop> OutLootDrops { get; }

        public P_LootTypeOnGenerateLootEvent(EntityUid victim, IList<LootDrop> lootDrops)
        {
            Victim = victim;
            OutLootDrops = lootDrops;
        }
    }
}