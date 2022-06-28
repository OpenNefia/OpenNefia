using Moq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Tests.Core.GameObjects
{
    public partial class EntityEventBusTests
    {
        [Test]
        public void SubscribeEntityEvent()
        {
            // Arrange
            var entUid = new EntityUid(7);

            var entManMock = new Mock<IEntityManager>();

            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            var calledCount = 0;

            bus.SubscribeEntityEvent<TestEvent>(HandleTestEvent);

            // Event tables won't be initialized for an entity unless the entity manager creates it.
            entManMock.Raise(m => m.EntityAdded += null, entManMock.Object, entUid);

            // Raise
            var eventArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, eventArgs);

            // Assert
            Assert.That(calledCount, Is.EqualTo(1));
            void HandleTestEvent(EntityUid uid, TestEvent args)
            {
                calledCount++;
                Assert.That(uid, Is.EqualTo(entUid));
                Assert.That(args.TestNumber, Is.EqualTo(5));
            }
        }

        [Test]
        public void CompAndEntityEventsOrdered()
        {
            // Arrange
            var entUid = new EntityUid(7);

            var entManMock = new Mock<IEntityManager>();
            var compFacMock = new Mock<IComponentFactory>();

            void Setup<T>(out T instance) where T : IComponent, new()
            {
                IComponent? inst = instance = new T();
                var reg = new Mock<IComponentRegistration>();
                reg.Setup(m => m.References).Returns(new Type[] { typeof(T) });

                compFacMock.Setup(m => m.GetRegistration(typeof(T))).Returns(reg.Object);
                entManMock.Setup(m => m.TryGetComponent(entUid, typeof(T), out inst)).Returns(true);
                entManMock.Setup(m => m.GetComponent(entUid, typeof(T))).Returns(inst);
            }

            Setup<OrderComponentA>(out var instA);
            Setup<OrderComponentB>(out var instB);

            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);
            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            var a = false;
            var entity = false;
            var b = false;

            void HandlerA(EntityUid uid, Component comp, TestEvent ev)
            {
                Assert.That(b, Is.False, "A should run before B");
                Assert.That(entity, Is.True, "A should run after entity");

                a = true;
            }

            void HandlerEntity(EntityUid uid, TestEvent ev)
            {
                Assert.That(a, Is.False, "Entity should run before A");
                Assert.That(b, Is.False, "Entity should run before B");
                entity = true;
            }

            void HandlerB(EntityUid uid, Component comp, TestEvent ev)
            {
                Assert.That(a, Is.True, "B should run after A");
                Assert.That(entity, Is.True, "B should run after entity");
                b = true;
            }

            bus.SubscribeComponentEvent<OrderComponentA, TestEvent>(HandlerA, EventPriorities.High);
            bus.SubscribeEntityEvent<TestEvent>(HandlerEntity, EventPriorities.VeryHigh);
            bus.SubscribeComponentEvent<OrderComponentB, TestEvent>(HandlerB, EventPriorities.VeryLow);

            // add a component to the system
            entManMock.Raise(m => m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instA, entUid));
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instB, entUid));

            // Raise
            var evntArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, evntArgs);

            // Assert
            Assert.That(a, Is.True, "A did not fire");
            Assert.That(entity, Is.True, "Entity did not fire");
            Assert.That(b, Is.True, "B did not fire");
        }
    }
}