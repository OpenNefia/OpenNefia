using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Generic;
using OpenNefia.Core.Stats;
using OpenNefia.Tests.Core.Serialization;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Content.Tests.GameObjects.Components
{
    [TestFixture]
    public class LevelAndPotentialSerializer_Tests : ContentSerializationTest
    {
        [Test]
        public void SerializationTest_Mapping()
        {
            var level = new LevelAndPotential()
            {
                Level = new(42),
                Potential = 150,
                Experience = 500
            };
            var node = Serialization.WriteValueAs<MappingDataNode>(level);

            Assert.That(node.Cast<ValueDataNode>("level").Value, Is.EqualTo("42"));
            Assert.That(node.Cast<ValueDataNode>("potential").Value, Is.EqualTo("150"));
            Assert.That(node.Cast<ValueDataNode>("experience").Value, Is.EqualTo("500"));
        }

        // TODO: WriteValueAs is just a naive cast; you can't have more than one ITypeWriter<T>
        // for the same target type but different data node types at the moment.
        /*
        [Test]
        public void SerializationTest_Value()
        {
            var level = new LevelAndPotential();
            var node = Serialization.WriteValueAs<ValueDataNode>(level);

            Assert.That(node.Value, Is.EqualTo("1"));
        }
        */

        [Test]
        public void DeserializationTest_Mapping()
        {
            var level = new LevelAndPotential()
            {
                Level = new(42),
                Potential = 150,
                Experience = 500
            };
            var node = new MappingDataNode();
            node.Add("level", "42");
            node.Add("potential", "150");
            node.Add("experience", "500");

            var deserializedLevel = Serialization.ReadValue<LevelAndPotential>(node);

            Assert.That(deserializedLevel, Is.EqualTo(level));
        }

        [Test]
        public void DeserializationTest_Value()
        {
            var version = new LevelAndPotential()
            {
                Level = new(42),
                Potential = 100,
                Experience = 0
            };
            var node = new ValueDataNode("42");

            var deserializedLevel = Serialization.ReadValue<LevelAndPotential>(node);

            Assert.That(deserializedLevel, Is.EqualTo(version));
        }
    }
}
