using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
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
        private static readonly PrototypeId<EntityPrototype> TestEntity1 = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity1}
  components:
  - type: Inventory
  - type: EquipSlots
  - type: Equipment
  - type: EquipmentGen
    initialEquipment:
      Elona.PrimaryWeapon:
        itemFilter:
          id: Elona.ItemLongSword
      Elona.RangedWeapon:
        itemFilter:
          id: Elona.ItemBowOfVindale

";

        [Test]
        public void TestEquipmentGenSystem()
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

            var ent = entMan.SpawnEntity(TestEntity1, map.AtPos(0, 0));

            Assert.Multiple(() =>
            {
                var template = sys.GenerateEquipmentTemplate(ent);
                Assert.That(template.Entries.Count, Is.EqualTo(2));
            });
        }
    }
}