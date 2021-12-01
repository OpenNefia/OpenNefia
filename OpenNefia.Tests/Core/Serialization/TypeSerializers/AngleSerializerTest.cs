using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(AngleSerializer))]
    public class AngleSerializerTest : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var degrees = 75d;
            var angle = Angle.FromDegrees(degrees);
            var node = Serialization.WriteValueAs<ValueDataNode>(angle);
            var serializedValue = $"{MathHelper.DegreesToRadians(degrees)} rad";

            Assert.That(node.Value, Is.EqualTo(serializedValue));
        }

        [Test]
        public void DeserializationTest()
        {
            var degrees = 75;
            var node = new ValueDataNode(degrees.ToString());
            var deserializedAngle = Serialization.ReadValue<Angle>(node);
            var angle = Angle.FromDegrees(degrees);

            Assert.That(deserializedAngle, Is.EqualTo(angle));
        }
    }
}
