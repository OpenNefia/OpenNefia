using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
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
        public void SerializationTest_Sequence()
        {
            var list = new[] { "A", "E" };
            var node = Serialization.WriteValueAs<SequenceDataNode>(list);

            Assert.That(node.Cast<ValueDataNode>(0).Value, Is.EqualTo("A"));
            Assert.That(node.Cast<ValueDataNode>(1).Value, Is.EqualTo("E"));
        }

        [Test]
        public void DeserializationTest_Sequence()
        {
            var list = new[] { "A", "E" };
            var node = new SequenceDataNode("A", "E");
            var deserializedList = Serialization.ReadValue<string[]>(node);

            Assert.That(deserializedList, Is.EqualTo(list));
        }

        [Test]
        public void SerializationTest_MultiDim()
        {
            var list = new[,] { { "A", "B", "C" }, { "E", "F", "G" } };
            var node = Serialization.WriteValueAs<MappingDataNode>(list);

            var lengths = (SequenceDataNode)node["lengths"];
            var elements = (SequenceDataNode)node["elements"];

            Assert.That(lengths.Cast<ValueDataNode>(0).Value, Is.EqualTo("2"));
            Assert.That(lengths.Cast<ValueDataNode>(1).Value, Is.EqualTo("3"));
            Assert.That(elements.Cast<ValueDataNode>(0).Value, Is.EqualTo("A"));
            Assert.That(elements.Cast<ValueDataNode>(1).Value, Is.EqualTo("B"));
            Assert.That(elements.Cast<ValueDataNode>(2).Value, Is.EqualTo("C"));
            Assert.That(elements.Cast<ValueDataNode>(3).Value, Is.EqualTo("E"));
            Assert.That(elements.Cast<ValueDataNode>(4).Value, Is.EqualTo("F"));
            Assert.That(elements.Cast<ValueDataNode>(5).Value, Is.EqualTo("G"));
        }

        [Test]
        public void DeserializationTest_MultiDim()
        {
            var list = new[,] { { "A", "B", "C" }, { "E", "F", "G" } };
            var elementsNode = new SequenceDataNode("A", "B", "C", "E", "F", "G");
            var lengthsNode = new SequenceDataNode("2", "3");
            var node = new MappingDataNode(new Dictionary<DataNode, DataNode> ()
            {
                { new ValueDataNode("lengths"), lengthsNode },
                { new ValueDataNode("elements"), elementsNode }
            });
            var deserializedList = Serialization.ReadValue<string[,]>(node);

            Assert.That(deserializedList, Is.EqualTo(list));
        }
    }
}
