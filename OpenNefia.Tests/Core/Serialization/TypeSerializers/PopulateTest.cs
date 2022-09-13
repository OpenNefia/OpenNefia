using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Value;

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    public class PopulateNullableStructTest : SerializationTest
    {
        [DataDefinition]
        private struct TestStruct : IPopulateDefaultValues
        {
            public bool Populated { get; set; }

            public void PopulateDefaultValues()
            {
                Populated = true;
            }
        }

        [Test]
        public void PopulateStruct()
        {
            var value = Serialization.Read<TestStruct>(new ValueDataNode(string.Empty));

            Assert.True(value.Populated);
        }

        [Test]
        public void PopulateNullableStruct()
        {
            var value = Serialization.Read<TestStruct?>(new ValueDataNode(string.Empty));

            Assert.NotNull(value);
            Assert.True(value.HasValue);
            Assert.True(value!.Value.Populated);
        }
    }
}
