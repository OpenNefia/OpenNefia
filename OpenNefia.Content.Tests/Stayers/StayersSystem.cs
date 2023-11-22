using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Stayers
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(StayersSystem))]
    public class StayersSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
";

        [Test]
        public void TestStayersSystem()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var stayers = sim.GetEntitySystem<IStayersSystem>();
            var transfer = sim.GetEntitySystem<IMapTransferSystem>();

            var map1 = sim.CreateMapAndSetActive(10, 10);
            var map2 = sim.CreateMap(10, 10);

            var player = entMan.SpawnEntity(TestEntity, map1.AtPos(5, 5));
            sim.Resolve<IGameSessionManager>().Player = player;

            var ally = entMan.SpawnEntity(TestEntity, map1.AtPos(1, 1));
            var allySpatial = entMan.GetComponent<SpatialComponent>(ally);
            var expectedPos = map1.AtPos(3, 4);

            stayers.RegisterStayer(ally, map1, StayingTags.Ally, expectedPos.Position);

            Assert.Multiple(() =>
            {
                Assert.That(stayers.IsStaying(ally, StayingTags.Ally), Is.True);
                Assert.That(stayers.IsStaying(ally, StayingTags.Adventurer), Is.False);
                Assert.That(stayers.EnumerateStayers(StayingTags.Ally).Count(), Is.EqualTo(0));
                Assert.That(entMan.IsAlive(ally), Is.True);
                Assert.That(allySpatial.MapPosition, Is.EqualTo(map1.AtPos(1, 1)));
            });

            transfer.DoMapTransfer(entMan.GetComponent<SpatialComponent>(player), map2, new CenterMapLocation(), MapLoadType.Full, noUnloadPrevious: true);

            Assert.Multiple(() =>
            {
                Assert.That(stayers.IsStaying(ally, StayingTags.Ally), Is.True);
                Assert.That(stayers.IsStaying(ally, StayingTags.Adventurer), Is.False);
                Assert.That(stayers.EnumerateStayers(StayingTags.Ally).Count(), Is.EqualTo(1));
                Assert.That(entMan.IsAlive(ally), Is.True);
                Assert.That(allySpatial.MapPosition, Is.EqualTo(MapCoordinates.Global));
            });

            transfer.DoMapTransfer(entMan.GetComponent<SpatialComponent>(player), map1, new CenterMapLocation(), MapLoadType.Full, noUnloadPrevious: true);

            Assert.Multiple(() =>
            {
                Assert.That(stayers.IsStaying(ally, StayingTags.Ally), Is.True);
                Assert.That(stayers.IsStaying(ally, StayingTags.Adventurer), Is.False);
                Assert.That(stayers.EnumerateStayers(StayingTags.Ally).Count(), Is.EqualTo(0));
                Assert.That(entMan.IsAlive(ally), Is.True);
                Assert.That(allySpatial.MapPosition, Is.EqualTo(expectedPos));
            });
        }
    }
}