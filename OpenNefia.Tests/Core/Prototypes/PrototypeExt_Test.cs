using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Prototypes
{
    [TestFixture]
    [TestOf(typeof(PrototypeExt))]
    public class PrototypeExt_Test : OpenNefiaUnitTest
    {
        [Test]
        public void TestGetStrongID_Interfaces()
        {
            TestPrototype proto = new TestPrototype();
            proto.ID = "Test";

            ITestPrototype downcast = proto;
            Assert.That(downcast.GetStrongID(), Is.EqualTo(proto.GetStrongID()));
        }
    }

    public interface ITestPrototype : IPrototype
    {
    }

    [Prototype("Test")]
    public class TestPrototype : ITestPrototype
    {
        [DataField("id")]
        public string ID { get; set; } = default!;
    }
}
