using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Moq;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture]
    [TestOf(typeof(PrototypeEventBus))]
    public class PrototypeEventBus_Test : OpenNefiaUnitTest
    {
        private IPrototypeManager manager = default!;

        private static readonly PrototypeId<EntityPrototype> TestProto1ID = new("TestProto1");

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
            manager = IoCManager.Resolve<IPrototypeManager>();
        }

        [SetUp]
        public void Setup()
        {
            manager.Clear();
            manager.RegisterType<EntityPrototype>();
        }

        [Test]
        public void SubscribeEvent()
        {
            var prototypes = @$"
- type: Entity
  id: {TestProto1ID}
";

            manager.LoadString(prototypes);
            manager.Resync();

            var eventBus = new PrototypeEventBus(manager);

            var a = false;
            var b = false;
            var c = false;

            void HandleTestEventA(EntityPrototype prototype, TestProtoEventArgs ev)
            {
                Assert.That(prototype.GetStrongID(), Is.EqualTo(TestProto1ID));
                Assert.That(ev.TestNumber, Is.EqualTo(5));
                Assert.That(b, Is.True, "A should run after B");
                Assert.That(c, Is.False, "A should run before C");
                a = true;
            }

            void HandleTestEventB(EntityPrototype prototype, TestProtoEventArgs ev)
            {
                Assert.That(a, Is.False, "B should run before A");
                Assert.That(c, Is.False, "B should run before C");
                b = true;
            }

            void HandleTestEventC(EntityPrototype prototype, TestProtoEventArgs ev)
            {
                Assert.That(b, Is.True, "C should run after B");
                c = true;
            }

            eventBus.SubscribeEvent<EntityPrototype, TestProtoEventArgs>((string)TestProto1ID, HandleTestEventA);
            eventBus.SubscribeEvent<EntityPrototype, TestProtoEventArgs>((string)TestProto1ID, HandleTestEventB, priority: EventPriorities.High);
            eventBus.SubscribeEvent<EntityPrototype, TestProtoEventArgs>((string)TestProto1ID, HandleTestEventC, priority: EventPriorities.Low);

            var ev = new TestProtoEventArgs(5);
            eventBus.RaiseEvent(TestProto1ID, ev);

            Assert.That(a, Is.True, "A was not called");
            Assert.That(b, Is.True, "B was not called");
            Assert.That(c, Is.True, "C was not called");
        }

        internal class TestProtoEventArgs : PrototypeEventArgs
        {
            public TestProtoEventArgs(int testNumber)
            {
                TestNumber = testNumber;
            }

            public int TestNumber { get; }
        }
    }
}