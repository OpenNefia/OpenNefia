using NUnit.Framework;

namespace OpenNefia.Tests.Core.Serialization
{
    [TestFixture]
    [NonParallelizable]
    public class SerializationShutdownTest : SerializationTest
    {
        [Test]
        public void SerializationInitializeShutdownInitializeTest()
        {
            Assert.DoesNotThrow(() =>
            {
                // First initialize is done in the parent class
                Serialization.Shutdown();
                Serialization.Initialize();
            });
        }
    }
}
