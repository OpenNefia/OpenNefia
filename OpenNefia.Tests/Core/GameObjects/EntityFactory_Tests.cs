using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FileFormats;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using YamlDotNet.RepresentationModel;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable, TestOf(typeof(EntityFactory))]
    public class EntityFactory_Tests : OpenNefiaUnitTest
    {
        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(factory => factory.RegisterClass<TestEntityFactoryComponent>())
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void Test_UpdateEntityComponents_AddComponent()
        {
            var sim = SimulationFactory();
            var entFac = sim.Resolve<IEntityFactory>();
            var entMan = sim.Resolve<IEntityManager>();
            var serMan = sim.Resolve<ISerializationManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(Vector2i.Zero));

            var comps = @"
- type: TestEntityFactory
  test1: 42
"
            ;

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(comps));
            var node = yamlStream.Documents[0].RootNode.ToDataNodeCast<SequenceDataNode>();
            var registry = serMan.Read<ComponentRegistry>(node);

            entFac.UpdateEntityComponents(entity, registry);

            Assert.Multiple(() =>
            {
                Assert.That(entMan.HasComponent<TestEntityFactoryComponent>(entity));

                var comp = entMan.GetComponent<TestEntityFactoryComponent>(entity);
                Assert.That(comp.Test1, Is.EqualTo(42));
                Assert.That(comp.Test2, Is.EqualTo(2));
            });
        }

        [Test]
        public void Test_UpdateEntityComponents_UpdateComponent()
        {
            var sim = SimulationFactory();
            var entFac = sim.Resolve<IEntityFactory>();
            var entMan = sim.Resolve<IEntityManager>();
            var serMan = sim.Resolve<ISerializationManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(Vector2i.Zero));
            var comp = entMan.AddComponent<TestEntityFactoryComponent>(entity);

            comp.Test1 = 3;
            comp.Test2 = 4;

            var comps = @"
- type: TestEntityFactory
  test1: 42
"
            ;

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(comps));
            var node = yamlStream.Documents[0].RootNode.ToDataNodeCast<SequenceDataNode>();
            var registry = serMan.Read<ComponentRegistry>(node);

            Assert.Multiple(() =>
            {
                Assert.That(comp.Test1, Is.EqualTo(3));
                Assert.That(comp.Test2, Is.EqualTo(4));
            });

            entFac.UpdateEntityComponents(entity, registry);

            Assert.Multiple(() =>
            {
                Assert.That(comp.Test1, Is.EqualTo(42));
                Assert.That(comp.Test2, Is.EqualTo(4));
            });
        }

        [Test]
        public void Test_UpdateEntityComponents_UpdateNested()
        {
            var sim = SimulationFactory();
            var entFac = sim.Resolve<IEntityFactory>();
            var entMan = sim.Resolve<IEntityManager>();
            var serMan = sim.Resolve<ISerializationManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(Vector2i.Zero));
            var comp = entMan.AddComponent<TestEntityFactoryComponent>(entity);

            comp.Test3 = new Dictionary<string, int>() {
                { "foo", 123 },
                { "bar", 456 }
            };

            var comps = @"
- type: TestEntityFactory
  test3:
    baz: 789
";

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(comps));
            var node = yamlStream.Documents[0].RootNode.ToDataNodeCast<SequenceDataNode>();
            var registry = serMan.Read<ComponentRegistry>(node);

            entFac.UpdateEntityComponents(entity, registry);

            Assert.That(comp.Test3, Is.EquivalentTo(new Dictionary<string, int>()
            {
                // TODO Would be very nice to have...
                // { "foo", 123 },
                // { "bar", 456 },
                { "baz", 789 }
            }));
        }

        [RegisterComponent]
        private class TestEntityFactoryComponent : Component
        {
            [DataField]
            public int Test1 { get; set; } = 1;

            [DataField]
            public int Test2 { get; set;  } = 2;

            [DataField]
            public Dictionary<string, int> Test3 { get; set; } = new();
        }
    }
}