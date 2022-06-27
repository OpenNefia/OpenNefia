using NUnit.Framework;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.SaveGames;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Maps.Entrances
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(MapTransferSystem))]
    public class MapTransferSystem_Tests
    {
        [Test]
        public void TestMapTransferForPlayer()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var gameSess = sim.Resolve<IGameSessionManager>();
            var saveGameMan = sim.Resolve<ISaveGameManager>();
            var mapLoader = sim.Resolve<IMapLoader>();
            var mapTransfer = sim.GetEntitySystem<IMapTransferSystem>();

            var map1 = sim.CreateMapAndSetActive(10, 10);
            var map2 = mapMan.CreateMap(10, 10);

            using var save = new TempSaveGameHandle();
            saveGameMan.CurrentSave = save;

            var player = entMan.SpawnEntity(null, map1.AtPos(Vector2i.One));
            gameSess.Player = player;
            entMan.EnsureComponent<PlayerComponent>(player);
            var playerSpatial = entMan.GetComponent<SpatialComponent>(player);

            Assert.Multiple(() =>
            {
                Assert.That(mapLoader.MapExistsInSave(map1.Id, save), Is.False);
                Assert.That(mapLoader.MapExistsInSave(map2.Id, save), Is.False);
            });

            var expectedPos = new Vector2i(3, 4);
            mapTransfer.DoMapTransfer(playerSpatial, map2, map2.AtPosEntity(expectedPos), MapLoadType.Full);

            Assert.Multiple(() =>
            {
                Assert.That(mapLoader.MapExistsInSave(map1.Id, save), Is.True, "Map 1 is saved");
                Assert.That(mapLoader.MapExistsInSave(map2.Id, save), Is.False, "Map 2 is not saved");
                Assert.That(mapMan.MapIsLoaded(map1.Id), Is.False, "Map 1 is not loaded");
                Assert.That(mapMan.MapIsLoaded(map2.Id), Is.True, "Map 2 is loaded");
                Assert.That(mapMan.ActiveMap?.Id, Is.EqualTo(map2.Id), "Map 2 is active");
                Assert.That(playerSpatial.MapPosition, Is.EqualTo(map2.AtPos(expectedPos)), "Position was updated");
            });
        }
    }
}
