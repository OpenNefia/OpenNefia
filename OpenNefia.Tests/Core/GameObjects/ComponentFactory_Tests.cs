using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture]
    [TestOf(typeof(ComponentFactory))]
    public class ComponentFactory_Tests : OpenNefiaUnitTest
    {
        private const string TestComponentName = "A";
        private const string LowercaseTestComponentName = "a";
        private const string NonexistentComponentName = "B";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            IoCManager.Resolve<IComponentFactory>().RegisterClass<TestComponent>();
        }

        [Test]
        public void GetComponentAvailabilityTest()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();

            // Should not exist
            Assert.False(componentFactory.IsRegistered(NonexistentComponentName));

            // Normal casing, do not ignore case, should exist
            Assert.True(componentFactory.IsRegistered(TestComponentName));

            // Lower casing, do not ignore case, should not exist
            Assert.False(componentFactory.IsRegistered(LowercaseTestComponentName));
        }

        [Test]
        public void GetComponentTest()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();

            // Should not exist
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetComponent(NonexistentComponentName));
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetComponent(NonexistentComponentName, true));

            // Normal casing, do not ignore case, should exist
            Assert.IsInstanceOf<TestComponent>(componentFactory.GetComponent(TestComponentName));

            // Normal casing, ignore case, should exist
            Assert.IsInstanceOf<TestComponent>(componentFactory.GetComponent(TestComponentName, true));

            // Lower casing, do not ignore case, should not exist
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetComponent(LowercaseTestComponentName));

            // Lower casing, ignore case, should exist
            Assert.IsInstanceOf<TestComponent>(componentFactory.GetComponent(LowercaseTestComponentName, true));
        }

        [Test]
        public void GetRegistrationTest()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();

            // Should not exist
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetRegistration(NonexistentComponentName));
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetRegistration(NonexistentComponentName, true));

            // Normal casing, do not ignore case, should exist
            Assert.DoesNotThrow(() => componentFactory.GetRegistration(TestComponentName));

            // Normal casing, ignore case, should exist
            Assert.DoesNotThrow(() => componentFactory.GetRegistration(TestComponentName, true));

            // Lower casing, do not ignore case, should not exist
            Assert.Throws<UnknownComponentException>(() => componentFactory.GetRegistration(LowercaseTestComponentName));

            // Lower casing, ignore case, should exist
            Assert.DoesNotThrow(() => componentFactory.GetRegistration(LowercaseTestComponentName, true));
        }

        [Test]
        public void TryGetRegistrationTest()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();

            // Should not exist
            Assert.False(componentFactory.TryGetRegistration(NonexistentComponentName, out _));
            Assert.False(componentFactory.TryGetRegistration(NonexistentComponentName, out _, true));

            // Normal casing, do not ignore case, should exist
            Assert.True(componentFactory.TryGetRegistration(TestComponentName, out _));

            // Normal casing, ignore case, should exist
            Assert.True(componentFactory.TryGetRegistration(TestComponentName, out _, true));

            // Lower casing, do not ignore case, should not exist
            Assert.False(componentFactory.TryGetRegistration(LowercaseTestComponentName, out _));

            // Lower casing, ignore case, should exist
            Assert.True(componentFactory.TryGetRegistration(LowercaseTestComponentName, out _, true));
        }

        private class TestComponent : Component
        {
            public override string Name => TestComponentName;
        }
    }
}
