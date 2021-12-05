using Moq;
using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Instanced;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapLoader))]
    public class MapLoader_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestMapLoader()
        {
            var deps = new DependencyCollection();

            var mockResourceManager = new Mock<IResourceManager>();

            var virtualDir = new VirtualWritableDirProvider();
            mockResourceManager.Setup(rm => rm.UserData).Returns(virtualDir);

            deps.Register<IMapLoader, MapLoader>();
            deps.Register<IMapManager, MapManager>();
            deps.Register<IInstancedSerializer, InstancedSerializer>();
            deps.RegisterInstance<IResourceManager>(mockResourceManager.Object);
            deps.BuildGraph();

            var map = new Map(50, 50);

            var mapManager = deps.Resolve<IMapManager>();
            var mapLoader = deps.Resolve<IMapLoader>();

            var mapId = mapManager.RegisterMap(map);

            mapLoader.SaveMap(mapId, new ResourcePath("/Test.sav"));

            mapLoader.LoadMap(mapId, new ResourcePath("/Test.sav"));
        }
    }
}
