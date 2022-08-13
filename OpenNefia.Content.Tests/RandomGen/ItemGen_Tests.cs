using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.RandomGen
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(ItemGenSystem))]
    public class ItemGen_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
";

        [Test]
        public void TestItemGenSystem_GoldPiece()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var stacks = sim.GetEntitySystem<IStackSystem>();

            var sys = sim.GetEntitySystem<IItemGen>();

            var map = sim.CreateMapAndSetActive(10, 10);

            Assert.Multiple(() =>
            {
                var ent = sys.GenerateItem(map.AtPos(0, 0), Protos.Item.GoldPiece, amount: 15, minLevel: 30);
                Assert.That(entMan.IsAlive(ent), Is.True);
                Assert.That(stacks.GetCount(ent), Is.EqualTo(20));
                
                ent = sys.GenerateItem(map.AtPos(0, 0), tags: new[] { Protos.Tag.ItemCatGold }, amount: 15, minLevel: 30);
                Assert.That(entMan.IsAlive(ent), Is.True);
                Assert.That(entMan.GetComponent<MetaDataComponent>(ent!.Value).EntityPrototype!.GetStrongID(), Is.EqualTo(Protos.Item.GoldPiece));
                Assert.That(stacks.GetCount(ent), Is.EqualTo(16));
            });
        }
    }
}