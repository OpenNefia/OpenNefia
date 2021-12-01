using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(ListSerializers<>))]
    public class ArraySerializerTest : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var list = new[] {"A", "E"};
            var node = Serialization.WriteValueAs<SequenceDataNode>(list);

            Assert.That(node.Cast<ValueDataNode>(0).Value, Is.EqualTo("A"));
            Assert.That(node.Cast<ValueDataNode>(1).Value, Is.EqualTo("E"));
        }

        [Test]
        public void DeserializationTest()
        {
            var list = new[] {"A", "E"};
            var node = new SequenceDataNode("A", "E");
            var deserializedList = Serialization.ReadValue<string[]>(node);

            Assert.That(deserializedList, Is.EqualTo(list));
        }
    }
}
