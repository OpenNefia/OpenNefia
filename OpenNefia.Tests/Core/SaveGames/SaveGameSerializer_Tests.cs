using Nett.Collections;
using NUnit.Framework;
using OpenNefia.Core.Areas;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.SaveGames
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SaveGameSerializer))]
    public class SaveGameSerializer_Tests : OpenNefiaUnitTest
    {
        [Test]
        public void TestSerializeOnEntitySystem()
        {
            // Arrange.
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<SaveGameTestSystem>();
                })
                .RegisterDataDefinitionTypes(types => types.Add(typeof(TestSaveData)))
                .InitializeInstance();

            var save = new TempSaveGameHandle();
            var saveSerMan = sim.Resolve<ISaveGameSerializerInternal>();

            // Act.
            var sys = sim.GetEntitySystem<SaveGameTestSystem>();

            sys.Data.Foo = 42;
            sys.Data.Bar = new List<string> { "hoge" };
            saveSerMan.SaveGlobalData(save);

            Assert.That(save.Files.Exists(new ResourcePath("/global.yml")), Is.True);

            sys.Data.Foo = 999;
            sys.Data.Bar.Clear();
            saveSerMan.LoadGlobalData(save);

            // Check that the save data was loaded into the entity system
            Assert.That(sys.Data.Foo, Is.EqualTo(42));
            Assert.That(sys.Data.Bar, Is.EquivalentTo(new[] { "hoge" }));
        }

        /// <summary>
        /// Tests that engine-critical things like the active player/map are
        /// saved and restored properly.
        /// </summary>
        [Test]
        public void TestSerializeGameSession()
        {
            // Arrange.
            var sim = GameSimulation
                .NewSimulation()
                .InitializeInstance();

            var save = new TempSaveGameHandle();
            var saveSerMan = sim.Resolve<ISaveGameSerializerInternal>();
            var entMan = sim.Resolve<IEntityManagerInternal>();
            var mapMan = sim.Resolve<IMapManagerInternal>();
            var mapLoader = sim.Resolve<IMapLoader>();
            var areaMan = sim.Resolve<IAreaManagerInternal>();
            var sessMan = sim.Resolve<IGameSessionManager>();

            var map = sim.CreateMapAndSetActive(50, 50);
            var mapEnt = map.MapEntityUid;

            // Act.
            mapMan.CreateMap(50, 50);
            var player = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            sessMan.Player = player;
            var area = areaMan.CreateArea(null);
            var areaId = area.Id;
            var areaEnt = area.AreaEntityUid;

            // Save these values.
            var nextMapId = mapMan.NextMapId;
            var nextAreaId = areaMan.NextAreaId;
            var nextEntId = entMan.NextEntityUid;
            var activeMapId = sim.ActiveMap!.Id;
            var playerUid = sessMan.Player;

            saveSerMan.SaveGame(save);

            // Allocate some more IDs.
            var nextMap = mapMan.CreateMap(50, 50);
            areaMan.CreateArea(null);
            var nextPlayer = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            mapMan.SetActiveMap(nextMap.Id);
            sessMan.Player = nextPlayer;

            saveSerMan.LoadGame(save);

            // Check that the highest free entity/map IDs were restored.
            Assert.That(mapMan.NextMapId, Is.EqualTo(nextMapId));
            Assert.That(areaMan.NextAreaId, Is.EqualTo(nextAreaId));
            Assert.That(entMan.NextEntityUid, Is.EqualTo(nextEntId));
            Assert.That(mapMan.ActiveMap?.Id, Is.EqualTo(activeMapId));
            Assert.That(sessMan.Player, Is.EqualTo(playerUid));

            Assert.That(mapLoader.MapExistsInSave(MapId.Global, save), Is.True);
            Assert.That(mapMan.MapIsLoaded(MapId.Global), Is.True);
            Assert.That(mapMan.MapIsLoaded(activeMapId), Is.True);
            Assert.That(entMan.EntityExists(mapEnt), Is.True);

            Assert.That(areaMan.AreaExists(areaId), Is.True);
            Assert.That(entMan.EntityExists(areaEnt), Is.True);
        }

        /// <summary>
        /// Tests that global save data is reset after resetting the game state.
        /// </summary>
        [Test]
        public void TestSaveDataReset()
        {
            // Arrange.
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<SaveGameTestSystem>();
                })
                .RegisterDataDefinitionTypes(types => types.Add(typeof(TestSaveData)))
                .InitializeInstance();

            var save = new TempSaveGameHandle();
            var saveSerMan = sim.Resolve<ISaveGameSerializerInternal>();

            // Act.
            var sys = sim.GetEntitySystem<SaveGameTestSystem>();

            sys.Data.Foo = 42;
            sys.Data.Bar = new List<string> { "hoge" };
            sys.Data.Hoge = new TestSaveDataProperty() { Foo = 1 };
            sys.Data.Piyo = new Formula("2 + 2");

            saveSerMan.ResetGameState();

            Assert.That(sys.Data.Foo, Is.EqualTo(-1));
            Assert.That(sys.Data.Bar, Is.EquivalentTo(new[] { "baz", "quux" }));
            Assert.That(sys.Data.Hoge, Is.Null);
            Assert.That(sys.Data.Piyo, Is.Null);
        }

        /// <summary>
        /// Tests that null references to save data can be restored.
        /// </summary>
        [Test]
        public void TestSaveDataNullable()
        {
            // Arrange.
            var sim = GameSimulation
                .NewSimulation()
                .RegisterEntitySystems(factory =>
                {
                    factory.LoadExtraSystemType<SaveGameTestSystem>();
                })
                .RegisterDataDefinitionTypes(types => types.Add(typeof(TestSaveData)))
                .InitializeInstance();

            var save = new TempSaveGameHandle();
            var saveSerMan = sim.Resolve<ISaveGameSerializerInternal>();
            var entMan = sim.Resolve<IEntityManagerInternal>();
            var mapMan = sim.Resolve<IMapManagerInternal>();
            var mapLoader = sim.Resolve<IMapLoader>();
            var areaMan = sim.Resolve<IAreaManagerInternal>();
            var sessMan = sim.Resolve<IGameSessionManager>();

            var map = sim.CreateMapAndSetActive(50, 50);
            var mapEnt = map.MapEntityUid;

            // Act.
            mapMan.CreateMap(50, 50);
            var player = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            sessMan.Player = player;

            var sys = sim.GetEntitySystem<SaveGameTestSystem>();
            sys.DataNullable = null;

            saveSerMan.SaveGame(save);
            saveSerMan.LoadGame(save);

            Assert.That(sys.Data.Foo, Is.EqualTo(-1));
            Assert.That(sys.Data.Bar, Is.EquivalentTo(new[] { "baz", "quux" }));
            Assert.That(sys.Data.Hoge, Is.Null);
            Assert.That(sys.Data.Piyo, Is.Null);

            sys.Data = null!;

            saveSerMan.SaveGame(save);
            Assert.Throws<ArgumentException>(() =>
            {
                saveSerMan.LoadGame(save);
            });
        }
    }

    [Reflect(false)]
    public class SaveGameTestSystem : EntitySystem
    {
        [RegisterSaveData("SaveGameTestSystem.Data")]
        public TestSaveData Data { get; set; } = new();

        [RegisterSaveData("SaveGameTestSystem.DataNullable")]
        public TestSaveData? DataNullable { get; set; } = new();
    }

    [DataDefinition]
    public class TestSaveDataProperty
    {
        [DataField]
        public int Foo { get; set; } = 42;
    }

    [DataDefinition]
    public class TestSaveData
    {
        [DataField(required: true)]
        public int Foo { get; set; } = -1;

        [DataField(required: true)]
        public List<string> Bar = new() { "baz", "quux" };

        // test reference nullable property
        [DataField]
        public TestSaveDataProperty? Hoge { get; set; } = null;

        // test struct nullable property
        [DataField]
        public Formula? Piyo { get; set; } = null;
    }
}
