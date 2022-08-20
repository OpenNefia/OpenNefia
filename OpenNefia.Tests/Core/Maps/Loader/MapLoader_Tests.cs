using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapLoader))]
    public class MapLoader_Tests : OpenNefiaUnitTest
    {
        protected override IEnumerable<Type> ExtraSystemTypes => new[] {
            typeof(EntityLookup),
            typeof(SpatialSystem)
        };

        private static readonly PrototypeId<TilePrototype> TileTestFloorID = new("Test.Floor");
        private static readonly PrototypeId<TilePrototype> TileTestWallID = new("Test.Wall");
        private static readonly PrototypeId<EntityPrototype> TestDeserializeID = new("Test.Deserialize");
        private static readonly PrototypeId<EntityPrototype> TestDeserializeOverrideID = new("Test.DeserializeOverride");
        private static readonly PrototypeId<EntityPrototype> TestImpassibleID = new("Test.Impassible");

        private static readonly string Prototypes = @$"
- type: Entity
  id: {TestDeserializeID}
  components:
  - type: MapDeserializeTest
    foo: 1
    bar: 2
  - type: MapDeserializeTestRemove

- type: Entity
  id: {TestDeserializeOverrideID}
  components:
  - type: MapDeserializeTestOverride
    bar: !type:TestDataProto {{}}

- type: Entity
  id: {TestImpassibleID}
  components:
  - type: Spatial
    isSolid: true
    isOpaque: true

# Required by the engine.
- type: Tile
  id: Empty
  image:
    filepath: /Default.png
  isSolid: false
  isOpaque: false

- type: Tile
  id: {TileTestFloorID}
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
            compFactory.RegisterClass<MapDeserializeTestEntityUidsComponent>();
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

            var mapData1 = @$"meta:
  format: 1
  name: test
  author: ruin
grid: |
  .
tilemap:
  '.': {TileTestWallID}
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

            var mapData2 = @$"meta:
  format: 1
  name: test
  author: ruin
grid: |
  .
tilemap:
  '.': {TileTestFloorID}
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

        /// <summary>
        /// Loaded maps should have tile tangibility recalculated correctly.
        /// </summary>
        [Test]
        public void TestMapLoadTileTangibility()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapData = @$"
meta:
  format: 1
  name: test
  author: ruin
grid: |
  @.
  ..
tilemap:
  '.': {TileTestFloorID}
  '@': {TileTestWallID}
entities:
- uid: 0
  components:
  - type: Map
- uid: 1
  protoId: {TestImpassibleID}
  components:
  - type: Spatial
    parent: 0
    pos: 1,0
";

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.MountString("/TestMap3.yml", mapData);

            var mapLoad = IoCManager.Resolve<IMapLoader>();
            var map = mapLoad.LoadBlueprint(new ResourcePath("/TestMap3.yml"));

            Assert.That(map, Is.Not.Null);

            var wallPos = Vector2i.Zero;
            var floorPos = Vector2i.One;
            var entityPos = new Vector2i(1, 0);

            Assert.Multiple(() =>
            {
                Assert.That(map.CanAccess(wallPos), Is.False, "Wall solid");
                Assert.That(map.CanAccess(floorPos), Is.True, "Floor solid");
                Assert.That(map.CanAccess(entityPos), Is.False, "Entity solid");
                Assert.That(map.CanSeeThrough(wallPos), Is.False, "Wall opaque");
                Assert.That(map.CanSeeThrough(floorPos), Is.True, "Floor opaque");
                Assert.That(map.CanSeeThrough(entityPos), Is.False, "Entity opaque");
            });
        }

        [Test]
        public void TestDataLoadPriority()
        {
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapData = @$"
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
  '@': {TileTestWallID}
  '.': {TileTestFloorID}
entities:
- uid: 0
  components:
  - type: Map
- uid: 1
  protoId: {TestDeserializeID}
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

            var entity = mapEntitySpatial.Children.Single().Owner;
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

            var entityUid = entMan.SpawnEntity(TestDeserializeID, map.AtPos(Vector2i.One));

            var c = entMan.GetComponent<MapDeserializeTestComponent>(entityUid);
            c.Foo = 999;
            c.Bar = 9999;
            c.Baz = 99999;

            entMan.RemoveComponent<MapDeserializeTestRemoveComponent>(entityUid);
            entMan.EnsureComponent<MapDeserializeTestAddComponent>(entityUid);

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

            c = entMan.GetComponent<MapDeserializeTestComponent>(entityUid);

            Assert.That(c.Foo, Is.EqualTo(999));
            Assert.That(c.Bar, Is.EqualTo(9999));
            Assert.That(c.Baz, Is.EqualTo(99999));

            Assert.That(entMan.HasComponent<MapDeserializeTestAddComponent>(entityUid), Is.True);
            Assert.That(entMan.HasComponent<MapDeserializeTestRemoveComponent>(entityUid), Is.False);
        }

        /// <summary>
        /// Tests that the full state of data definition properties are captured on map save,
        /// including defaults set by the prototype/constructors.
        /// </summary>
        [Test]
        public void TestMapLoadDataDefinitions()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(50, 50);
            var mapId = map.Id;

            var entityUid = entMan.SpawnEntity(TestDeserializeOverrideID, map.AtPos(Vector2i.One));

            var c = entMan.GetComponent<MapDeserializeTestOverrideComponent>(entityUid);
            c.Foo = new TestDataRuntime();

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            c = entMan.GetComponent<MapDeserializeTestOverrideComponent>(entityUid);

            Assert.Multiple(() =>
            {
                Assert.That(c.Foo, Is.TypeOf(typeof(TestDataRuntime)));
                Assert.That(c.Bar, Is.TypeOf(typeof(TestDataProto)));
                Assert.That(c.Baz, Is.TypeOf(typeof(TestDataCtor)));
            });
        }

        /// <summary>
        /// Tests that tile memory is saved and restored properly.
        /// </summary>
        [Test]
        public void TestMapLoadTileMemory()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            var tileDefMan = IoCManager.Resolve<ITileDefinitionManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(5, 5);
            var mapId = map.Id;

            map.SetTileMemory(Vector2i.One, TileTestWallID);

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            Assert.Multiple(() =>
            {
                Assert.That(map.GetTileMemory(Vector2i.Zero)!.Value.Tile, Is.EqualTo(Tile.Empty));

                var tileAtOne = map.GetTileMemory(Vector2i.One)!.Value.Tile;
                Assert.That(tileDefMan[tileAtOne.Type].GetStrongID(), Is.EqualTo(TileTestWallID));
            });
        }

        /// <summary>
        /// Tests that map object memory is saved and restored properly.
        /// </summary>
        [Test]
        public void TestMapLoadObjectMemory()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            var tileDefMan = IoCManager.Resolve<ITileDefinitionManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(5, 5);
            var mapId = map.Id;
            var coords = map.AtPos(Vector2i.One);

            var ent = entMan.SpawnEntity(TestDeserializeOverrideID, coords);

            var memIndex = 1;
            map.MapObjectMemory._allMemory[memIndex] = new MapObjectMemory()
            {
                Index = memIndex,
                ObjectUid = ent,
                Coords = coords
            };

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            Assert.Multiple(() =>
            {
                Assert.That(map.MapObjectMemory.AllMemory.Count, Is.EqualTo(1));
                Assert.That(map.MapObjectMemory.AllMemory.First().Value.ObjectUid, Is.EqualTo(ent));
                Assert.That(map.MapObjectMemory.AllMemory.First().Value.Coords, Is.EqualTo(coords));
            });
        }

        /// <summary>
        /// Tests that tile in sight data is saved and restored properly.
        /// </summary>
        [Test]
        public void TestMapLoadInSight()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            var tileDefMan = IoCManager.Resolve<ITileDefinitionManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(5, 5);
            var mapId = map.Id;

            map.InSight[1, 1] = 45;
            map.LastSightId = 100;

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            Assert.Multiple(() =>
            {
                Assert.That(map.InSight[0, 0], Is.Zero);
                Assert.That(map.InSight[1, 1], Is.EqualTo(45));
                Assert.That(map.LastSightId, Is.EqualTo(100));
            });
        }

        /// <summary>
        /// Tests that EntityUids are *always* saved on a full map save.
        /// </summary>
        [Test]
        public void TestMapSaveFullEntityUids()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            var tileDefMan = IoCManager.Resolve<ITileDefinitionManager>();

            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var map = mapMan.CreateMap(5, 5);
            var mapId = map.Id;
            var coords = map.AtPos(Vector2i.One);

            var ent = entMan.SpawnEntity(TestDeserializeOverrideID, coords);
            var testComp = entMan.EnsureComponent<MapDeserializeTestEntityUidsComponent>(ent);

            testComp.Uids.Add(ent);

            // Add some UIDs that aren't valid.
            // The idea is that these UIDs could still be valid in another map
            // that's currently unloaded, thus they should still get saved.
            // It would be the entity system's responsibility to check validity in this case.
            testComp.Uids.Add(new EntityUid(100));
            testComp.Uids.Add(new EntityUid(999));

            using var save = new TempSaveGameHandle();

            mapLoader.SaveMap(mapId, save);
            mapMan.UnloadMap(mapId);
            map = mapLoader.LoadMap(mapId, save);

            testComp = entMan.GetComponent<MapDeserializeTestEntityUidsComponent>(ent);
            Assert.That(testComp.Uids, Is.EquivalentTo(new EntityUid[] { ent, new EntityUid(100), new EntityUid(999) }));
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

        [DataDefinition]
        private sealed class MapDeserializeTestEntityUidsComponent : Component
        {
            public override string Name => "MapDeserializeTestEntityUids";

            [DataField]
            public List<EntityUid> Uids { get; set; } = new();
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
