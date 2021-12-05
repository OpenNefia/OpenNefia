using NUnit.Framework;
using OpenNefia.Core.Serialization.Instanced;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Serialization.Instanced
{
    [TestFixture]
    [TestOf(typeof(InstancedSerializer))]
    public class InstancedSerializer_Tests : OpenNefiaUnitTest
    {
        public class DummyClass1
        {
            public int BareField;
        
            public int BareProperty { get; set; }

            public int ReadonlyProperty { get; }

            public int DynamicProperty { get => BareField; set=> BareField = value; }
        }

        [Test]
        public void TestInstancedSerializerPolicy()
        {

        }
    }
}
