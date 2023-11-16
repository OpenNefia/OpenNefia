using System.Linq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable, TestOf(typeof(EntityManager))]
    public class EntityManager_Components_Tests
    {
        private static readonly Vector2i DefaultCoords = Vector2i.Zero;

        [Test]
        public void AddComponentTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = new DummyComponent()
            {
                Owner = entity
            };

            // Act
            entMan.AddComponent(entity, component);

            // Assert
            var result = entMan.GetComponent<DummyComponent>(entity);
            Assert.That(result, Is.EqualTo(component));
        }

        [Test]
        public void AddComponentOverwriteTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = new DummyComponent()
            {
                Owner = entity
            };

            // Act
            entMan.AddComponent(entity, component, true);

            // Assert
            var result = entMan.GetComponent<DummyComponent>(entity);
            Assert.That(result, Is.EqualTo(component));
        }

        [Test]
        public void AddComponent_ExistingDeleted()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var firstComp = new DummyComponent { Owner = entity };
            entMan.AddComponent(entity, firstComp);
            entMan.RemoveComponent<DummyComponent>(entity);
            var secondComp = new DummyComponent { Owner = entity };

            // Act
            entMan.AddComponent(entity, secondComp);

            // Assert
            var result = entMan.GetComponent<DummyComponent>(entity);
            Assert.That(result, Is.EqualTo(secondComp));
        }

        [Test]
        public void HasComponentTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            entMan.AddComponent<DummyComponent>(entity);

            // Act
            var result = entMan.HasComponent<DummyComponent>(entity);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TryGetComponentTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = entMan.AddComponent<DummyComponent>(entity);

            // Act
            var result = entMan.TryGetComponent<DummyComponent>(entity, out var comp);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(comp, Is.EqualTo(component));
        }

        [Test]
        public void RemoveComponentTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = entMan.AddComponent<DummyComponent>(entity);

            // Act
            entMan.RemoveComponent<DummyComponent>(entity);
            entMan.CullRemovedComponents();

            // Assert
            Assert.That(entMan.HasComponent(entity, component.GetType()), Is.False);
        }

        [Test]
        public void GetComponentsTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = entMan.AddComponent<DummyComponent>(entity);

            // Act
            var result = entMan.GetComponents<DummyComponent>(entity);

            // Assert
            var list = result.ToList();
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(component));
        }

        [Test]
        public void GetAllComponentsTest()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = entMan.AddComponent<DummyComponent>(entity);

            // Act
            var result = entMan.EntityQuery<DummyComponent>();

            // Assert
            var list = result.ToList();
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(component));
        }

        [Test]
        public void GetAllComponentInstances()
        {
            // Arrange
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var compFac = sim.Resolve<IComponentFactory>();
            var entity = entMan.SpawnEntity(null, sim.ActiveMap!.AtPos(DefaultCoords));
            var component = entMan.AddComponent<DummyComponent>(entity);

            // Act
            var result = entMan.GetComponents(entity);

            // Assert
            var list = result.Where(c => compFac.GetComponentName(c.GetType()) == "Dummy").ToList();
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list[0], Is.EqualTo(component));
        }

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(factory => factory.RegisterClass<DummyComponent>())
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [RegisterComponent]
        private class DummyComponent : Component, ICompType1, ICompType2
        {       
        }

        private interface ICompType1 { }

        private interface ICompType2 { }
    }
}
