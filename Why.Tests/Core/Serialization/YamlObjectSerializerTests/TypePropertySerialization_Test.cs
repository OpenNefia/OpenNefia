using System.IO;
using NUnit.Framework;
using Why.Core.GameObjects;
using Why.Core.IoC;
using Why.Core.Serialization;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Mapping;
using Why.Core.Serialization.Markdown.Value;
using YamlDotNet.RepresentationModel;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace Why.Tests.Shared.Serialization.YamlObjectSerializerTests
{
    [TestFixture]
    public class TypePropertySerialization_Test : WhyUnitTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
        }

        [Test]
        public void SerializeTypePropertiesTest()
        {
            ITestType? type = new TestTypeTwo
            {
                TestPropertyOne = "B",
                TestPropertyTwo = 10
            };
            var serMan = IoCManager.Resolve<ISerializationManager>();
            var mapping = (MappingDataNode)serMan.WriteValue(type);

            Assert.IsNotEmpty(mapping.Children);

            var testPropertyOne = mapping.Get("testPropertyOne") as ValueDataNode;
            var testPropertyTwo = mapping.Get("testPropertyTwo") as ValueDataNode;

            Assert.NotNull(testPropertyOne);
            Assert.NotNull(testPropertyTwo);
            Assert.That(testPropertyOne!.Value, Is.EqualTo("B"));
            Assert.That(testPropertyTwo!.Value, Is.EqualTo("10"));
        }

        [Test]
        public void DeserializeTypePropertiesTest()
        {
            var yaml = @"
- test:
    !type:testtype2
    testPropertyOne: A
    testPropertyTwo: 5
";

            using var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            writer.Write(yaml);
            writer.Flush();
            stream.Position = 0;

            var streamReader = new StreamReader(stream);
            var yamlStream = new YamlStream();
            yamlStream.Load(streamReader);

            var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode[0];

            var serMan = IoCManager.Resolve<ISerializationManager>();
            var type = serMan.ReadValue<ITestType>(mapping["test"].ToDataNode());

            Assert.NotNull(type);
            Assert.IsInstanceOf<TestTypeTwo>(type);

            var testTypeTwo = (TestTypeTwo)type!;

            Assert.That(testTypeTwo.TestPropertyOne, Is.EqualTo("A"));
            Assert.That(testTypeTwo.TestPropertyTwo, Is.EqualTo(5));
        }
    }

    [SerializedType("testtype2")]
    [DataDefinition]
    public class TestTypeTwo : ITestType
    {
        [DataField("testPropertyOne")]
        public string? TestPropertyOne { get; set; }

        [DataField("testPropertyTwo")]
        public int TestPropertyTwo { get; set; }
    }

    [RegisterComponent]
    public class TestComponent : Component
    {
        public override string Name => "Test";

        [DataField("testType")] public ITestType? TestType { get; set; }
    }
}
