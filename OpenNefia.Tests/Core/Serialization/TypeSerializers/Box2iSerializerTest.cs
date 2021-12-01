using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(UIBox2iFromDimensionsSerializer))]
    public class UIBox2iFromDimensionsSerializerTest : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var left = 1;
            var top = 2;
            var width = 3;
            var height = 4;
            var str = $"{left},{top},{width},{height}";
            var box = UIBox2i.FromDimensions(left, top, width, height);
            var node = Serialization.WriteValueAs<ValueDataNode>(box);

            Assert.That(node.Value, Is.EqualTo(str));
        }

        [Test]
        public void DeserializationTest()
        {
            var left = 1;
            var top = 2;
            var width = 3;
            var height = 4;
            var str = $"{left},{top},{width},{height}";
            var node = new ValueDataNode(str);
            var deserializedBox = Serialization.ReadValueOrThrow<UIBox2i>(node);
            var box = UIBox2i.FromDimensions(left, top, width, height);

            Assert.That(deserializedBox, Is.EqualTo(box));

            Assert.That(deserializedBox.Left, Is.EqualTo(box.Left));
            Assert.That(deserializedBox.Bottom, Is.EqualTo(box.Bottom));
            Assert.That(deserializedBox.Right, Is.EqualTo(box.Right));
            Assert.That(deserializedBox.Top, Is.EqualTo(box.Top));

            Assert.That(deserializedBox.BottomLeft, Is.EqualTo(box.BottomLeft));
            Assert.That(deserializedBox.BottomRight, Is.EqualTo(box.BottomRight));
            Assert.That(deserializedBox.TopLeft, Is.EqualTo(box.TopLeft));
            Assert.That(deserializedBox.TopRight, Is.EqualTo(box.TopRight));
        }
    }
}
