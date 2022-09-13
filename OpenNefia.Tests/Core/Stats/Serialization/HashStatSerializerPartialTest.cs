using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Stats;
using OpenNefia.Core.Stats.Serialization;
using OpenNefia.Tests.Core.Serialization;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Stats.Serialization
{
    [TestFixture]
    [TestOf(typeof(HashStatSerializerPartial<>))]
    public class HashStatSerializerPartialTest : SerializationTest
    {
        [Test]
        public void SerializationTest_Defaults()
        {
            var stat = new HashSetStat<int>(new() { 1, 2 }, new() { 42 });
            var node = Serialization.WriteValueAs<SequenceDataNode>(stat);

            Assert.That(node.Cast<ValueDataNode>(0).Value, Is.EqualTo("1"));
            Assert.That(node.Cast<ValueDataNode>(1).Value, Is.EqualTo("2"));
        }

        [Test]
        public void DeserializationTest_Sequence()
        {
            var stat = new HashSetStat<int>(new() { 1, 2 });
            var node = new SequenceDataNode(new List<string>() { "1", "2" });

            var deserializedStat = Serialization.Read<HashSetStat<int>>(node)!;

            Assert.That(deserializedStat.Base, Is.EquivalentTo(stat.Base));
            Assert.That(deserializedStat.Buffed, Is.EquivalentTo(stat.Buffed));
        }

        [Test]
        public void DeserializationTest_Mapping()
        {
            var stat = new HashSetStat<int>(new() { 1, 2 }, new() { 34, 56 });
            var node = new MappingDataNode();

            node.Add("base", new SequenceDataNode(new List<string>() { "1", "2" }));
            node.Add("buffed", new SequenceDataNode(new List<string>() { "34", "56" }));

            var deserializedStat = Serialization.Read<HashSetStat<int>>(node)!;

            Assert.That(deserializedStat.Base, Is.EquivalentTo(stat.Base));
            Assert.That(deserializedStat.Buffed, Is.EquivalentTo(stat.Buffed));
        }
    }
}
