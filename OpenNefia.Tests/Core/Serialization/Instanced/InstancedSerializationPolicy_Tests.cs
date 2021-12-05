using NUnit.Framework;
using OdinSerializer;
using OpenNefia.Core.Serialization.Instanced;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Serialization.Instanced
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(InstancedSerializationPolicy))]
    public class InstancedSerializationPolicy_Tests : OpenNefiaUnitTest
    {
        [Serializable]
        public class DummyClass1
        {
            public int BareField;

            public int BareProperty { get; set; }

            public int ReadonlyProperty { get; }

            public int DynamicProperty { get => BareField; set=> BareField = value; }

            [NonSerialized]
            public int NonserializedField;

            public DummyClass1(int readonlyProperty)
            {
                ReadonlyProperty = readonlyProperty;
            }
        }

        [Test]
        public void TestInstancedSerializationPolicyPrimitiveFields()
        {
            var dummy = new DummyClass1(33)
            {
                BareField = 11,
                BareProperty = 22,
                NonserializedField = 44
            };

            var serPolicy = new InstancedSerializationPolicy();
            var serConfig = new SerializationConfig()
            {
                SerializationPolicy = serPolicy
            };

            var bytes = SerializationUtility.SerializeValue(dummy, DataFormat.Binary, 
                new SerializationContext() { Config = serConfig });

            var dummy2 = SerializationUtility.DeserializeValue<DummyClass1>(bytes, DataFormat.Binary,
                new DeserializationContext() { Config = serConfig });

            Assert.That(dummy2.BareField, Is.EqualTo(11));
            Assert.That(dummy2.BareProperty, Is.EqualTo(22));
            Assert.That(dummy2.ReadonlyProperty, Is.EqualTo(33));
            Assert.That(dummy2.DynamicProperty, Is.EqualTo(11));
            Assert.That(dummy2.NonserializedField, Is.EqualTo(0));
        }
    }
}
