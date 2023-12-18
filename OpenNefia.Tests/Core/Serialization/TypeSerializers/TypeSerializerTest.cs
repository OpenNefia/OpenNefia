using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestOf(typeof(TypeSerializer))]
    public class TypeSerializerTest : SerializationTest
    {
        private const string TypeName = "OpenNefia.Tests.Core.Serialization.TypeSerializers.TypeSerializerTest";

        [Test]
        public void SerializationTest()
        {
            var type = typeof(TypeSerializerTest);
            var node = Serialization.WriteValueAs<ValueDataNode>(type);

            Assert.That(node.Value, Is.EqualTo(TypeName));
        }

        [Test]
        public void DeserializationTest()
        {
            var type = typeof(TypeSerializerTest);
            var node = new ValueDataNode(TypeName);
            var deserializedType = Serialization.Read<Type>(node);

            Assert.That(deserializedType, Is.EqualTo(type));
        }

        [Test]
        public void CopyTest()
        {
            var type = typeof(TypeSerializerTest);
            var newType = typeof(object);
            Serialization.Copy(type, ref newType);

            Assert.That(type, Is.EqualTo(type));
        }
    }
}
