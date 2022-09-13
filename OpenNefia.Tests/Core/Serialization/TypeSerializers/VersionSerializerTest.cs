using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;
using OpenNefia.Core.Stats;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(VersionSerializer))]
    public class VersionSerializerTest : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var version = new Version(1, 2, 3, 4);
            var node = Serialization.WriteValueAs<ValueDataNode>(version);

            Assert.That(node.Value, Is.EqualTo("1.2.3.4"));
        }

        [Test]
        public void DeserializationTest()
        {
            var version = new Version(1, 2, 3, 4);
            var node = new ValueDataNode("1.2.3.4");

            var deserializedVersion = Serialization.Read<Version>(node);

            Assert.That(deserializedVersion, Is.EqualTo(version));
        }
    }
}
