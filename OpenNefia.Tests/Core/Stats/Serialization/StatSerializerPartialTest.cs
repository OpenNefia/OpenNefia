using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Stats;
using OpenNefia.Core.Stats.Serialization;
using OpenNefia.Tests.Core.Serialization;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Stats.Serialization
{
    [TestFixture]
    [TestOf(typeof(StatSerializerPartial<>))]
    public class StatSerializerPartialTest : SerializationTest
    {
        /// <summary>
        /// The default <see cref="Stat{T}"/> serializer will only save the 
        /// base value, to reduce the size of the save. (Stats are meant to 
        /// be transient and recalculable at any time.)
        /// </summary>
        [Test]
        public void SerializationTest_Defaults()
        {
            var stat = new Stat<int>(1, 42);
            var node = Serialization.WriteValueAs<ValueDataNode>(stat);

            Assert.That(node.Value, Is.EqualTo("1"));
        }

        [Test]
        public void DeserializationTest_Value()
        {
            var stat = new Stat<int>(1, 1);
            var node = new ValueDataNode("1");

            var deserializedStat = Serialization.Read<Stat<int>>(node)!;

            Assert.That(deserializedStat.Base, Is.EqualTo(stat.Base));
            Assert.That(deserializedStat.Buffed, Is.EqualTo(stat.Buffed));
        }

        [Test]
        public void DeserializationTest_Mapping()
        {
            var stat = new Stat<int>(1, 42);
            var node = new MappingDataNode();

            node.Add("base", new ValueDataNode("1"));
            node.Add("buffed", new ValueDataNode("42"));

            var deserializedStat = Serialization.Read<Stat<int>>(node)!;

            Assert.That(deserializedStat.Base, Is.EqualTo(stat.Base));
            Assert.That(deserializedStat.Buffed, Is.EqualTo(stat.Buffed));
        }
    }
}
