using System;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Reflection;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture]
    public partial class EntityEventBusTests
    {
        [Test]
        public void SubscribeCompRefBroadcastEvent()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory => factory.LoadExtraSystemType<SubscribeCompRefBroadcastSystem>())
                .InitializeInstance();

            var ev = new TestStructEvent() { TestNumber = 5 };
            simulation.Resolve<IEntityManager>().EventBus.RaiseEvent(ref ev);
            Assert.That(ev.TestNumber, Is.EqualTo(15));
        }

        [Reflect(false)]
        public class SubscribeCompRefBroadcastSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeBroadcast<TestStructEvent>(OnTestEvent);
            }

            private void OnTestEvent(ref TestStructEvent ev)
            {
                Assert.That(ev.TestNumber, Is.EqualTo(5));
                ev.TestNumber += 10;
            }
        }

        [Test]
        public void SubscriptionNoMixedRefValueBroadcastEvent()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                    factory.LoadExtraSystemType<SubscriptionNoMixedRefValueBroadcastEventSystem>());

            // Act. No mixed ref and value subscriptions are allowed.
            Assert.Throws(typeof(InvalidOperationException), () => simulation.InitializeInstance());
        }

        [Reflect(false)]
        private class SubscriptionNoMixedRefValueBroadcastEventSystem : EntitySystem
        {
            public override void Initialize()
            {
                // The below is not allowed, as you're subscribing by-ref and by-value to the same event...
                SubscribeBroadcast<TestStructEvent>(MyRefHandler);
                SubscribeBroadcast<TestStructEvent>(MyValueHandler);
            }

            private void MyValueHandler(TestStructEvent args) { }
            private void MyRefHandler(ref TestStructEvent args) { }
        }

        [Test]
        public void SortedBroadcastRefEvents()
        {
            // Arrange.
            var simulation = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<BroadcastOrderASystem>();
                    factory.LoadExtraSystemType<BroadcastOrderBSystem>();
                    factory.LoadExtraSystemType<BroadcastOrderCSystem>();
                })
                .InitializeInstance();

            // Act.
            var testEvent = new TestStructEvent { TestNumber = 5 };
            var eventBus = simulation.Resolve<IEntityManager>().EventBus;
            eventBus.RaiseEvent(ref testEvent);

            // Check that the entity systems changed the value correctly
            Assert.That(testEvent.TestNumber, Is.EqualTo(15));
        }

        [Reflect(false)]
        private class BroadcastOrderASystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeBroadcast<TestStructEvent>(OnA);
            }

            private void OnA(ref TestStructEvent args)
            {
                // Second handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(0));
                args.TestNumber = 10;
            }
        }

        [Reflect(false)]
        private class BroadcastOrderBSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeBroadcast<TestStructEvent>(OnB, EventPriorities.Low);
            }

            private void OnB(ref TestStructEvent args)
            {
                // Last handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(10));
                args.TestNumber = 15;
            }
        }

        [Reflect(false)]
        private class BroadcastOrderCSystem : EntitySystem
        {
            public override void Initialize()
            {
                base.Initialize();

                SubscribeBroadcast<TestStructEvent>(OnC, EventPriorities.High);
            }

            private void OnC(ref TestStructEvent args)
            {
                // First handler being ran.
                Assert.That(args.TestNumber, Is.EqualTo(5));
                args.TestNumber = 0;
            }
        }
    }
}
