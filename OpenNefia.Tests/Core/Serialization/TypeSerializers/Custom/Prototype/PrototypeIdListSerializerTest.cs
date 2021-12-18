using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using YamlDotNet.RepresentationModel;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers.Custom.Prototype
{
    [TestFixture]
    [TestOf(typeof(PrototypeIdListSerializer<>))]
    public class PrototypeIdListSerializerTest : SerializationTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntityId = new($"{nameof(PrototypeIdListSerializerTest)}Dummy");

        private static readonly PrototypeId<EntityPrototype> TestInvalidEntityId = new($"{nameof(PrototypeIdListSerializerTest)}DummyInvalid");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntityId}";

        private static readonly string DataString = $@"
entitiesList:
- {TestEntityId}
entitiesReadOnlyList:
- {TestEntityId}
entitiesReadOnlyCollection:
- {TestEntityId}
entitiesImmutableList:
- {TestEntityId}";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var protoMan = IoCManager.Resolve<IPrototypeManager>();

            protoMan.RegisterType<EntityPrototype>();
            protoMan.LoadString(Prototypes);
            protoMan.Resync();
        }

        [Test]
        public void SerializationTest()
        {
            var definition = new PrototypeIdListSerializerTestDataDefinition
            {
                EntitiesList = {TestEntityId},
                EntitiesReadOnlyList = new List<PrototypeId<EntityPrototype>>() {TestEntityId},
                EntitiesReadOnlyCollection = new List<PrototypeId<EntityPrototype>>() {TestEntityId},
                EntitiesImmutableList = ImmutableList.Create(TestEntityId)
            };
            var node = Serialization.WriteValueAs<MappingDataNode>(definition);

            Assert.That(node.Children.Count, Is.EqualTo(4));

            var entities = node.Cast<SequenceDataNode>("entitiesList");
            Assert.That(entities.Sequence.Count, Is.EqualTo(1));
            Assert.That(entities.Cast<ValueDataNode>(0).Value, Is.EqualTo(TestEntityId.ToString()));

            var entitiesReadOnlyList = node.Cast<SequenceDataNode>("entitiesReadOnlyList");
            Assert.That(entitiesReadOnlyList.Sequence.Count, Is.EqualTo(1));
            Assert.That(entitiesReadOnlyList.Cast<ValueDataNode>(0).Value, Is.EqualTo(TestEntityId.ToString()));

            var entitiesReadOnlyCollection = node.Cast<SequenceDataNode>("entitiesReadOnlyCollection");
            Assert.That(entitiesReadOnlyCollection.Sequence.Count, Is.EqualTo(1));
            Assert.That(entitiesReadOnlyCollection.Cast<ValueDataNode>(0).Value, Is.EqualTo(TestEntityId.ToString()));

            var entitiesImmutableList = node.Cast<SequenceDataNode>("entitiesImmutableList");
            Assert.That(entitiesImmutableList.Sequence.Count, Is.EqualTo(1));
            Assert.That(entitiesImmutableList.Cast<ValueDataNode>(0).Value, Is.EqualTo(TestEntityId.ToString()));
        }

        [Test]
        public void DeserializationTest()
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(DataString));

            var node = stream.Documents[0].RootNode.ToDataNode();
            var definition = Serialization.ReadValue<PrototypeIdListSerializerTestDataDefinition>(node);

            Assert.NotNull(definition);

            Assert.That(definition!.EntitiesList.Count, Is.EqualTo(1));
            Assert.That(definition.EntitiesList[0], Is.EqualTo(TestEntityId));

            Assert.That(definition!.EntitiesReadOnlyList.Count, Is.EqualTo(1));
            Assert.That(definition.EntitiesReadOnlyList[0], Is.EqualTo(TestEntityId));

            Assert.That(definition!.EntitiesReadOnlyCollection.Count, Is.EqualTo(1));
            Assert.That(definition.EntitiesReadOnlyCollection.Single(), Is.EqualTo(TestEntityId));

            Assert.That(definition!.EntitiesImmutableList.Count, Is.EqualTo(1));
            Assert.That(definition.EntitiesImmutableList[0], Is.EqualTo(TestEntityId));
        }

        [Test]
        public void ValidationValidTest()
        {
            var validSequence = new SequenceDataNode(TestEntityId.ToString());

            var validations = Serialization.ValidateNodeWith<
                List<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(validSequence);
            Assert.True(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                IReadOnlyList<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(validSequence);
            Assert.True(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                IReadOnlyCollection<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(validSequence);
            Assert.True(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                ImmutableList<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(validSequence);
            Assert.True(validations.Valid);
        }

        [Test]
        public void ValidationInvalidTest()
        {
            var invalidSequence = new SequenceDataNode(TestInvalidEntityId.ToString());

            var validations = Serialization.ValidateNodeWith<
                List<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(invalidSequence);
            Assert.False(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                IReadOnlyList<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(invalidSequence);
            Assert.False(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                IReadOnlyCollection<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(invalidSequence);
            Assert.False(validations.Valid);

            validations = Serialization.ValidateNodeWith<
                ImmutableList<PrototypeId<EntityPrototype>>,
                PrototypeIdListSerializer<EntityPrototype>,
                SequenceDataNode>(invalidSequence);
            Assert.False(validations.Valid);
        }
    }

    [DataDefinition]
    public class PrototypeIdListSerializerTestDataDefinition
    {
        [DataField("entitiesList", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public List<PrototypeId<EntityPrototype>> EntitiesList = new();

        [DataField("entitiesReadOnlyList", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public IReadOnlyList<PrototypeId<EntityPrototype>> EntitiesReadOnlyList = new List<PrototypeId<EntityPrototype>>();

        [DataField("entitiesReadOnlyCollection", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public IReadOnlyCollection<PrototypeId<EntityPrototype>> EntitiesReadOnlyCollection = new List<PrototypeId<EntityPrototype>>();

        [DataField("entitiesImmutableList", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public ImmutableList<PrototypeId<EntityPrototype>> EntitiesImmutableList = ImmutableList<PrototypeId<EntityPrototype>>.Empty;
    }
}
