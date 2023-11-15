using System.IO;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using YamlDotNet.RepresentationModel;
using static OpenNefia.Core.Prototypes.EntityPrototype;
// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers
{
    [TestFixture]
    [TestFixture]
    [TestOf(typeof(ComponentRegistrySerializer))]
    public sealed class ComponentRegistrySerializerTest : SerializationTest
    {
        [OneTimeSetUp]
        public new void OneTimeSetup()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();
            componentFactory.RegisterClass<TestComponent>();
        }

        [Test]
        public void SerializationTest()
        {
            var component = new TestComponent();
            var registry = new ComponentRegistry { { "Test", new ComponentRegistryEntry(component, new MappingDataNode()) } };
            var node = Serialization.WriteValueAs<SequenceDataNode>(registry);

            Assert.That(node.Sequence.Count, Is.EqualTo(1));
            Assert.IsInstanceOf<MappingDataNode>(node[0]);

            var mapping = node.Cast<MappingDataNode>(0);
            Assert.That(mapping.Cast<ValueDataNode>("type").Value, Is.EqualTo("Test"));
        }

        [Test]
        public void DeserializationTest()
        {
            var str = "- type: Test";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(str));

            var mapping = yamlStream.Documents[0].RootNode.ToDataNodeCast<SequenceDataNode>();

            var deserializedRegistry = Serialization.Read<ComponentRegistry>(mapping);

            Assert.That(deserializedRegistry.Count, Is.EqualTo(1));
            Assert.That(deserializedRegistry.ContainsKey("Test"));
            Assert.IsInstanceOf<TestComponent>(deserializedRegistry["Test"].Component);
        }
    }

    public sealed class TestComponent : Component
    {
    }
}
