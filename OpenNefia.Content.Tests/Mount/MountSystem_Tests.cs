using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.Targetable;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Mount
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(MountSystem))]
    public class MountSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestRider = new("TestRider");
        private static readonly PrototypeId<EntityPrototype> TestMount = new("TestMount");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestRider}
  components:
  - type: Spatial
  - type: MountRider
  - type: Party
  - type: Skills

- type: Entity
  id: {TestMount}
  components:
  - type: Spatial
  - type: Mount
  - type: Party
  - type: Skills
";

        [Test]
        public void TestTryMount()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var playerRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));
            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 1));
            var partyMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 2));
            var npcMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 3));
            var partyMount2 = entMan.SpawnEntity(TestMount, map.AtPos(0, 4));
            var npcRider2 = entMan.SpawnEntity(TestRider, map.AtPos(0, 5));

            gameSession.Player = playerRider;

            Assert.Multiple(() =>
            {
                Assert.That(parties.RecruitAsAlly(playerRider, partyMount), Is.True, "Recruit ally");
                Assert.That(parties.RecruitAsAlly(playerRider, partyMount2), Is.True, "Recruit ally");

                Assert.That(mounts.TryMount(playerRider, playerRider), Is.False, "Attempting to ride self");
                Assert.That(mounts.TryMount(playerRider, npcRider), Is.False, "Can't ride non-mount");
                Assert.That(mounts.TryMount(partyMount, npcRider), Is.False, "Can't ride as a non-mount");
                Assert.That(mounts.TryMount(partyMount, npcMount), Is.False, "Can't ride as a non-mount");

                Assert.That(mounts.TryMount(playerRider, npcMount), Is.False, "Player can't ride a non-ally");
                Assert.That(mounts.TryMount(npcRider, partyMount), Is.False, "Non-players can't ride an ally");
                Assert.That(mounts.TryMount(playerRider, partyMount), Is.True, "Success");

                Assert.That(mounts.TryMount(playerRider, partyMount2), Is.False, "Player can't ride when already riding");

                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Non-players can ride a non-ally");
                Assert.That(mounts.TryMount(npcRider2, npcMount), Is.False, "Cannot ride something already ridden");
            });
        }

        [Test]
        public void TestMountEvents_RiderDeleted()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));
            var npcMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 1));

            var riderSpatial = entMan.GetComponent<SpatialComponent>(npcRider);
            var mountSpatial = entMan.GetComponent<SpatialComponent>(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Mount");
                Assert.That(riderSpatial.Coordinates, Is.EqualTo(mountSpatial.Coordinates));

                Assert.That(mounts.IsMounting(npcRider), Is.True);
                Assert.That(mounts.IsBeingMounted(npcMount), Is.True);
                Assert.That(entMan.GetComponent<MountComponent>(npcMount).Rider, Is.EqualTo(npcRider));
                Assert.That(entMan.GetComponent<TargetableComponent>(npcMount).IsTargetable.Buffed, Is.False);
            });

            entMan.DeleteEntity(npcRider);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.IsBeingMounted(npcMount), Is.False);
                Assert.That(mounts.IsMounting(npcRider), Is.False);
                Assert.That(entMan.GetComponent<MountComponent>(npcMount).Rider, Is.Null);
                Assert.That(entMan.GetComponent<TargetableComponent>(npcMount).IsTargetable.Buffed, Is.True);
            });
        }

        [Test]
        public void TestMountEvents_RiderKilled()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();
            var damage = sim.GetEntitySystem<IDamageSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));
            var npcMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 1));

            var riderSpatial = entMan.GetComponent<SpatialComponent>(npcRider);
            var mountSpatial = entMan.GetComponent<SpatialComponent>(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Mount");
                Assert.That(riderSpatial.Coordinates, Is.EqualTo(mountSpatial.Coordinates));

                Assert.That(mounts.IsMounting(npcRider), Is.True);
                Assert.That(mounts.IsBeingMounted(npcMount), Is.True);
                Assert.That(entMan.GetComponent<MountComponent>(npcMount).Rider, Is.EqualTo(npcRider));
                Assert.That(entMan.GetComponent<TargetableComponent>(npcMount).IsTargetable.Buffed, Is.False);
            });

            damage.Kill(npcRider);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.IsBeingMounted(npcMount), Is.False);
                Assert.That(mounts.IsMounting(npcRider), Is.False);
                Assert.That(entMan.GetComponent<MountComponent>(npcMount).Rider, Is.Null);
                Assert.That(entMan.GetComponent<TargetableComponent>(npcMount).IsTargetable.Buffed, Is.True);
            });
        }

        [Test]
        public void TestMountEvents_MountDeleted()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));
            var npcMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 1));

            var riderSpatial = entMan.GetComponent<SpatialComponent>(npcRider);
            var mountSpatial = entMan.GetComponent<SpatialComponent>(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Mount");
                Assert.That(riderSpatial.Coordinates, Is.EqualTo(mountSpatial.Coordinates));

                Assert.That(mounts.IsMounting(npcRider), Is.True);
                Assert.That(mounts.IsBeingMounted(npcMount), Is.True);
                Assert.That(entMan.GetComponent<MountRiderComponent>(npcRider).Mount, Is.EqualTo(npcMount));
            });

            entMan.DeleteEntity(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.IsBeingMounted(npcMount), Is.False);
                Assert.That(mounts.IsMounting(npcRider), Is.False);
                Assert.That(entMan.GetComponent<MountRiderComponent>(npcRider).Mount, Is.Null);
            });
        }

        [Test]
        public void TestMountEvents_MountKilled()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();
            var damage = sim.GetEntitySystem<IDamageSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));
            var npcMount = entMan.SpawnEntity(TestMount, map.AtPos(0, 1));

            var riderSpatial = entMan.GetComponent<SpatialComponent>(npcRider);
            var mountSpatial = entMan.GetComponent<SpatialComponent>(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.TryMount(npcRider, npcMount), Is.True, "Mount");
                Assert.That(riderSpatial.Coordinates, Is.EqualTo(mountSpatial.Coordinates));

                Assert.That(mounts.IsMounting(npcRider), Is.True);
                Assert.That(mounts.IsBeingMounted(npcMount), Is.True);
                Assert.That(entMan.GetComponent<MountRiderComponent>(npcRider).Mount, Is.EqualTo(npcMount));
            });

            damage.Kill(npcMount);

            Assert.Multiple(() =>
            {
                Assert.That(mounts.IsBeingMounted(npcMount), Is.False);
                Assert.That(mounts.IsMounting(npcRider), Is.False);
                Assert.That(entMan.GetComponent<MountRiderComponent>(npcRider).Mount, Is.Null);
            });
        }

        [Test]
        public void TestTryMount_Blocking()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var gameSession = sim.Resolve<IGameSessionManager>();

            var mounts = sim.GetEntitySystem<IMountSystem>();
            var parties = sim.GetEntitySystem<IPartySystem>();
            var stayers = sim.GetEntitySystem<IStayersSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var npcRider = entMan.SpawnEntity(TestRider, map.AtPos(0, 0));

            var npcMount1 = entMan.SpawnEntity(TestMount, map.AtPos(0, 1));
            var npcMount2 = entMan.SpawnEntity(TestMount, map.AtPos(0, 2));
            var npcMount3 = entMan.SpawnEntity(TestMount, map.AtPos(0, 3));

            entMan.EnsureComponent<TemporaryAllyComponent>(npcMount1);
            entMan.EnsureComponent<EscortedInQuestComponent>(npcMount2);
            stayers.RegisterStayer(npcMount3, map, "Test");

            Assert.Multiple(() =>
            {
                Assert.That(mounts.TryMount(npcRider, npcMount1), Is.False);
                Assert.That(mounts.TryMount(npcRider, npcMount2), Is.False);
                Assert.That(mounts.TryMount(npcRider, npcMount3), Is.False);
            });
        }
    }
}