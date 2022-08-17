using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Equipment
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(EquipmentGenSystem))]
    public class EquipmentGenSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity1 = new("TestEntity1");
        private static readonly PrototypeId<EntityPrototype> TestEntity2 = new("TestEntity2");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity1}
  components:
  - type: Inventory
  - type: EquipSlots
    initialEquipSlots:
    - Elona.Hand
    - Elona.Ranged
  - type: Equipment
  - type: EquipmentGen
    initialEquipment:
      Elona.PrimaryWeapon:
        itemFilter:
          id: Elona.ItemLongSword
      Elona.RangedWeapon:
        itemFilter:
          id: Elona.ItemBowOfVindale

- type: Entity
  id: {TestEntity2}
  components:
  - type: Inventory
  - type: EquipSlots
    initialEquipSlots:
    - Elona.Hand
    - Elona.Ranged
    - Elona.Ammo
    - Elona.Back
    - Elona.Body
    - Elona.Arm
    - Elona.Leg
  - type: Equipment
  - type: Quality
    quality: God # Always generate all equipment in the equipment type
  - type: EquipmentGen
    equipmentType: Elona.Archer
";

        [Test]
        public void TestEquipmentGenSystem_GenerateEquipmentTemplate()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var sys = sim.GetEntitySystem<IEquipmentGenSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent1 = entGen.SpawnEntity(TestEntity1, map.AtPos(0, 0))!.Value;
            var ent2 = entGen.SpawnEntity(TestEntity2, map.AtPos(0, 0))!.Value;

            Assert.Multiple(() =>
            {
                var template1 = sys.GenerateEquipmentTemplate(ent1);
                Assert.That(template1.Entries.Keys, Is.EquivalentTo(new[] { Protos.EquipmentSpec.PrimaryWeapon, Protos.EquipmentSpec.RangedWeapon }));
                Assert.That(template1.Entries[Protos.EquipmentSpec.PrimaryWeapon].ItemFilter.Id, Is.EqualTo(Protos.Item.LongSword));
                Assert.That(template1.Entries[Protos.EquipmentSpec.RangedWeapon].ItemFilter.Id, Is.EqualTo(Protos.Item.BowOfVindale));

                var template2 = sys.GenerateEquipmentTemplate(ent2);
                Assert.That(template2.Entries.Keys, Is.EquivalentTo(new[] { 
                    Protos.EquipmentSpec.PrimaryWeapon, 
                    Protos.EquipmentSpec.RangedWeapon,
                    Protos.EquipmentSpec.Ammo, 
                    Protos.EquipmentSpec.Cloak,
                    Protos.EquipmentSpec.Armor, 
                    Protos.EquipmentSpec.Gloves, 
                    Protos.EquipmentSpec.Boots 
                }));
            });
        }

        [Test]
        public void TestEquipmentGenSystem_GenerateEquipment()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var equipSlots = sim.GetEntitySystem<IEquipSlotsSystem>();

            var sys = sim.GetEntitySystem<IEquipmentGenSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent1 = entGen.SpawnEntity(TestEntity1, map.AtPos(0, 0))!.Value;
            var ent2 = entGen.SpawnEntity(TestEntity2, map.AtPos(0, 0))!.Value;

            Assert.Multiple(() =>
            {
                sys.GenerateEquipment(ent1);
                Assert.That(equipSlots.EnumerateEquippedEntities(ent1).Count(), Is.EqualTo(2));

                sys.GenerateEquipment(ent2);
                Assert.That(equipSlots.EnumerateEquippedEntities(ent2).Count(), Is.EqualTo(7));
            });
        }
    }
}