using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Charas
{
    [TestFixture]
    [TestOf(typeof(CharaSystem))]
    public class CharaSystem_Tests
    {
        private static readonly PrototypeId<EntityPrototype> TestCharaID = new("TestChara");

        private static readonly PrototypeId<EquipSlotPrototype> TestSlot1ID = new("TestSlot1");
        private static readonly PrototypeId<EquipSlotPrototype> TestSlot2ID = new("TestSlot2");

        private static readonly string Prototypes = @$"
- type: Elona.EquipSlot
  id: {TestSlot1ID}
- type: Elona.EquipSlot
  id: {TestSlot2ID}

- type: Elona.Race
  id: TestRace
  initialEquipSlots:
  - {TestSlot1ID}
  - {TestSlot1ID}
  - {TestSlot2ID}

- type: Elona.Class
  id: TestClass

- type: Entity
  id: TestChara
  parent: BaseChara
  components:
  - type: Chara
    race: TestRace
    class: TestClass
";

        private ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            return sim;
        }

        [Test]
        public void TestRaceEquipSlotsInitialization()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var invSys = sim.GetEntitySystem<InventorySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(invSys.HasEquipSlot(ent, TestSlot1ID), Is.True, "Has equip slot 1");
                Assert.That(invSys.HasEquipSlot(ent, TestSlot2ID), Is.True, "Has equip slot 2");

                Assert.That(entMan.TryGetComponent(ent, out InventoryComponent inventory), Is.True, "Has inventory component");

                Assert.That(inventory.EquipSlots.Count, Is.EqualTo(3), "Equip slots count");
                Assert.That(inventory.EquipSlots[0].ID, Is.EqualTo(TestSlot1ID), "Equip slot 1 ID");
                Assert.That(inventory.EquipSlots[1].ID, Is.EqualTo(TestSlot2ID), "Equip slot 2 ID");
                Assert.That(inventory.EquipSlots[2].ID, Is.EqualTo(TestSlot2ID), "Equip slot 3 ID");
            });
        }
    }
}
