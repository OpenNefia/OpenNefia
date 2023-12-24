using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(EnumRangeSerializer<>))]
    public class EnumRangeSerializer_Tests : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var range = new EnumRange<TestRangeEnum>(TestRangeEnum.Bar, TestRangeEnum.Hoge);
            var node = Serialization.WriteValueAs<ValueDataNode>(range);
            var serializedValue = $"{range.Min}~{range.Max}";

            Assert.That(node.Value, Is.EqualTo(serializedValue));
        }

        [Test]
        public void DeserializationTest()
        {
            var range = new EnumRange<TestRangeEnum>(TestRangeEnum.Bar, TestRangeEnum.Hoge);
            var node = new ValueDataNode($"Bar~Hoge");
            var deserializedRange = Serialization.Read<EnumRange<TestRangeEnum>>(node);

            Assert.That(deserializedRange, Is.EqualTo(range));
        }

        [Test]
        public void DeserializationTest_Underscore()
        {
            var range = new EnumRange<TestRangeEnum>(TestRangeEnum._Foo, TestRangeEnum._Piyo);
            var node = new ValueDataNode($"_Foo~_Piyo");
            var deserializedRange = Serialization.Read<EnumRange<TestRangeEnum>>(node);

            Assert.That(deserializedRange, Is.EqualTo(range));
        }

        [Test]
        public void DeserializationTest_Reverse()
        {
            var range = new EnumRange<TestRangeEnum>(TestRangeEnum._Piyo, TestRangeEnum._Foo);
            var node = new ValueDataNode($"_Piyo~_Foo");
            var deserializedRange = Serialization.Read<EnumRange<TestRangeEnum>>(node);

            Assert.That(deserializedRange, Is.EqualTo(range));
        }

        [Test]
        public void DeserializationTest_CaseSensitivity()
        {
            var node = new ValueDataNode($"bar~hoge");

            Assert.Throws<InvalidMappingException>(() =>
            {
                Serialization.Read<EnumRange<TestRangeEnum>>(node);
            });
        }
    }

    public enum TestRangeEnum
    {
        _Foo,
        Bar,
        Baz,
        Hoge,
        _Piyo
    }
}