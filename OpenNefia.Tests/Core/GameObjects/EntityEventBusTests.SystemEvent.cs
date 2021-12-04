using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable, TestOf(typeof(EntityEventBus))]
    public partial class EntityEventBusTests
    {
        private static EntityEventBus BusFactory()
        {
            var entManMock = new Mock<IEntityManager>();
            var bus = new EntityEventBus(entManMock.Object);
            return bus;
        }

        /// <summary>
        /// Trying to subscribe a null handler causes a <see cref="ArgumentNullException"/> to be thrown.
        /// </summary>
        [Test]
        public void SubscribeEvent_NullHandler_NullArgumentException()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            // Act
            void Code() => bus.SubscribeEvent(EventSource.Local, subscriber, (EntityEventHandler<TestEventArgs>) null!);

            //Assert
            Assert.Throws<ArgumentNullException>(Code);
        }

        /// <summary>
        /// Trying to subscribe with a null subscriber causes a <see cref="ArgumentNullException"/> to be thrown.
        /// </summary>
        [Test]
        public void SubscribeEvent_NullSubscriber_NullArgumentException()
        {
            // Arrange
            var bus = BusFactory();

            // Act
            void Code() => bus.SubscribeEvent<TestEventArgs>(EventSource.Local, null!, ev => {});

            //Assert: this should do nothing
            Assert.Throws<ArgumentNullException>(Code);
        }

        /// <summary>
        /// Unlike C# events, the set of event handler delegates is unique.
        /// Subscribing the same delegate multiple times will only call the handler once.
        /// </summary>
        [Test]
        public void SubscribeEvent_DuplicateSubscription_RaisedOnce()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delegateCallCount = 0;
            void Handler(TestEventArgs ev) => delegateCallCount++;

            // 2 subscriptions 1 handler
            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, Handler);
            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, Handler);

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            //Assert
            Assert.That(delegateCallCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Subscribing two different delegates to a single event type causes both events
        /// to be raised in an indeterminate order.
        /// </summary>
        [Test]
        public void SubscribeEvent_MultipleDelegates_BothRaised()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delFooCount = 0;
            int delBarCount = 0;

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, ev => delFooCount++);
            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, ev => delBarCount++);

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            // Assert
            Assert.That(delFooCount, Is.EqualTo(1));
            Assert.That(delBarCount, Is.EqualTo(1));
        }

        /// <summary>
        /// A subscriber's handlers are properly called only when the specified event type is raised.
        /// </summary>
        [Test]
        public void SubscribeEvent_MultipleSubscriptions_IndividuallyCalled()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delFooCount = 0;
            int delBarCount = 0;

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, ev => delFooCount++);
            bus.SubscribeEvent<TestEventTwoArgs>(EventSource.Local, subscriber, ev => delBarCount++);

            // Act & Assert
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());
            Assert.That(delFooCount, Is.EqualTo(1));
            Assert.That(delBarCount, Is.EqualTo(0));

            delFooCount = delBarCount = 0;

            bus.RaiseEvent(EventSource.Local, new TestEventTwoArgs());
            Assert.That(delFooCount, Is.EqualTo(0));
            Assert.That(delBarCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Trying to subscribe with <see cref="EventSource.None"/> makes no sense and causes
        /// a <see cref="ArgumentOutOfRangeException"/> to be thrown.
        /// </summary>
        [Test]
        public void SubscribeEvent_SourceNone_ArgOutOfRange()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            void TestEventHandler(TestEventArgs args) { }

            // Act
            void Code() => bus.SubscribeEvent(EventSource.None, subscriber, (EntityEventHandler<TestEventArgs>)TestEventHandler);

            //Assert
            Assert.Throws<ArgumentOutOfRangeException>(Code);
        }

        /// <summary>
        /// Unsubscribing a handler twice does nothing.
        /// </summary>
        [Test]
        public void UnsubscribeEvent_DoubleUnsubscribe_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            void Handler(TestEventArgs ev) { }

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, Handler);
            bus.UnsubscribeEvent<TestEventArgs>(EventSource.Local, subscriber);

            // Act
            bus.UnsubscribeEvent<TestEventArgs>(EventSource.Local, subscriber);

            // Assert: Does not throw
        }

        /// <summary>
        /// Unsubscribing a handler that was never subscribed in the first place does nothing.
        /// </summary>
        [Test]
        public void UnsubscribeEvent_NoSubscription_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            // Act
            bus.UnsubscribeEvent<TestEventArgs>(EventSource.Local, subscriber);

            // Assert: Does not throw
        }

        /// <summary>
        /// Trying to unsubscribe with a null subscriber causes a <see cref="ArgumentNullException"/> to be thrown.
        /// </summary>
        [Test]
        public void UnsubscribeEvent_NullSubscriber_NullArgumentException()
        {
            // Arrange
            var bus = BusFactory();

            // Act
            void Code() => bus.UnsubscribeEvent<TestEventArgs>(EventSource.Local, null!);

            // Assert
            Assert.Throws<ArgumentNullException>(Code);
        }

        /// <summary>
        /// An event cannot be subscribed to with <see cref="EventSource.None"/>, so trying to unsubscribe
        /// with an <see cref="EventSource.None"/> causes a <see cref="ArgumentOutOfRangeException"/> to be thrown.
        /// </summary>
        [Test]
        public void UnsubscribeEvent_SourceNone_ArgOutOfRange()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            // Act
            void Code() => bus.UnsubscribeEvent<TestEventArgs>(EventSource.None, subscriber);

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(Code);
        }

        /// <summary>
        /// Raising an event with no handlers subscribed to it does nothing.
        /// </summary>
        [Test]
        public void RaiseEvent_NoSubscriptions_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delCalledCount = 0;
            bus.SubscribeEvent<TestEventTwoArgs>(EventSource.Local, subscriber, ev => delCalledCount++);

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            // Assert
            Assert.That(delCalledCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Raising an event when a handler has been unsubscribed no longer calls the handler.
        /// </summary>
        [Test]
        public void RaiseEvent_Unsubscribed_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delCallCount = 0;
            void Handler(TestEventArgs ev) => delCallCount++;

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, Handler);
            bus.UnsubscribeEvent<TestEventArgs>(EventSource.Local, subscriber);

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            // Assert
            Assert.That(delCallCount, Is.EqualTo(0));
        }

        /// <summary>
        /// Trying to raise an event with <see cref="EventSource.None"/> makes no sense and causes
        /// a <see cref="ArgumentOutOfRangeException"/> to be thrown.
        /// </summary>
        [Test]
        public void RaiseEvent_SourceNone_ArgOutOfRange()
        {
            // Arrange
            var bus = BusFactory();

            // Act
            void Code() => bus.RaiseEvent(EventSource.None, new TestEventArgs());

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(Code);
        }

        /// <summary>
        /// Trying to unsubscribe all of a null subscriber's events causes a <see cref="ArgumentNullException"/> to be thrown.
        /// </summary>
        [Test]
        public void UnsubscribeEvents_NullSubscriber_NullArgumentException()
        {
            // Arrange
            var bus = BusFactory();

            // Act
            void Code() => bus.UnsubscribeEvents(null!);

            // Assert
            Assert.Throws<ArgumentNullException>(Code);
        }

        /// <summary>
        /// Unsubscribing a subscriber with no subscriptions does nothing.
        /// </summary>
        [Test]
        public void UnsubscribeEvents_NoSubscriptions_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            // Act
            bus.UnsubscribeEvents(subscriber);

            // Assert: no exception
        }

        /// <summary>
        /// The subscriber's handlers are not raised after they are unsubscribed.
        /// </summary>
        [Test]
        public void UnsubscribeEvents_UnsubscribedHandler_Nop()
        {
            // Arrange
            var bus = BusFactory();
            var subscriber = new TestEventSubscriber();

            int delCallCount = 0;
            void Handler(TestEventArgs ev) => delCallCount++;

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, subscriber, Handler);
            bus.UnsubscribeEvents(subscriber);

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            // Assert
            Assert.That(delCallCount, Is.EqualTo(0));
        }

        [Test]
        public void RaiseEvent_Ordered()
        {
            // Arrange
            var bus = BusFactory();

            // Expected order is A -> C -> B
            var a = false;
            var b = false;
            var c = false;

            void HandlerA(TestEventArgs ev)
            {
                Assert.That(b, Is.False, "A should run before B");
                Assert.That(c, Is.False, "A should run before C");

                a = true;
            }

            void HandlerB(TestEventArgs ev)
            {
                Assert.That(c, Is.True, "B should run after C");
                b = true;
            }

            void HandlerC(TestEventArgs ev) => c = true;

            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, new SubA(), HandlerA,
                new SubId(typeof(SubA), "A"), before: new []{new SubId(typeof(SubB), "B"), new SubId(typeof(SubC), "C")});
            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, new SubB(), HandlerB, 
                new SubId(typeof(SubB), "B"), after: new []{new SubId(typeof(SubC), "C")});
            bus.SubscribeEvent<TestEventArgs>(EventSource.Local, new SubC(), HandlerC, 
                new SubId(typeof(SubC), "C"));

            // Act
            bus.RaiseEvent(EventSource.Local, new TestEventArgs());

            // Assert
            Assert.That(a, Is.True, "A did not fire");
            Assert.That(b, Is.True, "B did not fire");
            Assert.That(c, Is.True, "C did not fire");
        }

        public sealed class SubA : IEntityEventSubscriber
        {
        }

        public sealed class SubB : IEntityEventSubscriber
        {
        }

        public sealed class SubC : IEntityEventSubscriber
        {
        }
    }

    internal class TestEventSubscriber : IEntityEventSubscriber { }

    internal class TestEventArgs : EntityEventArgs { }
    internal class TestEventTwoArgs : EntityEventArgs { }
}
