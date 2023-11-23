using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.SaveGames;
using OpenNefia.Tests;

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

        [Test]
        public void TestMapTransfer_VillagerRevival()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var world = sim.GetEntitySystem<IWorldSystem>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var saveGameMan = sim.Resolve<ISaveGameManager>();

            var sys = sim.GetEntitySystem<IMapTransferSystem>();

            var map1 = sim.CreateMapAndSetActive(10, 10);
            var map2 = sim.CreateMap(10, 10);

            using var save = new TempSaveGameHandle();
            saveGameMan.CurrentSave = save;
            world.State.GameDate = new GameDateTime(512, 1, 1);

            var ent = entGen.SpawnEntity(Protos.Chara.Shopkeeper, map1.AtPos(1, 1))!.Value;
            var entChara = entMan.GetComponent<CharaComponent>(ent);
            entChara.Liveness = CharaLivenessState.VillagerDead;
            entChara.RevivalDate = new GameDateTime(512, 1, 2);

            var player = entGen.SpawnEntity(Protos.Chara.Putit, map1.AtPos(0, 0))!.Value;
            sim.Resolve<IGameSessionManager>().Player = player;
            var playerSpatial = entMan.GetComponent<SpatialComponent>(player);

            Assert.Multiple(() =>
            {
                Assert.That(lookup.EntityQueryInMap<CharaComponent>(map1, includeDead: true).Count(), Is.EqualTo(2));
                Assert.That(lookup.EntityQueryInMap<CharaComponent>(map2, includeDead: true).Count(), Is.EqualTo(0));
            });

            sys.DoMapTransfer(playerSpatial, map2, map2.AtPosEntity(0, 0), MapLoadType.Full, noUnloadPrevious: true);
            sys.DoMapTransfer(playerSpatial, map1, map1.AtPosEntity(0, 0), MapLoadType.Full, noUnloadPrevious: true);

            Assert.Multiple(() =>
            {
                Assert.That(lookup.EntityQueryInMap<CharaComponent>(map1, includeDead: true).Count(), Is.EqualTo(2));
                Assert.That(lookup.EntityQueryInMap<CharaComponent>(map2, includeDead: true).Count(), Is.EqualTo(0));
            });

            entChara = entMan.GetComponent<CharaComponent>(ent);
            Assert.Multiple(() =>
            {
                Assert.That(entMan.IsAlive(ent), Is.False);
                Assert.That(entMan.IsDeadAndBuried(ent), Is.False);
                Assert.That(entChara.Liveness, Is.EqualTo(CharaLivenessState.VillagerDead));
            });

            world.State.GameDate = new GameDateTime(512, 1, 2);

            sys.DoMapTransfer(playerSpatial, map2, map2.AtPosEntity(0, 0), MapLoadType.Full, noUnloadPrevious: true);
            sys.DoMapTransfer(playerSpatial, map1, map1.AtPosEntity(0, 0), MapLoadType.Full, noUnloadPrevious: true);

            entChara = entMan.GetComponent<CharaComponent>(ent);
            Assert.That(entChara.Liveness, Is.EqualTo(CharaLivenessState.Alive));
        }

        [Test]
        public void TestMapTransfer_GlobalTempEntities()
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

            var tempEntity = entMan.SpawnEntity(null, MapCoordinates.Global);
            var nonTempEntity = entMan.SpawnEntity(null, MapCoordinates.Global);

            entMan.GetComponent<MetaDataComponent>(tempEntity).IsMapSavable = false;

            mapTransfer.DoMapTransfer(playerSpatial, map2, map2.AtPosEntity(3, 4), MapLoadType.Full);

            Assert.Multiple(() =>
            {
                Assert.That(entMan.IsAlive(tempEntity), Is.False, "Global temp entity deleted");
                Assert.That(entMan.IsAlive(nonTempEntity), Is.True, "Global entity preserved");
                Assert.That(entMan.GetComponent<MetaDataComponent>(nonTempEntity).IsMapSavable, Is.True, "IsMapSavable");
            });
        }
    }
}
