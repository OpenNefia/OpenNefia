using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;
using OpenNefia.Core.Stats;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(StatSerializer<>))]
    public class StatSerializerTest : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var stat = new Stat<int>(1, 42);
            var node = Serialization.WriteValueAs<MappingDataNode>(stat);

            Assert.That(node.Cast<ValueDataNode>("base").Value, Is.EqualTo("1"));
            Assert.That(node.Cast<ValueDataNode>("buffed").Value, Is.EqualTo("42"));
        }

        [Test]
        public void DeserializationTest()
        {
            var stat = new Stat<int>(1, 42);
            var node = new MappingDataNode();

            node.Add("base", new ValueDataNode("1"));
            node.Add("buffed", new ValueDataNode("42"));

            var deserializedStat = Serialization.ReadValue<Stat<int>>(node);

            Assert.That(deserializedStat, Is.EqualTo(stat));
        }
    }
}
