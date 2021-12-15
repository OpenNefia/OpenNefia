using Moq;
using NUnit.Framework;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.Maps.Loader
{
    [TestFixture]
    [TestOf(typeof(MapLoader))]
    public class MapLoader_Tests : OpenNefiaUnitTest
    {
        protected override void OverrideIoC()
        {
            base.OverrideIoC();

            var mockResourceManager = new Mock<IResourceManager>();
            var virtualDir = new VirtualWritableDirProvider();
            mockResourceManager.Setup(rm => rm.UserData).Returns(virtualDir);
            IoCManager.RegisterInstance<IResourceManager>(mockResourceManager.Object, overwrite: true);
        }

        [Test]
        public void TestMapLoader()
        {
            var mapManager = IoCManager.Resolve<IMapManager>();
            var mapLoader = IoCManager.Resolve<IMapLoader>();

            var mapId = new MapId(1);
            mapManager.CreateMap(50, 50, mapId);

            mapLoader.SaveMap(mapId, new ResourcePath("/Test.sav"));

            Assert.That(mapManager.MapExists(mapId), Is.True);

            mapManager.UnloadMap(mapId);

            Assert.That(mapManager.MapExists(mapId), Is.False);

            mapLoader.LoadMap(mapId, new ResourcePath("/Test.sav"));

            Assert.That(mapManager.MapExists(mapId), Is.True);
            
            var grid = mapManager.GetMap(mapId);
            Assert.That(grid.Size, Is.EqualTo(new Vector2i(50, 50)));
        }
    }
}
