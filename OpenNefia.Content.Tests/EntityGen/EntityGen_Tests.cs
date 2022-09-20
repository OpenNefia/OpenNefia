﻿using System.IO;
using NUnit.Framework;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Skills;
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
        private const string Prototypes = @"
- type: Entity
  id: EntityGenTest
  components:
  - type: Spatial
  - type: EntityGenTest
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