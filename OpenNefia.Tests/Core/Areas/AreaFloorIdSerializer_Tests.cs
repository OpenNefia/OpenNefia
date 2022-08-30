using NUnit.Framework;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(AreaFloorIdSerializer))]
    public class AreaFloorIdSerializer_Tests : SerializationTest
    {
        [Test]
        public void SerializationTest()
        {
            var range = new AreaFloorId("TestAreaFloor", 4567);
            var node = Serialization.WriteValueAs<ValueDataNode>(range);
            var serializedValue = $"TestAreaFloor:4567";

            Assert.That(node.Value, Is.EqualTo(serializedValue));
        }
        
        [Test]
        public void SerializationTest_Colon()
        {
            var range = new AreaFloorId("TestAreaFloor:Dood", 4567);
            var node = Serialization.WriteValueAs<ValueDataNode>(range);
            var serializedValue = $"TestAreaFloor:Dood:4567";

            Assert.That(node.Value, Is.EqualTo(serializedValue));
        }

        [Test]
        public void DeserializationTest()
        {
            var floorId = new AreaFloorId("TestAreaFloor", 4567);
            var node = new ValueDataNode($"{floorId.ID}:{floorId.FloorNumber}");
            var deserializedRange = Serialization.ReadValue<AreaFloorId>(node);

            Assert.That(deserializedRange, Is.EqualTo(floorId));
        }

        [Test]
        public void DeserializationTest_Colon()
        {
            var floorId = new AreaFloorId("TestAreaFloor:Dood", 4567);
            var node = new ValueDataNode($"{floorId.ID}:{floorId.FloorNumber}");
            var deserializedRange = Serialization.ReadValue<AreaFloorId>(node);

            Assert.That(deserializedRange, Is.EqualTo(floorId));
        }
    }
}