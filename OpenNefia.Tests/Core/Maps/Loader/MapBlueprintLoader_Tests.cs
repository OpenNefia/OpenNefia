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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapBlueprintLoader))]
    public class MapBlueprintLoader_Tests : OpenNefiaUnitTest
    {

        private const string Prototypes = @"
- type: Entity
  id: MapDeserializeTest
  components:
  - type: MapDeserializeTest
    foo: 1
    bar: 2

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
        public void Setup()
        {
            var compFactory = IoCManager.Resolve<IComponentFactory>();
            compFactory.RegisterClass<MapDeserializeTestComponent>();
            compFactory.FinishRegistration();
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var resourceManager = IoCManager.Resolve<IResourceManagerInternal>();
            resourceManager.Initialize(null);
            resourceManager.MountString("/Prototypes/All.yml", Prototypes);

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            protoMan.RegisterType(typeof(EntityPrototype));
            protoMan.RegisterType(typeof(TilePrototype));
            protoMan.LoadDirectory(new ResourcePath("/Prototypes"));

            var tileDefMan = IoCManager.Resolve<ITileDefinitionManagerInternal>();
            tileDefMan.RegisterAll();
        }

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

            var mapLoad = IoCManager.Resolve<IMapBlueprintLoader>();
            var mapId = mapMan.GetFreeMapId();
            Assert.Throws<InvalidDataException>(() => mapLoad.LoadBlueprint(mapId, new ResourcePath("/TestMap1.yml")));
        }

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

            var mapLoad = IoCManager.Resolve<IMapBlueprintLoader>();
            var mapId = mapMan.GetFreeMapId();
            Assert.Throws<InvalidDataException>(() => mapLoad.LoadBlueprint(mapId, new ResourcePath("/TestMap2.yml")));
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

            var mapLoad = IoCManager.Resolve<IMapBlueprintLoader>();
            var mapId = mapMan.GetFreeMapId();
            var map = mapLoad.LoadBlueprint(mapId, new ResourcePath("/TestMap.yml"));

            Assert.That(map, Is.Not.Null);

            var mapEntitySpatial = entMan.GetComponent<SpatialComponent>(map!.MapEntityUid);

            Assert.That(mapEntitySpatial.ChildCount, Is.EqualTo(1));

            var entity = mapEntitySpatial.Children.Single().OwnerUid;
            var c = entMan.GetComponent<MapDeserializeTestComponent>(entity);

            Assert.That(c.Bar, Is.EqualTo(2));
            Assert.That(c.Foo, Is.EqualTo(3));
            Assert.That(c.Baz, Is.EqualTo(-1));

            var entitySpatial = entMan.GetComponent<SpatialComponent>(entity);

            Assert.That(entitySpatial.WorldPosition, Is.EqualTo(new Vector2i(3, 3)));
        }

        [DataDefinition]
        private sealed class MapDeserializeTestComponent : Component
        {
            public override string Name => "MapDeserializeTest";

            [DataField("foo")] public int Foo { get; set; } = -1;
            [DataField("bar")] public int Bar { get; set; } = -1;
            [DataField("baz")] public int Baz { get; set; } = -1;
        }
    }
}
