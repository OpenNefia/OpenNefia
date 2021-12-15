using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapBlueprintLoader))]
    public class MapBlueprintLoader_Tests : OpenNefiaUnitTest
    {
        private const string MapData = @"
meta:
  format: 1
  name: test
  author: ruin
  postmapinit: false
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
  protoId: MapDeserializeTest
  components:
  - type: MapDeserializeTest
    foo: 3
  - type: Spatial
";

        private const string Prototype = @"
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
            resourceManager.MountString("/TestMap.yml", MapData);
            resourceManager.MountString("/Prototypes/TestMapEntity.yml", Prototype);

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            protoMan.RegisterType(typeof(EntityPrototype));
            protoMan.RegisterType(typeof(TilePrototype));
            protoMan.LoadDirectory(new ResourcePath("/Prototypes"));

            var tileDefMan = IoCManager.Resolve<ITileDefinitionManagerInternal>();
            tileDefMan.RegisterAll();
        }

        [Test]
        public void TestDataLoadPriority()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

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
