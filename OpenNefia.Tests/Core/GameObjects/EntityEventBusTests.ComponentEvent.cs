using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Tests.Core.GameObjects
{
    public partial class EntityEventBusTests
    {
        [Test]
        public void SubscribeCompEvent()
        {
            // Arrange
            var entUid = new EntityUid(7);
            var compInstance = new MetaDataComponent();

            var compRegistration = new Mock<IComponentRegistration>();

            var entManMock = new Mock<IEntityManager>();

            var compFacMock = new Mock<IComponentFactory>();

            compRegistration.Setup(m => m.References).Returns(new List<Type> {typeof(MetaDataComponent)});
            compFacMock.Setup(m => m.GetRegistration(typeof(MetaDataComponent))).Returns(compRegistration.Object);
            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);

            IComponent? outIComponent = compInstance;
            entManMock.Setup(m => m.TryGetComponent(entUid, typeof(MetaDataComponent), out outIComponent))
                .Returns(true);

            entManMock.Setup(m => m.GetComponent(entUid, typeof(MetaDataComponent)))
                .Returns(compInstance);

            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            int calledCount = 0;
            bus.SubscribeComponentEvent<MetaDataComponent, TestEvent>(HandleTestEvent);

            // add a component to the system
            entManMock.Raise(m=>m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(compInstance, entUid));

            // Raise
            var evntArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, evntArgs);

            // Assert
            Assert.That(calledCount, Is.EqualTo(1));
            void HandleTestEvent(EntityUid uid, MetaDataComponent component, TestEvent args)
            {
                calledCount++;
                Assert.That(uid, Is.EqualTo(entUid));
                Assert.That(component, Is.EqualTo(compInstance));
                Assert.That(args.TestNumber, Is.EqualTo(5));
            }
        }

        [Test]
        public void UnsubscribeCompEvent()
        {
            // Arrange
            var entUid = new EntityUid(7);
            var compInstance = new MetaDataComponent();

            var entManMock = new Mock<IEntityManager>();

            var compRegistration = new Mock<IComponentRegistration>();

            var compFacMock = new Mock<IComponentFactory>();

            compRegistration.Setup(m => m.References).Returns(new List<Type> {typeof(MetaDataComponent)});
            compFacMock.Setup(m => m.GetRegistration(typeof(MetaDataComponent))).Returns(compRegistration.Object);
            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);

            IComponent? outIComponent = compInstance;
            entManMock.Setup(m => m.TryGetComponent(entUid, typeof(MetaDataComponent), out outIComponent))
                .Returns(true);

            entManMock.Setup(m => m.GetComponent(entUid, typeof(MetaDataComponent)))
                .Returns(compInstance);

            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            int calledCount = 0;
            bus.SubscribeComponentEvent<MetaDataComponent, TestEvent>(HandleTestEvent);
            bus.UnsubscribeAllComponentEvents<MetaDataComponent, TestEvent>();

            // add a component to the system
            entManMock.Raise(m => m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(compInstance, entUid));

            // Raise
            var evntArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, evntArgs);

            // Assert
            Assert.That(calledCount, Is.EqualTo(0));
            void HandleTestEvent(EntityUid uid, MetaDataComponent component, TestEvent args)
            {
                calledCount++;
            }
        }

        [Test]
        public void SubscribeCompLifeEvent()
        {
            // Arrange
            var entUid = new EntityUid(7);
            var compInstance = new MetaDataComponent();

            var entManMock = new Mock<IEntityManager>();

            compInstance.Owner = entUid;

            var compRegistration = new Mock<IComponentRegistration>();

            var compFacMock = new Mock<IComponentFactory>();

            compRegistration.Setup(m => m.References).Returns(new List<Type> {typeof(MetaDataComponent)});
            compFacMock.Setup(m => m.GetRegistration(typeof(MetaDataComponent))).Returns(compRegistration.Object);
            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);

            IComponent? outIComponent = compInstance;
            entManMock.Setup(m => m.TryGetComponent(entUid, typeof(MetaDataComponent), out outIComponent))
                .Returns(true);

            entManMock.Setup(m => m.GetComponent(entUid, typeof(MetaDataComponent)))
                .Returns(compInstance);

            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            int calledCount = 0;
            bus.SubscribeComponentEvent<MetaDataComponent, ComponentInit>(HandleTestEvent);

            // add a component to the system
            entManMock.Raise(m=>m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(compInstance, entUid));

            // Raise
            ((IEventBus)bus).RaiseDirectedComponentEvent(compInstance, new ComponentInit());

            // Assert
            Assert.That(calledCount, Is.EqualTo(1));
            void HandleTestEvent(EntityUid uid, MetaDataComponent component, ComponentInit args)
            {
                calledCount++;
                Assert.That(uid, Is.EqualTo(entUid));
                Assert.That(component, Is.EqualTo(compInstance));
            }
        }

        [Test]
        public void CompEventOrdered()
        {
            // Arrange
            var entUid = new EntityUid(7);

            var entManMock = new Mock<IEntityManager>();
            var compFacMock = new Mock<IComponentFactory>();

            void Setup<T>(out T instance) where T : IComponent, new()
            {
                IComponent? inst = instance = new T();
                var reg = new Mock<IComponentRegistration>();
                reg.Setup(m => m.References).Returns(new Type[] {typeof(T)});

                compFacMock.Setup(m => m.GetRegistration(typeof(T))).Returns(reg.Object);
                entManMock.Setup(m => m.TryGetComponent(entUid, typeof(T), out inst)).Returns(true);
                entManMock.Setup(m => m.GetComponent(entUid, typeof(T))).Returns(inst);
            }

            Setup<OrderComponentA>(out var instA);
            Setup<OrderComponentB>(out var instB);
            Setup<OrderComponentC>(out var instC);
            Setup<OrderComponentC2>(out var instC2);

            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);
            var bus = new EntityEventBus(entManMock.Object);

            // Subscribe
            var a = false;
            var b = false;
            var c = false;
            var c2 = false;

            void HandlerA(EntityUid uid, Component comp, TestEvent ev)
            {
                Assert.That(b, Is.False, "A should run before B");
                Assert.That(c, Is.False, "A should run before C");
                Assert.That(c2, Is.False, "A should run before C2");

                a = true;
            }

            void HandlerB(EntityUid uid, Component comp, TestEvent ev)
            {
                Assert.That(c, Is.True, "B should run after C");
                Assert.That(c2, Is.False, "B should run before C2");
                b = true;
            }

            void HandlerC2(EntityUid uid, Component comp, TestEvent ev)
            {
                Assert.That(b, Is.True, "B should run before C2");
                Assert.That(c, Is.True, "C2 should run after C");
                c2 = true;
            };

            void HandlerC(EntityUid uid, Component comp, TestEvent ev) => c = true;

            bus.SubscribeComponentEvent<OrderComponentA, TestEvent>(HandlerA, EventPriorities.Highest);
            bus.SubscribeComponentEvent<OrderComponentB, TestEvent>(HandlerB);
            bus.SubscribeComponentEvent<OrderComponentC, TestEvent>(HandlerC, EventPriorities.High);
            bus.SubscribeComponentEvent<OrderComponentC2, TestEvent>(HandlerC2, EventPriorities.Low);

            // add a component to the system
            entManMock.Raise(m => m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instA, entUid));
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instB, entUid));
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instC, entUid));
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(instC2, entUid));

            // Raise
            var evntArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, evntArgs);

            // Assert
            Assert.That(a, Is.True, "A did not fire");
            Assert.That(b, Is.True, "B did not fire");
            Assert.That(c, Is.True, "C did not fire");
            Assert.That(c2, Is.True, "C2 did not fire");
        }

        [Test]
        public void DuplicateCompEventPairs()
        {
            // Arrange
            var entUid = new EntityUid(7);
            var compInstance = new MetaDataComponent();

            var compRegistration = new Mock<IComponentRegistration>();
            var entManMock = new Mock<IEntityManager>();
            var compFacMock = new Mock<IComponentFactory>();

            compRegistration.Setup(m => m.References).Returns(new List<Type> { typeof(MetaDataComponent) });
            compFacMock.Setup(m => m.GetRegistration(typeof(MetaDataComponent))).Returns(compRegistration.Object);
            entManMock.Setup(m => m.ComponentFactory).Returns(compFacMock.Object);

            IComponent? outIComponent = compInstance;
            entManMock.Setup(m => m.TryGetComponent(entUid, typeof(MetaDataComponent), out outIComponent))
                .Returns(true);
            entManMock.Setup(m => m.GetComponent(entUid, typeof(MetaDataComponent)))
                .Returns(compInstance);

            var bus = new EntityEventBus(entManMock.Object);

            var a = false;
            var b = false;

            void HandleTestEventA(EntityUid uid, MetaDataComponent component, TestEvent args)
            {
                Assert.That(b, Is.True, "A should run after B");
                a = true;
            }
            
            void HandleTestEventB(EntityUid uid, MetaDataComponent component, TestEvent args)
            {
                Assert.That(a, Is.False, "B should run before A.");
                b = true;
            }

            // Subscribe
            bus.SubscribeComponentEvent<MetaDataComponent, TestEvent>(HandleTestEventA, priority: EventPriorities.Low);
            bus.SubscribeComponentEvent<MetaDataComponent, TestEvent>(HandleTestEventB);

            // add a component to the system
            entManMock.Raise(m => m.EntityAdded += null, entManMock.Object, entUid);
            entManMock.Raise(m => m.ComponentAdded += null, new AddedComponentEventArgs(compInstance, entUid));

            // Raise
            var evntArgs = new TestEvent(5);
            bus.RaiseEvent(entUid, evntArgs);

            // Assert
            Assert.That(a, Is.True, "A did not fire.");
            Assert.That(a, Is.True, "B did not fire.");
        }

        private class DummyComponent : Component
        {
            public override string Name => "Dummy";
        }

        private class OrderComponentA : Component
        {
            public override string Name => "OrderComponentA";
        }

        private class OrderComponentB : Component
        {
            public override string Name => "OrderComponentB";
        }

        private class OrderComponentC : Component
        {
            public override string Name => "OrderComponentC";
        }
        
        private class OrderComponentC2 : Component
        {
            public override string Name => "OrderComponentC2";
        }

        private class DummyEventSubscriber : IEntityEventSubscriber {}

        private class TestEvent : EntityEventArgs
        {
            public int TestNumber { get; }

            public TestEvent(int testNumber)
            {
                TestNumber = testNumber;
            }
        }
    }
}
