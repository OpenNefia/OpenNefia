using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapLoader))]
    public class MapLoader_Tests : OpenNefiaUnitTest
    {

        private const string Prototypes = @"
- type: Entity
  id: MapDeserializeTest
  components:
  - type: MapDeserializeTest
    foo: 1
    bar: 2
  - type: MapDeserializeTestRemove

- type: Entity
  id: MapDeserializeTestOverride
  components:
  - type: MapDeserializeTestOverride
    bar: !type:TestDataProto {}

# Required by the engine.
- type: Tile
  id: Empty
  image:
    filepath: /Default.png
  isSolid: false
  isOpaque: false

- type: Tile
  id: Test.Floor
  image:
    filepath: /Default.png
  isSolid: false
  isOpaque: false
- type: Tile
  id: Test.Wall
  image:
    filepath: /Default.png
  isSolid: true
  isOpaque: true
";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var compFactory = IoCManager.Resolve<IComponentFactory>();
            compFactory.RegisterClass<MapDeserializeTestComponent>();
            compFactory.RegisterClass<MapDeserializeTestAddComponent>();
            compFactory.RegisterClass<MapDeserializeTestRemoveComponent>();
            compFactory.RegisterClass<MapDeserializeTestOverrideComponent>();
            compFactory.FinishRegistration();
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.Initialize(new VirtualWritableDirProvider());
            resourceManager.MountString("/Prototypes/All.yml", Prototypes);

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            protoMan.RegisterType<EntityPrototype>();
            protoMan.RegisterType<TilePrototype>();
            protoMan.LoadDirectory(new ResourcePath("/Prototypes")); 
            protoMan.Resync();

            var tileDefMan = IoCManager.Resolve<ITileDefinitionManagerInternal>();
            tileDefMan.RegisterAll();
        }

        [SetUp]
        public void Setup()
        {
            var mapMan = IoCManager.Resolve<IMapManagerInternal>();
            mapMan.FlushMaps();

            var entMan = IoCManager.Resolve<IEntityManagerInternal>();
            entMan.FlushEntities();
        }

        /// <summary>
        /// A map blueprint must have exactly one entity with a <see cref="MapComponent"/>.
        /// </summary>
        [Test]
        public void TestMapEntityCheckNone()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapData1 = @"meta:
  format: 1
  name: test
  author: ruin
grid: |
  .
tilemap:
  '.': Test.Floor
entities: []
";

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountString("/TestMap1.yml", mapData1);

            var mapLoad = IoCManager.Resolve<IMapLoader>();
            Assert.Throws<InvalidDataException>(() => mapLoad.LoadBlueprint(new ResourcePath("/TestMap1.yml")));
        }

        /// <summary>
        /// A map blueprint must have exactly one entity with a <see cref="MapComponent"/>.
        /// </summary>
        [Test]
        public void TestMapEntityCheck()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapData2 = @"meta:
  format: 1
  name: test
  author: ruin
grid: |
  .
tilemap:
  '.': Test.Floor
entities:
- uid: 0
  components:
  - type: Map
- uid: 1
  components:
  - type: Map
";

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountString("/TestMap2.yml", mapData2);

            var mapLoad = IoCManager.Resolve<IMapLoader>();
            Assert.Throws<InvalidDataException>(() => mapLoad.LoadBlueprint(new ResourcePath("/TestMap2.yml")));
        }

        [Test]
        public void TestDataLoadPriority()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();


            var mapData = @"
meta:
  format: 1
  name: test
  author: ruin
grid: |
  @@@@@@@@
  @......@
  @......@
  @......@
  @......@
  @@@@@@@@
tilemap:
  '@': Test.Wall
  '.': Test.Floor
entities:
- uid: 0
  components:
  - type: Map
- uid: 1
  protoId: MapDeserializeTest
  components:
  - type: MapDeserializeTest
    foo: 3
  - type: Spatial
    parent: 0
    pos: 3,3
";

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountString("/TestMap.yml", mapData);

            var mapLoad = IoCManager.Resolve<IMapLoader>();
            var map = mapLoad.LoadBlueprint(new ResourcePath("/TestMap.yml"));

            Assert.That(map, Is.Not.Null);

            var mapEntitySpatial = entMan.GetComponent<SpatialComponent>(map!.MapEntityUid);

            Assert.That(mapEntitySpatial.ChildCount, Is.EqualTo(1));

            var mapEntityMap = entMan.GetComponent<MapComponent>(map!.MapEntityUid);

            Assert.That(mapEntityMap.Metadata.Name, Is.EqualTo("test"));
            Assert.That(mapEntityMap.Metadata.Author, Is.EqualTo("ruin"));

            var entity = mapEntitySpatial.Children.Single().OwnerUid;
            var c = entMan.GetComponent<MapDeserializeTestComponent>(entity);

            Assert.That(c.Bar, Is.EqualTo(2));
            Assert.That(c.Foo, Is.EqualTo(3));
            Assert.That(c.Baz, Is.EqualTo(-1));

            var entitySpatial = entMan.GetComponent<SpatialComponent>(entity);

            Assert.That(entitySpatial.WorldPosition, Is.EqualTo(new Vector2i(3, 3)));
        }

        /// <summary>
        /// Test that added/removed/changed components are accounted for when loading
        /// with full map serialization.
        /// </summary>
        [Test]
        public void TestMapLoadComponents()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(50, 50);
            var mapId = map.Id;
            var mapEntId = map.MapEntityUid;

            var entity = entMan.SpawnEntity(new("MapDeserializeTest"), map.AtPos(Vector2i.One));
            var entityUid = entity.Uid;

            var c = entMan.GetComponent<MapDeserializeTestComponent>(entity.Uid);
            c.Foo = 999;
            c.Bar = 9999;
            c.Baz = 99999;

            entMan.RemoveComponent<MapDeserializeTestRemoveComponent>(entity.Uid);
            entMan.EnsureComponent<MapDeserializeTestAddComponent>(entity.Uid);

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            Assert.Multiple(() =>
            {
                Assert.That(map.Id, Is.EqualTo(mapId));
                Assert.That(map.MapEntityUid, Is.EqualTo(mapEntId));
                Assert.That(mapMan.MapIsLoaded(map.Id), Is.True);
                Assert.That(mapMan.GetMap(mapId), Is.EqualTo(map));
                Assert.That(entMan.EntityExists(entityUid), Is.True);
            });

            entity = entMan.GetEntity(entityUid);

            Assert.That(entity.Uid, Is.EqualTo(entityUid));

            c = entMan.GetComponent<MapDeserializeTestComponent>(entity.Uid);

            Assert.That(c.Foo, Is.EqualTo(999));
            Assert.That(c.Bar, Is.EqualTo(9999));
            Assert.That(c.Baz, Is.EqualTo(99999));

            Assert.That(entMan.HasComponent<MapDeserializeTestAddComponent>(entity.Uid), Is.True);
            Assert.That(entMan.HasComponent<MapDeserializeTestRemoveComponent>(entity.Uid), Is.False);
        }

        /// <summary>
        /// Tests that the full state of data definition properties are captured on map save
        /// , including defaults set by the prototype/constructors.
        /// </summary>
        [Test]
        public void TestMapLoadDataDefinitions()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(50, 50);
            var mapId = map.Id;

            var entity = entMan.SpawnEntity(new("MapDeserializeTestOverride"), map.AtPos(Vector2i.One));
            var entityUid = entity.Uid;

            var c = entMan.GetComponent<MapDeserializeTestOverrideComponent>(entity.Uid);
            c.Foo = new TestDataRuntime();

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            entity = entMan.GetEntity(entityUid);

            c = entMan.GetComponent<MapDeserializeTestOverrideComponent>(entity.Uid);

            Assert.Multiple(() =>
            {
                Assert.That(c.Foo, Is.TypeOf(typeof(TestDataRuntime)));
                Assert.That(c.Bar, Is.TypeOf(typeof(TestDataProto)));
                Assert.That(c.Baz, Is.TypeOf(typeof(TestDataCtor)));
            });
        }

        [DataDefinition]
        private sealed class MapDeserializeTestComponent : Component
        {
            public override string Name => "MapDeserializeTest";

            [DataField("foo")] public int Foo { get; set; } = -1;
            [DataField("bar")] public int Bar { get; set; } = -1;
            [DataField("baz")] public int Baz { get; set; } = -1;
        }

        [DataDefinition]
        private sealed class MapDeserializeTestAddComponent : Component
        {
            public override string Name => "MapDeserializeTestAdd";
        }

        [DataDefinition]
        private sealed class MapDeserializeTestRemoveComponent : Component
        {
            public override string Name => "MapDeserializeTestRemove";
        }

        [DataDefinition]
        private sealed class MapDeserializeTestOverrideComponent : Component
        {
            public override string Name => "MapDeserializeTestOverride";

            [DataField]
            public ITestData Foo { get; set; } = new TestDataCtor();

            [DataField]
            public ITestData Bar { get; set; } = new TestDataCtor();

            [DataField]
            public ITestData Baz { get; set; } = new TestDataCtor();
        }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface ITestData
    {
    }

    [DataDefinition]
    public class TestDataProto : ITestData
    {
    }

    [DataDefinition]
    public class TestDataCtor : ITestData
    {
    }

    [DataDefinition]
    public class TestDataRuntime : ITestData
    {
    }
}
