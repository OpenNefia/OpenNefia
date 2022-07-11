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
            Assert.That(Serialization.CanSerializeType(typeof(string)), Is.True);
            Assert.That(Serialization.CanSerializeType(typeof(EntityCoordinates)), Is.True);
            Assert.That(Serialization.CanSerializeType(typeof(List<string>)), Is.True);
        }
    }
}