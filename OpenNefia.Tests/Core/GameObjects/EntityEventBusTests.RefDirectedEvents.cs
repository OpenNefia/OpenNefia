using System;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;

namespace OpenNefia.Tests.Core.GameObjects
{
    public partial class EntityEventBusTests
    {

        [Test]
        public void SubscribeCompRefDirectedEvent()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterComponents(factory => factory.RegisterClass<DummyComponent>())
                .RegisterEntitySystems(factory => factory.LoadExtraSystemType<SubscribeCompRefDirectedEventSystem>())
                .InitializeInstance();

            var map = simulation.CreateMapAndSetActive(50, 50);

            var entMan = simulation.Resolve<IEntityManager>();
            var entity = simulation.SpawnEntity(null, map.AtPos(0, 0));
            entMan.AddComponent<DummyComponent>(entity);

            // Act.
            var testEvent = new TestStructEvent {TestNumber = 5};
            var eventBus = simulation.Resolve<IEntityManager>().EventBus;
            eventBus.RaiseEvent(entity, ref testEvent);

            // Check that the entity system changed the value correctly
            Assert.That(testEvent.TestNumber, Is.EqualTo(10));
        }

        [Reflect(false)]
        private class SubscribeCompRefDirectedEventSystem : EntitySystem
        {
            public override void Initialize()
            {
                SubscribeComponent<DummyComponent, TestStructEvent>(MyRefHandler);
            }

            private void MyRefHandler(EntityUid uid, DummyComponent component, ref TestStructEvent args)
            {
                Assert.That(args.TestNumber, Is.EqualTo(5));
                args.TestNumber = 10;
            }
        }

        [Test]
        public void SubscriptionNoMixedRefValueDirectedEvent()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterComponents(factory =>
                {
                    factory.RegisterClass<DummyComponent>();
                    factory.RegisterClass<DummyTwoComponent>();
                })
                .RegisterEntitySystems(factory =>
                    factory.LoadExtraSystemType<SubscriptionNoMixedRefValueDirectedEventSystem>());

            // Act. No mixed ref and value subscriptions are allowed.
            Assert.Throws(typeof(InvalidOperationException), () => simulation.InitializeInstance());
        }

        [Reflect(false)]
        private class SubscriptionNoMixedRefValueDirectedEventSystem : EntitySystem
        {
            public override void Initialize()
            {
                // The below is not allowed, as you're subscribing by-ref and by-value to the same event...
                SubscribeComponent<DummyComponent, TestStructEvent>(MyRefHandler);
                SubscribeComponent<DummyTwoComponent, TestStructEvent>(MyValueHandler);
            }

            private void MyValueHandler(EntityUid uid, DummyTwoComponent component, TestStructEvent args) { }
            private void MyRefHandler(EntityUid uid, DummyComponent component, ref TestStructEvent args) { }
        }

        [Test]
        public void SortedDirectedRefEvents()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterComponents(factory =>
                {
                    factory.RegisterClass<OrderAComponent>();
                    factory.RegisterClass<OrderBComponent>();
                    factory.RegisterClass<OrderCComponent>();
                    factory.RegisterClass<OrderC2Component>();
                })
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<OrderASystem>();
                    factory.LoadExtraSystemType<OrderBSystem>();
                    factory.LoadExtraSystemType<OrderCSystem>();
                })
                .InitializeInstance();

            var map = simulation.CreateMapAndSetActive(50, 50);

            var entMan = simulation.Resolve<IEntityManager>();
            var entity = simulation.SpawnEntity(null, map.AtPos(0, 0));
            entMan.AddComponent<OrderAComponent>(entity);
            entMan.AddComponent<OrderBComponent>(entity);
            entMan.AddComponent<OrderCComponent>(entity);
            entMan.AddComponent<OrderC2Component>(entity);

            // Act.
            var testEvent = new TestStructEvent {TestNumber = 5};
            var eventBus = simulation.Resolve<IEntityManager>().EventBus;
            eventBus.RaiseEvent(entity, ref testEvent);

            // Check that the entity systems changed the value correctly
            Assert.That(testEvent.TestNumber, Is.EqualTo(20));
        }

        [Reflect(false)]
        private class OrderASystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeComponent<OrderAComponent, TestStructEvent>(OnA);
            }

            private void OnA(EntityUid uid, OrderAComponent component, ref TestStructEvent args)
            {
                // Third handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(15));
                args.TestNumber = 10;
            }
        }

        [Reflect(false)]
        private class OrderBSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeComponent<OrderBComponent, TestStructEvent>(OnB, EventPriorities.Low);
            }

            private void OnB(EntityUid uid, OrderBComponent component, ref TestStructEvent args)
            {
                // Last handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(10));
                args.TestNumber = 20;
            }
        }

        [Reflect(false)]
        private class OrderCSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeComponent<OrderCComponent, TestStructEvent>(OnC, EventPriorities.Highest);
                SubscribeComponent<OrderC2Component, TestStructEvent>(OnC2, EventPriorities.High);
            }

            private void OnC(EntityUid uid, OrderCComponent component, ref TestStructEvent args)
            {
                // First handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(5));
                args.TestNumber = 0;
            }

            private void OnC2(EntityUid uid, OrderC2Component component, ref TestStructEvent args)
            {
                // Second handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(0));
                args.TestNumber = 15;
            }
        }

        private class DummyTwoComponent : Component
        {        }

        [ByRefEvent]
        private struct TestStructEvent
        {
            public int TestNumber;
        }
    }
}
