using System.Collections.Generic;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Tests.Core.Prototypes
{
    [TestFixture]
    public sealed class HotReloadTest : OpenNefiaUnitTest
    {
        private const string DummyId = "Dummy";
        public const string HotReloadTestComponentOneId = "HotReloadTestOne";
        public const string HotReloadTestComponentTwoId = "HotReloadTestTwo";

        private static readonly string InitialPrototypes = $@"
- type: Entity
  id: {DummyId}
  components:
  - type: {HotReloadTestComponentOneId}
    value: 5";

        private static readonly string ReloadedPrototypes = $@"
- type: Entity
  id: {DummyId}
  components:
  - type: {HotReloadTestComponentOneId}
    value: 10
  - type: {HotReloadTestComponentTwoId}";

        private IComponentFactory _components = default!;
        private PrototypeManager _prototypes = default!;
        private IMapManager _maps = default!;
        private IEntityManager _entities = default!;

        [OneTimeSetUp]
        public void Setup()
        {
            _components = IoCManager.Resolve<IComponentFactory>();
            _components.RegisterClass<HotReloadTestOneComponent>();
            _components.RegisterClass<HotReloadTestTwoComponent>();

            IoCManager.Resolve<ISerializationManager>().Initialize();
            _prototypes = (PrototypeManager)IoCManager.Resolve<IPrototypeManager>();
            _prototypes.RegisterType(typeof(EntityPrototype));
            _prototypes.LoadString(InitialPrototypes);
            _prototypes.ResolveResults();

            _maps = IoCManager.Resolve<IMapManager>();
            _entities = IoCManager.Resolve<IEntityManager>();
        }

        [Test]
        public void TestHotReload()
        {
            var map = _maps.CreateMap(25, 25);
            var entity = _entities.SpawnEntity(new(DummyId), map.AtPos(Vector2i.Zero));
            var entityComponent = _entities.GetComponent<HotReloadTestOneComponent>(entity);

            Assert.That(entityComponent.Value, Is.EqualTo(5));
            Assert.False(_entities.HasComponent<HotReloadTestTwoComponent>(entity));

            var reloaded = false;
            _prototypes.PrototypesReloaded += _ => reloaded = true;

            _prototypes.ReloadPrototypes(new Dictionary<Type, HashSet<string>>());

            Assert.True(reloaded);
            reloaded = false;

            Assert.That(entityComponent.Value, Is.EqualTo(5));
            Assert.False(_entities.HasComponent<HotReloadTestTwoComponent>(entity));

            var changedPrototypes = new Dictionary<Type, HashSet<string>>();
            _prototypes.LoadString(ReloadedPrototypes, true, changedPrototypes);
            _prototypes.ReloadPrototypes(changedPrototypes);

            Assert.True(reloaded);
            reloaded = false;

            // Existing component values are not modified in the current implementation
            Assert.That(entityComponent.Value, Is.EqualTo(5));

            // New components are added
            Assert.True(_entities.HasComponent<HotReloadTestTwoComponent>(entity));

            changedPrototypes = new Dictionary<Type, HashSet<string>>();
            _prototypes.LoadString(InitialPrototypes, true, changedPrototypes);
            _prototypes.ReloadPrototypes(changedPrototypes);

            Assert.True(reloaded);
            reloaded = false;

            // Existing component values are not modified in the current implementation
            Assert.That(entityComponent.Value, Is.EqualTo(5));

            // Old components are removed
            Assert.False(_entities.HasComponent<HotReloadTestTwoComponent>(entity));
        }
    }

    [ComponentProtoName(HotReloadTest.HotReloadTestComponentOneId)]
    public class HotReloadTestOneComponent : Component
    {
        [DataField("value")]
        public int Value { get; }
    }

    [ComponentProtoName(HotReloadTest.HotReloadTestComponentTwoId)]
    public class HotReloadTestTwoComponent : Component
    {
    }
}