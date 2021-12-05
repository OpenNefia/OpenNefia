using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Definition;

namespace OpenNefia.Tests.Core.Serialization
{
    [TestFixture]
    [TestOf(typeof(DataDefinition))]
    public class InheritanceSerializationTest : OpenNefiaUnitTest
    {
        private const string BaseEntityId = "BaseEntity";
        private const string InheritorEntityId = "InheritorEntityId";
        private const string FinalEntityId = "FinalEntityId";

        private const string BaseComponentFieldValue = "BaseFieldValue";
        private const string InheritorComponentFieldValue = "InheritorFieldValue";
        private const string FinalComponentFieldValue = "FinalFieldValue";

        private static readonly string Prototypes = $@"
- type: Entity
  id: {BaseEntityId}
  components:
  - type: TestBase
    baseField: {BaseComponentFieldValue}

- type: Entity
  id: {InheritorEntityId}
  components:
  - type: TestInheritor
    baseField: {BaseComponentFieldValue}
    inheritorField: {InheritorComponentFieldValue}

- type: Entity
  id: {FinalEntityId}
  components:
  - type: TestFinal
    baseField: {BaseComponentFieldValue}
    inheritorField: {InheritorComponentFieldValue}
    finalField: {FinalComponentFieldValue}";

        [Test]
        public void Test()
        {
            var componentFactory = IoCManager.Resolve<IComponentFactory>();

            componentFactory.RegisterClass<BaseComponent>();
            componentFactory.RegisterClass<InheritorComponent>();
            componentFactory.RegisterClass<FinalComponent>();
            componentFactory.FinishRegistration();

            var serializationManager = IoCManager.Resolve<ISerializationManager>();
            serializationManager.Initialize();

            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            prototypeManager.RegisterType(typeof(EntityPrototype));
            prototypeManager.LoadString(Prototypes);

            var entityManager = IoCManager.Resolve<IEntityManager>();

            var mapManager = IoCManager.Resolve<IMapManager>();

            var map = mapManager.CreateMap(null, 20, 20);

            var coordinates = map.AtPos(Vector2i.Zero);

            var baseEntity = entityManager.SpawnEntity(BaseEntityId, coordinates);

            Assert.That(baseEntity.TryGetComponent(out BaseComponent? baseComponent));
            Assert.That(baseComponent!.BaseField, Is.EqualTo(BaseComponentFieldValue));

            var inheritorEntity = entityManager.SpawnEntity(InheritorEntityId, coordinates);

            Assert.That(inheritorEntity.TryGetComponent(out InheritorComponent? inheritorComponent));
            Assert.That(inheritorComponent!.BaseField, Is.EqualTo(BaseComponentFieldValue));
            Assert.That(inheritorComponent!.InheritorField, Is.EqualTo(InheritorComponentFieldValue));

            var finalEntity = entityManager.SpawnEntity(FinalEntityId, coordinates);

            Assert.That(finalEntity.TryGetComponent(out FinalComponent? finalComponent));
            Assert.That(finalComponent!.BaseField, Is.EqualTo(BaseComponentFieldValue));
            Assert.That(finalComponent!.InheritorField, Is.EqualTo(InheritorComponentFieldValue));
            Assert.That(finalComponent!.FinalField, Is.EqualTo(FinalComponentFieldValue));
        }
    }

    public class BaseComponent : Component
    {
        public override string Name => "TestBase";

        [DataField("baseField")] public string? BaseField;
    }

    public class InheritorComponent : BaseComponent
    {
        public override string Name => "TestInheritor";

        [DataField("inheritorField")] public string? InheritorField;
    }

    public class FinalComponent : InheritorComponent
    {
        public override string Name => "TestFinal";

        [DataField("finalField")] public string? FinalField;
    }
}
