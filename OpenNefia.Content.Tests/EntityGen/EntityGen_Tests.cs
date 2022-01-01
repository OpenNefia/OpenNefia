using System.IO;
using NUnit.Framework;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Tests;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Content.Tests.EntityGen;

[TestFixture]
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
        var sim = GameSimulation
           .NewSimulation()
           .RegisterComponents(factory =>
           {
               factory.RegisterClass<EntityGenTestComponent>();
           })
           .RegisterDataDefinitionTypes(types =>
           {
               types.Add(typeof(EntityGenTestComponent));
           })
           .RegisterDependencies(deps => deps.Register<IMapBlueprintLoader, MapBlueprintLoader>())
           .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
           .RegisterEntitySystems(factory =>
           {
               factory.LoadExtraSystemType<EntityGenSystem>();
               factory.LoadExtraSystemType<EntityGenTestSystem>();
           })
           .InitializeInstance();

        sim.CreateMapAndSetActive(50, 50);

        return sim;
    }

    [Test]
    public void EntityGenEventsTest()
    {
        var sim = SimulationFactory();

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

        var map = sim.Resolve<IMapBlueprintLoader>().LoadBlueprint(null, new StringReader(mapBlueprint));

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

            SubscribeLocalEvent<EntityGenTestComponent, EntityGeneratedEvent>(OnGen, "OnGen");
        }

        private void OnGen(EntityUid uid, EntityGenTestComponent component, ref EntityGeneratedEvent args)
        {
            component.Foo = 42;
        }
    }

    [DataDefinition]
    private sealed class EntityGenTestComponent : Component
    {
        public override string Name => "EntityGenTest";

        [DataField("foo")]
        public int Foo { get; set; } = -1;
    }
}