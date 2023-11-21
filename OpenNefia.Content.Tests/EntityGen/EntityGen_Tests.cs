using System.IO;
using NUnit.Framework;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Tests;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Content.Tests.EntityGen
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(EntityGenSystem))]
    public class EntityGen_Tests : ContentUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> IdEntityGenTestChara = new("EntityGenTestChara");
        private static readonly PrototypeId<EntityPrototype> IdEntityGenTestVillager = new("EntityGenTestVillager");

        private static readonly string Prototypes = $@"
- type: Entity
  id: EntityGenTest
  components:
  - type: Spatial
  - type: EntityGenTest

- type: Entity
  id: {IdEntityGenTestChara}
  components:
  - type: Spatial
    isSolid: true
  - type: Chara
  - type: EntityGenTest

- type: Entity
  id: {IdEntityGenTestVillager}
  components:
  - type: Spatial
    isSolid: true
  - type: Chara
  - type: EntityGenTest
  - type: RoleSpecial
";

        private static ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
               .NewSimulation()
               .RegisterComponents(factory =>
               {
                   factory.RegisterClass<EntityGenTestComponent>();
               })
               .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
               .RegisterEntitySystems(factory => factory.LoadExtraSystemType<EntityGenTestSystem>())
               .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void EntityGenEventsTest()
        {
            var sim = SimulationFactory();
            var mapMan = sim.Resolve<IMapManager>();

            var mapBlueprint = @"
meta:
  format: 1
  name: test
  author: ruin
grid: |
  .
tilemap:
  '.': Empty
entities:
- uid: 0
  components:
  - type: Map
- uid: 1
  protoId: EntityGenTest
  components:
  - type: Spatial
    parent: 0
    pos: 0,0
";

            var map = sim.Resolve<IMapLoader>().LoadBlueprint(new StringReader(mapBlueprint));

            var testComp = sim.GetEntitySystem<IEntityLookup>()
                .EntityQueryInMap<EntityGenTestComponent>(map.Id).First();

            Assert.That(testComp.Foo, Is.EqualTo(42));
        }

        [Test]
        public void EntityGenPositioningTest_Chara()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var entityA = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));

            var entityB = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));

            Assert.That(entMan.IsAlive(entityA), Is.True);
            Assert.That(entMan.IsAlive(entityB), Is.False);
            Assert.That(entityB, Is.Null);
        }

        [Test]
        public void EntityGenPositioningTest_Villager()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var entityA = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));

            // In vanilla, any character with a role (but not an adventurer) will try
            // to respawn on death. The EntityUid returned should be non-null but
            // IsAlive() should still report false.
            var entityB = entGen.SpawnEntity(IdEntityGenTestVillager, map.AtPos(0, 0));

            Assert.That(entMan.IsAlive(entityA), Is.True);
            Assert.That(entMan.IsAlive(entityB), Is.False);
            Assert.That(entityB, Is.Not.Null);
            Assert.That(entMan.GetComponent<CharaComponent>(entityB!.Value).Liveness, Is.EqualTo(CharaLivenessState.VillagerDead));
        }

        [Test]
        public void EntityGenPositioningTest_Global()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var entityA = entGen.SpawnEntity(IdEntityGenTestChara, MapCoordinates.Global);

            // Entities in the global map should always be placed on top of each other,
            // even if they're characters
            var entityB = entGen.SpawnEntity(IdEntityGenTestVillager, MapCoordinates.Global);

            Assert.That(entMan.IsAlive(entityA), Is.True);
            Assert.That(entMan.IsAlive(entityB), Is.True);
            Assert.That(entityB, Is.Not.Null);
            Assert.That(entMan.GetComponent<CharaComponent>(entityB!.Value).Liveness, Is.EqualTo(CharaLivenessState.Alive));
            Assert.That(entMan.GetComponent<SpatialComponent>(entityA!.Value).Coordinates, Is.EqualTo(entMan.GetComponent<SpatialComponent>(entityB.Value).Coordinates));
        }

        [Test]
        public void EntityGenPositioningTest_Villager_Override()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var entityA = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));

            // Override positioning so entities can spawn on top of each other
            var entityB = entGen.SpawnEntity(IdEntityGenTestVillager, map.AtPos(0, 0),
                args: EntityGenArgSet.Make(new EntityGenCommonArgs() { PositionSearchType = PositionSearchType.General }));

            Assert.That(entMan.IsAlive(entityA), Is.True);
            Assert.That(entMan.IsAlive(entityB), Is.True);
            Assert.That(entityB, Is.Not.Null);
            Assert.That(entMan.GetComponent<CharaComponent>(entityB!.Value).Liveness, Is.EqualTo(CharaLivenessState.Alive));
            Assert.That(entMan.GetComponent<SpatialComponent>(entityA!.Value).Coordinates, Is.EqualTo(entMan.GetComponent<SpatialComponent>(entityB.Value).Coordinates));
        }

        [Test]
        public void EntityGenPositioningTest_General()
        {
            var sim = SimulationFactory();
            var map = sim.CreateMapAndSetActive(1, 1);
            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var entityA = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));
            entMan.EnsureComponent<SpatialComponent>(entityA!.Value).IsSolid = true;

            var entityB = entGen.SpawnEntity(IdEntityGenTestChara, map.AtPos(0, 0));

            Assert.That(entMan.IsAlive(entityA), Is.True);
            Assert.That(entMan.IsAlive(entityB), Is.False);
        }

        [Reflect(false)]
        private class EntityGenTestSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeComponent<EntityGenTestComponent, EntityGeneratedEvent>(OnGen);
            }

            private void OnGen(EntityUid uid, EntityGenTestComponent component, ref EntityGeneratedEvent args)
            {
                component.Foo = 42;
            }
        }

        [DataDefinition]
        private sealed class EntityGenTestComponent : Component
        {
            [DataField("foo")]
            public int Foo { get; set; } = -1;
        }
    }
}