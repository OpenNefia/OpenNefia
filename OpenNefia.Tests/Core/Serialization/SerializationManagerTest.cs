using NUnit.Framework;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Tests.Core.Serialization
{
    [TestFixture]
    [TestOf(typeof(SerializationManager))]
    public sealed class SerializationManagerTest : SerializationTest
    {
        [Test]
        public void TestCanSerializeType()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Serialization.CanSerializeType(typeof(string)), Is.True, "Builtin");
                Assert.That(Serialization.CanSerializeType(typeof(string[])), Is.True, "Array");
                Assert.That(Serialization.CanSerializeType(typeof(EntityCoordinates)), Is.True, "Custom");
                Assert.That(Serialization.CanSerializeType(typeof(List<string>)), Is.True, "Generic (builtin)");
                Assert.That(Serialization.CanSerializeType(typeof(List<EntityCoordinates>)), Is.True, "Generic (custom)");

                Assert.That(Serialization.CanSerializeType(typeof(CancellationToken[])), Is.False, "Array (no serializer)");
                Assert.That(Serialization.CanSerializeType(typeof(List<CancellationToken>)), Is.True, "Generic (no serializer)");
            });
        }
    }
}