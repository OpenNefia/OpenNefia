using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(IntRangeSerializer))]
    public class IntRangeSerializer_Tests : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var range = new IntRange(123, 4567);
            var node = Serialization.WriteValueAs<ValueDataNode>(range);
            var serializedValue = $"{range.Min}~{range.Max}";

            Assert.That(node.Value, Is.EqualTo(serializedValue));
        }

        [Test]
        public void DeserializationTest()
        {
            var range = new IntRange(123, 4567);
            var node = new ValueDataNode($"{range.Min}~{range.Max}");
            var deserializedRange = Serialization.Read<IntRange>(node);

            Assert.That(deserializedRange, Is.EqualTo(range));
        }
    }
}