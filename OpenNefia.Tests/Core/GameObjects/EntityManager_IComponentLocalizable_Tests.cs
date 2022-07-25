using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable, TestOf(typeof(EntityManager))]
    public class EntityManager_IComponentLocalizable_Tests
    {
        private static readonly Vector2i DefaultCoords = Vector2i.Zero;
        private static readonly PrototypeId<EntityPrototype> TestEntityID = new("TestEntity");
        private static readonly PrototypeId<EntityPrototype> TestEntity2ID = new("TestEntity2");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntityID}
  components:
  - type: Spatial
  - type: DummyLocalizable

- type: Entity
  id: {TestEntity2ID}
  components:
  - type: Spatial
";

        private static readonly string Localizations = $@"
OpenNefia.Prototypes.Entity = {{
    {TestEntityID} = {{
        DummyLocalizable = {{
            LocalizedText = 'Foo'
        }}
    }},
    {TestEntity2ID} = {{
        DummyLocalizable = {{
            LocalizedText = 'Foo'
        }}
    }}
}}
";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterDependencies(factory => factory.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true))
                .RegisterComponents(factory => factory.RegisterClass<DummyLocalizableComponent>())
                .RegisterPrototypes(factory => factory.LoadString(Prototypes))
                .LoadLocalizations(factory => factory.LoadString(Localizations))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void SpawnEntityTest_IComponentLocalizable()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();

            // Act
            var entity = entMan.SpawnEntity(TestEntityID, sim.ActiveMap!.AtPos(DefaultCoords));

            // Assert
            var result = entMan.GetComponent<DummyLocalizableComponent>(entity);
            Assert.That(result.LocalizedText, Is.EqualTo("Foo"));
        }

        [Test]
        public void AddComponentTest_IComponentLocalizable()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(TestEntity2ID, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = new DummyLocalizableComponent()
            {
                Owner = entity
            };

            // Act
            entMan.AddComponent(entity, component);

            // Assert
            var result = entMan.GetComponent<DummyLocalizableComponent>(entity);
            Assert.That(result.LocalizedText, Is.EqualTo("Foo"));
        }

        [Test]
        public void EnsureComponentTest_IComponentLocalizable()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(TestEntity2ID, sim.ActiveMap!.AtPos(DefaultCoords));

            // Act
            var result = entMan.EnsureComponent<DummyLocalizableComponent>(entity);

            // Assert
            Assert.That(result.LocalizedText, Is.EqualTo("Foo"));
        }

        [RegisterComponent]
        private class DummyLocalizableComponent : Component, IComponentLocalizable
        {
            public override string Name => "DummyLocalizable";

            [Localize]
            public string? LocalizedText = null;

            void IComponentLocalizable.LocalizeFromLua(NLua.LuaTable table)
            {
                LocalizedText = table.GetStringOrNull(nameof(LocalizedText));
            }
        }
    }
}
