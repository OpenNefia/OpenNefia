using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;
using OpenNefia.Core.Stats;
using OpenNefia.Tests.Core.Serialization;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Content.Tests.World
{
    [TestFixture]
    [TestOf(typeof(GameTimeSpanSerializer))]
    public class GameTimeSpanSerializer_Tests : ContentSerializationTest
    {
        private static object[] Cases =
        {
            new object[] { 0, 0, 0, "00:00:00" },
            new object[] { 0, 0, -1, "-00:00:01" },
            new object[] { 0, -1, 0, "-00:01:00" },
            new object[] { -1, 0, 0, "-01:00:00" },
            new object[] { 40, 40, 40, "40:40:40" },
            new object[] { 0, 0, 60, "00:01:00" },
            new object[] { 0, 60, 0, "01:00:00" },
            new object[] { 24, 0, 0, "24:00:00" },
            new object[] { 128, 40, 40, "128:40:40" },
            new object[] { 12800, 40, 40, "12800:40:40" },
            new object[] { 1280000, 40, 40, "1280000:40:40" },
            new object[] { -56, -34, -12, "-56:34:12" },
        };

        [Test]
        [TestCaseSource(nameof(Cases))]
        public void SerializationTest(int hours, int minutes, int seconds, string value)
        {
            var time = new GameTimeSpan(hours, minutes, seconds);
            var node = Serialization.WriteValueAs<ValueDataNode>(time);

            Assert.That(node.Value, Is.EqualTo(value));
        }

        [Test]
        [TestCaseSource(nameof(Cases))]
        public void DeserializationTest(int hours, int minutes, int seconds, string value)
        {
            var time = new GameTimeSpan(hours, minutes, seconds);
            var node = new ValueDataNode(value);

            var deserializedTime = Serialization.ReadValue<GameTimeSpan>(node);

            Assert.That(deserializedTime, Is.EqualTo(time));
        }
    }
}
