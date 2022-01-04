using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps
{
    [TestFixture]
    [TestOf(typeof(MapManager))]
    public class MapManager_Tests : OpenNefiaUnitTest
    {
        [SetUp]
        public void Setup()
        {
            IoCManager.Resolve<IMapManagerInternal>().FlushMaps();
            IoCManager.Resolve<IEntityManagerInternal>().FlushEntities();
        }

        [Test]
        public void TestRegisterMap()
        {
            var mapMan = IoCManager.Resolve<IMapManagerInternal>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            mapMan.NextMapId = 2;

            var map = new Map(12, 34);
            var mapId = new MapId(42);
            var mapEnt = entMan.CreateEntityUninitialized(null);

            mapMan.RegisterMap(map, mapId, mapEnt.Uid);

            Assert.That(mapMan.MapIsLoaded(map.Id), Is.True);
            Assert.That(map.MapEntityUid, Is.EqualTo(mapEnt.Uid));
            Assert.That(map.Id, Is.EqualTo(mapId));
            
            // RegisterMap() will not affect the highest map ID. This is
            // an internal function for game save purposes only. It is
            // assumed that HighestMapId is saved as part of the global
            // session data (see IGameSaveSerializer).
            Assert.That(mapMan.NextMapId, Is.EqualTo(2));
        }

        [Test]
        public void TestCreateMap()
        {
            var mapMan = IoCManager.Resolve<IMapManager>();

            var map = mapMan.CreateMap(12, 34);

            Assert.That(mapMan.MapIsLoaded(map.Id), Is.True);
            Assert.That(map.Size, Is.EqualTo(new Vector2i(12, 34)));
            Assert.That(map.Tiles.Length, Is.EqualTo(12 * 34));
        }

        [Test]
        public void TestCreateMap_HighestMapID()
        {
            var mapMan = IoCManager.Resolve<IMapManagerInternal>();

            mapMan.CreateMap(12, 34);
            var map2 = mapMan.CreateMap(12, 34);

            Assert.That(mapMan.NextMapId == (int)map2.Id + 1);
        }

        [Test]
        public void TestUnloadMap_Entities()
        {
            var entMan = IoCManager.Resolve<IEntityManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();

            var map1 = mapMan.CreateMap(50, 50);
            var map2 = mapMan.CreateMap(50, 50);

            var map1ent = entMan.SpawnEntity(null, map1.AtPos(Vector2i.One));
            var map2ent = entMan.SpawnEntity(null, map2.AtPos(Vector2i.One));

            Assert.That(entMan.EntityExists(map1.MapEntityUid), Is.True);
            Assert.That(entMan.EntityExists(map2.MapEntityUid), Is.True);

            mapMan.UnloadMap(map1.Id);

            Assert.That(entMan.EntityExists(map1.MapEntityUid), Is.False);
            Assert.That(entMan.EntityExists(map2.MapEntityUid), Is.True);

            Assert.That(entMan.EntityExists(map1ent.Uid), Is.False);
            Assert.That(entMan.EntityExists(map2ent.Uid), Is.True);

            Assert.That(mapMan.MapIsLoaded(map1.Id), Is.False);
            Assert.That(mapMan.MapIsLoaded(map2.Id), Is.True);
        }
    }
}
